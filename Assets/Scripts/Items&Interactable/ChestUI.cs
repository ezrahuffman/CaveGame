using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    public Transform itemsParent;

    ChestInventory inventory;

    InventorySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        inventory = ChestInventory.instance;
        inventory.onItemChangedCallback += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
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
