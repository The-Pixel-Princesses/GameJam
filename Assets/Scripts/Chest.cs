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
    public float interactDistance = 3f;

    [Header("Spawn")]
    public Transform itemSpawnPoint;

    [Header("Runtime (wired automatically)")]
    [HideInInspector] public roomController room;
    [HideInInspector] public bool isMimic = true;

    private RoomLoader _loader;
    private Transform _player;
    private bool _opened;
    private SpriteRenderer _sr;
    private Camera _mainCam;

    public void Initialize(roomController ownerRoom, RoomLoader loader)
    {
        room = ownerRoom;
        _loader = loader;
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _mainCam = Camera.main;
        SetOpenedVisual(false);
    }

    private void Start()
    {
        // auto-find player if not wired
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        // failsafe if Initialize didn't run yet
        if (room == null) room = GetComponentInParent<roomController>();
        if (_loader == null) _loader = FindFirstObjectByType<RoomLoader>();
    }

    private void Update()
    {
        if (_opened) return;
        if (Mouse.current == null || _mainCam == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mouseWorld = _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null) return;
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

        if (_player != null && Vector2.Distance(_player.position, transform.position) > interactDistance)
            return;

        Interact();
    }

    private void Interact()
    {
        _opened = true;
        SetOpenedVisual(true);

        if (room == null)
        {
            Debug.LogWarning("[Chest] room reference missing.");
            return;
        }

        if (isMimic)
        {
            Debug.Log($"[Chest] Mimic triggered in room {room.roomId} (chestId={chestId})");
            // TODO: spawn bats / combat
            return;
        }

        // REAL chest: spawn key once per roomId
        if (_loader == null)
        {
            Debug.LogWarning("[Chest] RoomLoader missing; cannot persist keyCollected.");
            return;
        }

        var state = _loader.GetRoomState(room.roomId);
        if (state.keyCollected)
        {
            Debug.Log($"[Chest] Key already collected for room {room.roomId}.");
            return;
        }

        if (room.keyItemPrefab == null)
        {
            Debug.LogError("[Chest] keyItemPrefab not assigned on roomController.");
            return;
        }

        if (itemSpawnPoint == null)
        {
            Debug.LogError("[Chest] itemSpawnPoint not assigned on Chest.");
            return;
        }

        Instantiate(room.keyItemPrefab, itemSpawnPoint.position, Quaternion.identity);
        state.keyCollected = true;

        Debug.Log($"[Chest] Spawned key from real chest in room {room.roomId}");
    }

    public void SetOpenedVisual(bool open)
    {
        _opened = open;
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = open ? openSprite : closedSprite;
    }
}
