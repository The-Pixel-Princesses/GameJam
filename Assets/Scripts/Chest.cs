using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour
{
    [Header("Identity (unique per room)")]
    public int chestId;

    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Interaction")]
    public Transform player;
    public float interactDistance = 3f;

    [Header("Assigned by roomController")]
    [HideInInspector] public roomController room;
    [HideInInspector] public bool isMimic = true;
    public Transform itemSpawnPoint;
    private bool opened;
    private SpriteRenderer sr;
    private Camera mainCam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        sr.sprite = closedSprite;
    }

    void Update()
    {
        if (opened) return;
        if (Mouse.current == null || mainCam == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
            if (hit == null) return;

            if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

            if (player != null && Vector2.Distance(player.position, transform.position) > interactDistance)
                return;

            Interact();
        }
    }

    private void Interact()
    {
        opened = true;
        sr.sprite = openSprite;

        if (room == null)
        {
            Debug.LogWarning("[Chest] room reference missing.");
            return;
        }

        if (isMimic)
        {
            Debug.Log($"[Chest] Mimic triggered in room {room.roomId} (chestId={chestId})");
            // TODO: spawn bats / combat
        }
        else
{ }}
    // REAL chest: spawn key item prefab exactly once
   //     var loader = FindFirstObjectByType<RoomLoader>();
        //var state = loader != null ? loader.GetRoomState(room.roomId) : null;

        // if (state != null && state.keyCollected)
        // {
        //     Debug.Log("[Chest] Key already collected for this room.");
        //     return;
        // }

        // if (room.keyItemPrefab == null)
        // {
        //     Debug.LogError("[Chest] keyItemPrefab not assigned on roomController.");
        //     return;
        // }

        // if (itemSpawnPoint == null)
        // {
        //     Debug.LogError("[Chest] itemSpawnPoint not assigned.");
        //     return;
        // }

        //Instantiate(room.keyItemPrefab, itemSpawnPoint.position, Quaternion.identity);

        // Mark collected so it won't spawn again
    //     if (state != null) state.keyCollected = true;

    //     Debug.Log($"[Chest] Spawned key item from real chest in room {room.roomId}");
    // }

 //   }

    public void SetOpenedVisual(bool open)
    {
        opened = open;
        sr.sprite = open ? openSprite : closedSprite;
    }

    
}
