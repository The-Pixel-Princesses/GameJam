using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private Sprite sprite;
    private ApplicationScript appScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    appScript = GameObject.Find("CharacterCanvas").GetComponent<ApplicationScript>();

    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplicationScript.Instance.AddItem(itemName, quantity, sprite);
            Destroy(gameObject);
        }
    }

}
