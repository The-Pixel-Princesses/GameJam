using System.Collections.Generic;
using UnityEngine;

public enum Direction { N, E, S, W }

public class roomController : MonoBehaviour
{
    [Header("Room Identity")]
    public string roomId; // e.g. "Start", "LibraryA"

    [Header("Entry Spawn Points")]
    public Transform enterFromN;
    public Transform enterFromE;
    public Transform enterFromS;
    public Transform enterFromW;

    [Header("Optional groups (for organization)")]
    public GameObject doorsRoot;
    public GameObject torchesRoot;
    public GameObject chestsRoot;

    public GameObject keyItemPrefab;

    // You can expand later to store torch/chest components, etc.
    public Transform GetEntryPoint(Direction enteredFrom)
    {
        return enteredFrom switch
        {
            Direction.N => enterFromN,
            Direction.E => enterFromE,
            Direction.S => enterFromS,
            Direction.W => enterFromW,
            _ => enterFromS
        };
    }

    void Awake()
{
        // Wire chests to this room
    foreach (var chest in GetComponentsInChildren<Chest>(true))
    {
        chest.room = this;
    }

    var loader = FindFirstObjectByType<RoomLoader>();
    var router = FindFirstObjectByType<MansionRouter>(); // or DebugDestinationProvider for now

    foreach (var door in GetComponentsInChildren<Door>(true))
    {
        door.room = this;
        door.loader = loader;
        door.destinationProviderComponent = router;
    }

    if (enterFromN == null || enterFromE == null || enterFromS == null || enterFromW == null)
        Debug.LogWarning($"[roomController] Missing entry spawn points in room '{roomId}' on '{gameObject.name}'.");
}

public void ApplyState(RoomState state)
{
    var chests = chestsRoot.GetComponentsInChildren<Chest>(true);

    if (chests.Length != 4)
    {
        Debug.LogError($"[Room] {roomId} expected 4 chests, found {chests.Length}");
        return;
    }

    // Assign randomized IDs ONCE
    if (state.chestIdByIndex == null || state.chestIdByIndex.Length != 4)
    {
        state.chestIdByIndex = new int[] { 0, 1, 2, 3 };
        Shuffle(state.chestIdByIndex);
    }

    for (int i = 0; i < 4; i++)
    {
        chests[i].room = this;
        chests[i].chestId = state.chestIdByIndex[i];
    }

    // Pick real chest slot ONCE
    if (state.realChestIndex < 0 || state.realChestIndex > 3)
        state.realChestIndex = Random.Range(0, 4);

    for (int i = 0; i < 4; i++)
        chests[i].isMimic = (i != state.realChestIndex);
}

private void Shuffle(int[] arr)
{
    for (int i = arr.Length - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        (arr[i], arr[j]) = (arr[j], arr[i]);
    }
}

public void OnRealChestOpened()
{
    // Get the persistent state for THIS room from the loader
    var loader = FindFirstObjectByType<RoomLoader>();
    if (loader == null)
    {
        Debug.LogError("[Room] No RoomLoader found.");
        return;
    }

    var state = loader.GetRoomState(roomId);
    if (state.keyCollected)
    {
        Debug.Log("[Room] Key already collected here.");
        return;
    }

    state.keyCollected = true;
    Debug.Log($"[Room] Key collected in room {roomId}");
}


}