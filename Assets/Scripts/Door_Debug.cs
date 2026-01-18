using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door_Debug : MonoBehaviour
{
    public Direction exitDirection;

    private roomController _room;
    private RoomLoader _loader;
    private IDestinationProvider _router;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Initialize(roomController room, RoomLoader loader, MonoBehaviour routerComponent)
    {
        _room = room;
        _loader = loader;
        _router = routerComponent as IDestinationProvider;

        if (_loader == null) _loader = FindFirstObjectByType<RoomLoader>();
        if (_router == null) _router = FindFirstObjectByType<MansionRouter>();

        Debug.Log($"[Door] Initialized '{name}' in room '{_room?.roomId}' exit={exitDirection} loader={_loader != null} router={_router != null}");
    }

    private void Awake()
    {
        // Safety: ensure trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        // Failsafe auto-find in case Initialize() wasn't called (still works)
        if (_room == null) _room = GetComponentInParent<roomController>();
        if (_loader == null) _loader = FindFirstObjectByType<RoomLoader>();
        if (_router == null) _router = FindFirstObjectByType<MansionRouter>();

        if (_room == null)
            Debug.LogError($"[Door] No roomController found in parents for door '{name}'.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_room == null || _loader == null || _router == null)
        {
            Debug.LogError($"[Door] Missing refs on '{name}': room={_room != null}, loader={_loader != null}, router={_router != null}");
            return;
        }

        string nextRoomId = _router.GetDestinationRoomId(_room.roomId, exitDirection);
        Debug.Log($"[Door] { _room.roomId } --{ exitDirection }--> { nextRoomId }");

        _loader.LoadRoom(nextRoomId, Opposite(exitDirection));
    }

    private static Direction Opposite(Direction d) => d switch
    {
        Direction.N => Direction.S,
        Direction.S => Direction.N,
        Direction.E => Direction.W,
        Direction.W => Direction.E,
        _ => Direction.S
    };
}
