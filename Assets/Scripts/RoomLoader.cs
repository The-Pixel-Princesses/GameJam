using System.Collections.Generic;
using UnityEngine;

public class RoomLoader : MonoBehaviour
{

    [Header("Scene Grid")]
    public Transform gridRoot;

    [Header("Player")]
    public Transform player;

    [Header("Room Prefabs (drag prefab GameObjects here)")]
    public List<GameObject> roomPrefabs; // drag prefabs here

    [Header("Debug Clamp (temporary)")]
    public bool clampPlayerToBounds = true;
    public Vector2 clampMin = new Vector2(-8f, -4f);
    public Vector2 clampMax = new Vector2(8f, 4f);

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
    LogPlayerAndCamera("LoadRoom BEGIN");

    // Destroy old room
    if (_currentRoom != null)
    {
        Debug.Log($"[RoomLoader] Destroying old room '{_currentRoom.roomId}'");
        Destroy(_currentRoom.gameObject);
        _currentRoom = null;
    }

    // IMPORTANT: prefab is defined HERE
    if (!_prefabById.TryGetValue(roomId, out var prefab) || prefab == null)
    {
        Debug.LogError($"[RoomLoader] No room prefab registered for roomId '{roomId}'.");
        return;
    }

    Debug.Log($"[RoomLoader] Loading prefab '{prefab.name}'");

    // Instantiate AFTER prefab exists
    GameObject instance = Instantiate(prefab, gridRoot);

    instance.transform.localPosition = Vector3.zero;
    instance.transform.localRotation = Quaternion.identity;
    instance.transform.localScale = Vector3.one;

    Debug.Log($"[RoomLoader] Spawned instance '{instance.name}' under '{gridRoot.name}'");

    _currentRoom = instance.GetComponent<roomController>();
    if (_currentRoom == null)
    {
        Debug.LogError($"[RoomLoader] Spawned room '{prefab.name}' but no roomController found.");
        Destroy(instance);
        return;
    }

    // Player placement debug
    var spawn = _currentRoom.GetEntryPoint(enteredFrom);
Vector3 target = spawn ? spawn.position : _currentRoom.transform.position;

if (clampPlayerToBounds)
{
    target.x = Mathf.Clamp(target.x, clampMin.x, clampMax.x);
    target.y = Mathf.Clamp(target.y, clampMin.y, clampMax.y);
}

player.position = new Vector3(target.x, target.y, player.position.z);

    Debug.Log($"[RoomLoader] Spawn point = {(spawn ? spawn.name : "NULL")}");

    LogPlayerAndCamera("Before Player Move");

    if (spawn != null && player != null)
        player.position = new Vector3(spawn.position.x, spawn.position.y, player.position.z);

    LogPlayerAndCamera("After Player Move");

    StartCoroutine(LogNextFrame());
}

private System.Collections.IEnumerator LogNextFrame()
{
    yield return null; // wait 1 frame
    LogPlayerAndCamera("1 Frame Later");
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

private void LogPlayerAndCamera(string tag)
{
    var cam = Camera.main;
    Vector3 p = player ? player.position : new Vector3(float.NaN, float.NaN, float.NaN);
    Vector3 c = cam ? cam.transform.position : new Vector3(float.NaN, float.NaN, float.NaN);

    string pz = player ? player.position.z.ToString("F3") : "null";
    string cz = cam ? cam.transform.position.z.ToString("F3") : "null";

    Debug.Log(
        $"[{tag}] " +
        $"Player={(player ? player.name : "NULL")} pos={p} z={pz} | " +
        $"Cam={(cam ? cam.name : "NULL")} pos={c} z={cz}"
    );
}

//     void Start()
// {
//     // // If a room already exists in the scene, initialize it
//     // var existingRoom = FindFirstObjectByType<roomController>();
//     // if (existingRoom != null)
//     // {
//     //     _currentRoom = existingRoom;

//     //     var state = GetRoomState(existingRoom.roomId);
//     //     existingRoom.ApplyState(state);

//     //     // Optional: spawn player at EnterFromS for the first room
//     //     var spawn = existingRoom.enterFromS;
//     //     if (spawn != null && player != null)
//     //         player.position = spawn.position;
//     }
// }

}