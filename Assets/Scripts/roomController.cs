using UnityEngine;
using UnityEngine.Tilemaps;

public class roomController : MonoBehaviour
{
    [Header("Room Identity")]
    public string roomId;

    [Header("Tilemap Names (adjust if yours differ)")]
    public string floorTilemapName = "Tilemap Floor";

    [Header("Door Object Names (must exist as children somewhere in the prefab)")]
    public string doorNName = "Door_N";
    public string doorEName = "Door_E";
    public string doorSName = "Door_S";
    public string doorWName = "Door_W";

    public Tilemap Floor { get; private set; }

    public GameObject keyItemPrefab;

    private Transform _doorN, _doorE, _doorS, _doorW;

    private void Awake()
    {
        // Cache doors (by name)
        _doorN = FindDeepChild(transform, doorNName);
        _doorE = FindDeepChild(transform, doorEName);
        _doorS = FindDeepChild(transform, doorSName);
        _doorW = FindDeepChild(transform, doorWName);

        // Cache floor tilemap (by name)
        var floorTr = FindDeepChild(transform, floorTilemapName);
        if (floorTr != null) Floor = floorTr.GetComponent<Tilemap>();

        if (Floor == null)
            Debug.LogWarning($"[RoomController] Floor Tilemap not found in '{name}'. Expected child named '{floorTilemapName}'.");
    }

    public Transform GetDoorTransform(Direction side)
    {
        return side switch
        {
            Direction.N => _doorN,
            Direction.E => _doorE,
            Direction.S => _doorS,
            Direction.W => _doorW,
            _ => null
        };
    }

    // Finds a child recursively by name (works for nested hierarchies)
    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;

        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            var result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }

private void Start()
{
    var loader = FindFirstObjectByType<RoomLoader>();
    var router = FindFirstObjectByType<MansionRouter>(); // or your router type

    // Wire doors
    foreach (var door in GetComponentsInChildren<Door_Debug>(true))
        door.Initialize(this, loader, router);

    // Wire chests
    var chests = GetComponentsInChildren<Chest>(true);

    foreach (var chest in chests)
        chest.Initialize(this, loader);

    // Guarantee: exactly 1 real chest per room (chosen once)
    if (loader != null)
    {
        var state = loader.GetRoomState(roomId);

        // Pick once
        if (state.realChestIndex < 0 || state.realChestIndex >= chests.Length)
            state.realChestIndex = Random.Range(0, chests.Length);

        for (int i = 0; i < chests.Length; i++)
            chests[i].isMimic = (i != state.realChestIndex);

        Debug.Log($"[roomController] Room '{roomId}' realChestIndex={state.realChestIndex} (total chests={chests.Length})");
    }
}

}
