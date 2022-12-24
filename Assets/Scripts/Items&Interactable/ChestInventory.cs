using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInventory : Inventory
{
    #region Singleton

    public static ChestInventory instance;

    private void Awake()
    {
        //create singleton
        if (instance != null)
        {
            Debug.LogWarning("there is already an instance of inventory system, but you are trying to create chestInventory");
            return;
        }
        instance = this;

        //set chest UI and disable 
        chestUI = GameObject.FindGameObjectWithTag("ChestUI");
        if (chestUI != null)
        {
            chestUI.SetActive(false);
        }

        //hide open text
        openChestText.SetActive(false);
    }

    #endregion

    private Chest curChest;
    private GameObject chestUI;
    public GameObject openChestText;

    public void SetChest(Chest chest)
    {
        //set current chest and update space
        curChest = chest;
        space = chest.space;

        //clear previous chest inventory
        instance.Clear();

        //put items into chest inventory
        foreach (Item item in chest.Items)
        {
            instance.Add(item);
        }
    }

    //Close chest
    public void CloseChest()
    {
        //if there is a chest, reset the items
        if (curChest != null)
        {
            curChest.ResetItems(items);
        }

        //hide chest UI
        chestUI.SetActive(false);
    }

    //Open chest
    public void OpenChest(Chest chest)
    {
        //Set the chest to current chest
        SetChest(chest);

        //display the chest UI
        chestUI.SetActive(true);
    }

    public Chest Chest
    {
        get
        {
            return curChest;
        }
    }
}
