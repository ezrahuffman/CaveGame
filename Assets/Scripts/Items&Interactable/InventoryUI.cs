using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryUI;
    public Transform itemsParent;

    PlayerInventory inventory;

    InventorySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        inventory = PlayerInventory.instance;
        if (inventory != null)
        {
            inventory.onItemChangedCallback += UpdateUI;
        }
        else
        {
            Debug.LogWarning("there is no callback");
        }


        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        //set as player slots
        //NOTE: this is just to ensure the type of slots is correct
        foreach (InventorySlot slot in slots)
        {
            slot.type = InventoryType.Player;
        }
        PlayerInventory.instance.AddDefault();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }

    void UpdateUI()
    {
        int i = 0;
        //add items to slots
        foreach (List<Item> itemList in inventory.items.Values)
        {
            if (i < slots.Length)
            {
                slots[i].AddItem(itemList);
            }
            i++;
        }
        //clear any remaining slots
        for (; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }
}
