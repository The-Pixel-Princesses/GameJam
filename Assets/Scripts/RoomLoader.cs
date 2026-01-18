using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomLoader : MonoBehaviour
{

    [Header("Debug")]
    public bool enableDebugRoomJump = true;

    [Tooltip("Rooms mapped to keys 1–9 (index 0 = key 1, index 8 = key 9)")]
    public List<string> debugRoomOrder = new();

    // roomId -> simple persistent state
    private readonly Dictionary<string, RoomState> _stateByRoomId = new();

    public RoomState GetRoomState(string roomId)
    {
        if (!_stateByRoomId.TryGetValue(roomId, out var state))
        {
            state = new RoomState();
            _stateByRoomId[roomId] = state;
        }
        return state;
    }
    [Header("Scene Grid")]
    public Transform gridRoot;

    [Header("Player")]
    public Transform player;

    [Header("Room Prefabs (root must have roomController)")]
    public List<GameObject> roomPrefabs;

    [Header("Start Room")]
    public string startRoomId = "Start";
    public Direction startEnteredFrom = Direction.S;

    [Header("Spawn Tuning")]
    public float doorInwardOffset = 1.0f;
    public int floorSearchRadius = 10;
    public Vector3 floorCellCenterOffset = new Vector3(0.5f, 0.5f, 0f);

    private readonly Dictionary<string, GameObject> _prefabById = new();
    private readonly Dictionary<string, roomController> _instanceByRoomId = new();

    private string _currentRoomId = "";
    private roomController _currentRoom;

    private void Awake()
    {
        _prefabById.Clear();

        foreach (var prefab in roomPrefabs)
        {
            if (prefab == null) continue;

            var rc = prefab.GetComponent<roomController>();
            if (rc == null)
            {
                Debug.LogError($"[RoomLoader] Prefab '{prefab.name}' missing roomController on ROOT.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(rc.roomId))
            {
                Debug.LogError($"[RoomLoader] Prefab '{prefab.name}' has empty roomId.");
                continue;
            }

            _prefabById[rc.roomId] = prefab;
        }
    }

    private void Start()
    {
        // Always start by generating the Start room prefab (if nothing is loaded yet)
        if (string.IsNullOrEmpty(_currentRoomId))
        {
            // Optional safety: hide anything manually under Grid (in case you forgot to delete it)
            HideAllRoomsUnderGrid();

            if (!_prefabById.ContainsKey(startRoomId))
            {
                Debug.LogError($"[RoomLoader] startRoomId '{startRoomId}' not found in RoomLoader.roomPrefabs list.");
                return;
            }

            LoadRoom(startRoomId, startEnteredFrom);
        }
    }

    public string GetCurrentRoomId() => _currentRoomId;

    public void LoadRoom(string roomId) => LoadRoom(roomId, Direction.S);

    public void LoadRoom(string roomId, Direction enteredFrom)
    {
        if (gridRoot == null)
        {
            Debug.LogError("[RoomLoader] gridRoot not assigned.");
            return;
        }

        // Hard guarantee: only one room visible at a time
        HideAllRoomsUnderGrid();

        // Get or create target room instance
        if (!_instanceByRoomId.TryGetValue(roomId, out var targetRoom) || targetRoom == null)
        {
            if (!_prefabById.TryGetValue(roomId, out var prefab) || prefab == null)
            {
                Debug.LogError($"[RoomLoader] No prefab registered for roomId '{roomId}'.");
                return;
            }

            GameObject instance = Instantiate(prefab, gridRoot);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            targetRoom = instance.GetComponent<roomController>();
            if (targetRoom == null)
            {
                Debug.LogError($"[RoomLoader] Spawned '{prefab.name}' but missing roomController on root.");
                Destroy(instance);
                return;
            }

            _instanceByRoomId[roomId] = targetRoom;
        }

        // Show target room
        targetRoom.gameObject.SetActive(true);

        // Track current
        _currentRoom = targetRoom;
        _currentRoomId = roomId;

        // Move player: spawn near the door we entered from, then snap to nearest floor tile
        if (player != null)
        {
            Vector3 desired = ComputeSpawnNearDoor(_currentRoom, enteredFrom);
            Vector3 snapped = SnapToNearestFloor(_currentRoom.Floor, desired);
            player.position = new Vector3(snapped.x, snapped.y, player.position.z);
        }

        Debug.Log($"[RoomLoader] Switched to '{roomId}'. Active instances={_instanceByRoomId.Count}");
    }

    private void HideAllRoomsUnderGrid()
    {
        if (gridRoot == null) return;

        for (int i = 0; i < gridRoot.childCount; i++)
        {
            gridRoot.GetChild(i).gameObject.SetActive(false);
        }
    }

    private Vector3 ComputeSpawnNearDoor(roomController room, Direction enteredFrom)
    {
        var door = room.GetDoorTransform(enteredFrom);
        Vector3 basePos = door ? door.position : room.transform.position;

        Vector2 inward = enteredFrom switch
        {
            Direction.N => Vector2.down,
            Direction.S => Vector2.up,
            Direction.E => Vector2.left,
            Direction.W => Vector2.right,
            _ => Vector2.up
        };

        return basePos + (Vector3)(inward * doorInwardOffset);
    }

    private Vector3 SnapToNearestFloor(Tilemap floor, Vector3 desiredWorld)
    {
        if (floor == null) return desiredWorld;

        Vector3Int startCell = floor.WorldToCell(desiredWorld);

        for (int r = 0; r <= floorSearchRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    var cell = new Vector3Int(startCell.x + dx, startCell.y + dy, startCell.z);
                    if (floor.HasTile(cell))
                        return floor.CellToWorld(cell) + floorCellCenterOffset;
                }
            }
        }

        return desiredWorld;
    }
private void Update()
{
    if (!enableDebugRoomJump) return;

    // Keys 1–9
    for (int i = 0; i < debugRoomOrder.Count && i < 9; i++)
    {
        KeyCode key = KeyCode.Alpha1 + i;

        if (Input.GetKeyDown(key))
        {
            string roomId = debugRoomOrder[i];

            if (string.IsNullOrWhiteSpace(roomId))
            {
                Debug.LogWarning($"[RoomLoader][Debug] No roomId assigned for key {i + 1}");
                continue;
            }

            if (!_prefabById.ContainsKey(roomId))
            {
                Debug.LogError($"[RoomLoader][Debug] Room '{roomId}' not found in RoomLoader.roomPrefabs");
                continue;
            }

            Debug.Log($"[RoomLoader][Debug] Jumping to room '{roomId}' via key {i + 1}");
            LoadRoom(roomId, Direction.S); // default enter-from
        }
    }
}

}
