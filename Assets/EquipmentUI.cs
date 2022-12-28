using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    public GameObject equipmentUI;
    public Transform equipedItemsParent;

    PlayerInventory inventory;
    EquipmentManager equipmentManager;

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

        equipmentManager = EquipmentManager.instance;
        equipmentManager.onEquipmentChanged += UpdateUI;


        slots = equipedItemsParent.GetComponentsInChildren<InventorySlot>();

        //set as equipment slots
        //NOTE: this is just to ensure the type of slots is correct
        foreach (InventorySlot slot in slots)
        {
            slot.type = InventoryType.Equipment;
        }
    }

    string PrintArr(Equipment[] arr)
    {
        string retStr = "";
        foreach (var elem in arr)
        {
            if (elem != null)
            {
                retStr += elem.name + ", ";
            }
            else
            {
                retStr += "null, ";
            }
        }

        return retStr;
    }

    void UpdateUI()
    {

        //Debug.Log($"Update Equipment UI: {PrintArr(equipmentManager.currentEquipment)}");
        int i = 0;
        //add items to slots
        foreach (var equipment in equipmentManager.currentEquipment)
        {
            if(equipment != null && !equipment.hideInEquipmentManager)
            {
                slots[(int)equipment.equipmentSlot].AddItem(new List<Item>() { equipment });
            }
            else
            {
                //Debug.Log($"clear slot {i}");
                slots[i].ClearSlot();
            }
            i++;
        }
    }

    void UpdateUI(Equipment newItem, Equipment oldItem)
    {
        Debug.Log("update UI");
        //Debug.Log($"Update Equipment UI: {PrintArr(equipmentManager.currentEquipment)}");
        int i = 0;
        //add items to slots
        foreach (var equipment in equipmentManager.currentEquipment)
        {
            if (equipment != null && !equipment.hideInEquipmentManager)
            {
                slots[(int)equipment.equipmentSlot].AddItem(new List<Item>() { equipment });
            }
            else
            {
                //Debug.Log($"clear slot {i}");
                slots[i].ClearSlot();
            }
            i++;
        }
    }
}
