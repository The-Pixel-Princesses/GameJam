using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class TorchLight : MonoBehaviour
{
    [Header("State")]
    public bool lightOn = false;

    [Header("Sprites")]
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Interaction")]
    public Transform player;
    public string playerTag = "Player";
    public float interactDistance = 0.1f;

    private Camera mainCam;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
            else Debug.LogError($"[TorchLight] Could not find a GameObject with tag '{playerTag}'.");
        
    }

        if (sr == null)
            Debug.LogError("[TorchLight] Missing SpriteRenderer on the torch object.");

        if (mainCam == null)
            Debug.LogError("[TorchLight] Camera.main is NULL. Tag your camera as MainCamera.");

        ApplyVisual();
    }

    void Update()
    {
        if (mainCam == null || sr == null) return;
        if (Mouse.current == null) return;

        // Left click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

            if (hit == null) return;

            // Click must hit this torch (or a child)
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                if (player != null && Vector2.Distance(player.position, transform.position) > interactDistance)
                    return;

                Toggle();
            }
        }
    }

    public void Toggle()
    {
        if (!lightOn)
        {
            lightOn = !lightOn;
            ApplyVisual();
            Debug.Log($"[TorchLight] {gameObject.name} toggled. Now On={lightOn}");
        }
    }

    private void ApplyVisual()
    {
        if (sr == null) return;

        if (lightOn && onSprite != null)
            sr.sprite = onSprite;
        else if (!lightOn && offSprite != null)
            sr.sprite = offSprite;
    }
}
