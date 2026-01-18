using System.Collections.Generic;
using UnityEngine;

public class RoomLoader : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Room Prefabs (drag prefab GameObjects here)")]
    public List<GameObject> roomPrefabs; // drag prefabs here

    private readonly Dictionary<string, GameObject> _prefabById = new();
    private readonly Dictionary<string, RoomState> _stateByRoomId = new();

    private roomController _currentRoom;

    void Awake()
    {
        // Build lookup: roomId -> prefab GameObject
        foreach (var prefab in roomPrefabs)
        {
            if (prefab == null) continue;

            var rc = prefab.GetComponent<roomController>();
            if (rc == null)
            {
                Debug.LogError($"Room prefab '{prefab.name}' is missing roomController on the ROOT object.");
                continue;
            }

            if (string.IsNullOrEmpty(rc.roomId))
            {
                Debug.LogError($"Room prefab '{prefab.name}' has an empty roomId on roomController.");
                continue;
            }

            _prefabById[rc.roomId] = prefab;
        }
    }

public void LoadRoom(string roomId, Direction enteredFrom)
{
    // Destroy old room
    if (_currentRoom != null)
        Destroy(_currentRoom.gameObject);

    // Find prefab
    if (!_prefabById.TryGetValue(roomId, out var prefab))
    {
        Debug.LogError($"No room prefab registered for roomId '{roomId}'. Add it to RoomLoader.roomPrefabs.");
        return;
    }

    // Spawn new room
    GameObject instance = Instantiate(prefab);
    _currentRoom = instance.GetComponent<roomController>();

    if (_currentRoom == null)
    {
        Debug.LogError($"Spawned room '{prefab.name}' but it has no roomController on the root.");
        Destroy(instance);
        return;
    }

    // Ensure state exists
    if (!_stateByRoomId.TryGetValue(roomId, out var state))
    {
        state = new RoomState();
        _stateByRoomId[roomId] = state;
    }

    // Apply state (sets chest ids, mimic vs real, torches later, etc.)
    _currentRoom.ApplyState(state);

    // Place player
    var spawn = _currentRoom.GetEntryPoint(enteredFrom);
    if (spawn != null && player != null)
        player.position = spawn.position;
}

public RoomState GetRoomState(string roomId)
{
    if (!_stateByRoomId.TryGetValue(roomId, out var state))
    {
        state = new RoomState();
        _stateByRoomId[roomId] = state;
    }
    return state;
}

    public string GetCurrentRoomId() => _currentRoom != null ? _currentRoom.roomId : "";

    void Start()
{
    // If a room already exists in the scene, initialize it
    var existingRoom = FindFirstObjectByType<roomController>();
    if (existingRoom != null)
    {
        _currentRoom = existingRoom;

        var state = GetRoomState(existingRoom.roomId);
        existingRoom.ApplyState(state);

        // Optional: spawn player at EnterFromS for the first room
        var spawn = existingRoom.enterFromS;
        if (spawn != null && player != null)
            player.position = spawn.position;
    }
}

}