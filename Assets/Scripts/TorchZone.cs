using UnityEngine;

public class TorchZone : MonoBehaviour
{
    public GoldHealthBarScript script;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("In the trigger zone");
            script.SetNearTorch(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Out the trigger zone");
            script.SetNearTorch(false);
        }
    }
}
