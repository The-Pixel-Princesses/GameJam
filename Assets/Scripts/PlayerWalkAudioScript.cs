using UnityEngine;

public class WalkLoopSFX : MonoBehaviour
{
    [SerializeField] private AudioSource walkLoopSource;
    [SerializeField] private float minSpeedToPlay = 0.1f;

    void Update()
    {
        // Example: using Rigidbody2D velocity. Replace with your own movement check.
        float speed = GetComponent<Rigidbody2D>().linearVelocity.magnitude;

        bool shouldPlay = speed > minSpeedToPlay;

        if (shouldPlay && !walkLoopSource.isPlaying) walkLoopSource.Play();
        if (!shouldPlay && walkLoopSource.isPlaying) walkLoopSource.Stop();
    }
}
