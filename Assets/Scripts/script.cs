using UnityEngine;

/// <summary>
/// Temporary camera follow script for debugging room transitions.
/// - Follows a target (usually Player)
/// - Optionally clamps to bounds (can be disabled)
/// - Uses LateUpdate so it follows after movement/teleports
/// </summary>
public class CameraFollowDebug : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float followSpeed = 12f;   // higher = snappier
    public float z = -10f;

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-999f, -999f);
    public Vector2 maxBounds = new Vector2(999f, 999f);

    public bool logWhenTargetMissing = true;

    void LateUpdate()
    {
        if (target == null)
        {
            if (logWhenTargetMissing)
                Debug.LogWarning("[CameraFollowDebug] No target assigned (drag Player here).");
            return;
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, z);

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }

        // Smooth follow (if you want instant snap, set followSpeed very high or use transform.position = desired)
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
    }

    // Convenience button: call this from inspector via context menu if needed
    [ContextMenu("Snap To Target Now")]
    public void SnapToTargetNow()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x, target.position.y, z);
        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        }
        transform.position = desired;
    }
}
