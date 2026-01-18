using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationScript : MonoBehaviour
{
    public bool MapOpen;
    [SerializeField] protected GameObject TheMap;
    public int numKeys;
    [SerializeField] private TextMeshProUGUI keysText;
    [SerializeField] private GameObject inventoryMenu;
    public bool inventoryOpen;
    public itemSlot[] itemSlot;
    public Sprite emptySprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapOpen = false;
        inventoryOpen = false;
        TheMap.SetActive(MapOpen);
        inventoryMenu.SetActive(inventoryOpen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateKeyUI()
    {
        keysText.text = $"Keys: {numKeys}";
    }

    public void loadMap()
    {
        MapOpen = !MapOpen;
        inventoryOpen = !inventoryOpen;
        TheMap.SetActive(MapOpen);
        inventoryMenu.SetActive(inventoryOpen);
        if (MapOpen) UpdateKeyUI();

    }

    public void AddItem(string itemName, int quantity, Sprite sprite)
    {
       
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].isFull == false)
            {
                itemSlot[i].AddItem(itemName, quantity, sprite);
                numKeys++;
                return;
            }
        }
        UpdateKeyUI();
    }

    public bool TryUseItem(string itemName)
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].isFull && itemSlot[i].itemName == itemName)
            {

                itemSlot[i].isFull = false;
                itemSlot[i].itemName = "";
                itemSlot[i].itemSprite = emptySprite;
                itemSlot[i].GetComponent<itemSlot>().ClearSlot();
                itemSlot[i].UpdateSlotUI();

                numKeys--;
                UpdateKeyUI(); // if keys matter
                return true;
            }
        }
        return false; // item not found
    }

    public static ApplicationScript Instance;

    private void Awake()
    {
        Instance = this;
    }


}
