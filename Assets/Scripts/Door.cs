using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    public Direction exitDirection;

    // These get assigned automatically by roomController (recommended)
    [HideInInspector] public roomController room;
    [HideInInspector] public RoomLoader loader;
    [HideInInspector] public MonoBehaviour destinationProviderComponent;

    private IDestinationProvider Provider => (IDestinationProvider)destinationProviderComponent;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log($"[Door] Entered {room.roomId} via {exitDirection}");

        if (room == null || loader == null || destinationProviderComponent == null) return;

        string nextRoomId = Provider.GetDestinationRoomId(room.roomId, exitDirection);
        loader.LoadRoom(nextRoomId, Opposite(exitDirection));
    }

    private Direction Opposite(Direction d) => d switch
    {
        Direction.N => Direction.S,
        Direction.S => Direction.N,
        Direction.E => Direction.W,
        Direction.W => Direction.E,
        _ => Direction.S
    };
}