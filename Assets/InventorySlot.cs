using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public GameObject counterImage;
    public TextMeshProUGUI counter;
    public Button removeBtn;

    //public bool isPlayerSlot = false;
    public InventoryType type;

    Item item;

    public void AddItem(List<Item> itemList)
    {
        item = itemList[0];

        icon.sprite = item.icon;
        icon.enabled = true;

        // Don't show the remove button in the equipment manager
        removeBtn.interactable = type != InventoryType.Equipment;

        //counter value = itemList.count
        counter.text = itemList.Count.ToString();
        if (itemList.Count <= 1)
        {
            //make sure to change this back to false
            counterImage.SetActive(false);
        } else
        { 
            counterImage.SetActive(true);
        }


    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        removeBtn.interactable = false;
        counterImage.SetActive(false);
    }

    public void OnRemoveButton()
    {
        if (type == InventoryType.Player)
        {
            PlayerInventory.instance.Remove(item);
        }
        else if (type == InventoryType.Chest)
        {
            ChestInventory.instance.Remove(item);
        }
        else
        {
            Debug.LogError("Removal of unknown type????");
        }
    }

    public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }
}
