using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    //TODO: update chest inventory to also be stackable (i.e. having stacks of items in the chest)
    public List<Item> items;

    ChestInventory inventory;
    GameObject openTextObj;

    public int space;
    public bool chestIsOpen = false;

    //Empty Constructor
    public Chest()
    {
        //instantiate items list as empty list
        items = new List<Item>();

        if (ChestInventory.instance != null)
        {
            inventory = ChestInventory.instance;
        }
    }

    private void Start()
    {
        //if inventory has not been set, set it
        if (inventory == null)
        {
            inventory = ChestInventory.instance;
        }

        //set open text
        openTextObj = inventory.openChestText;
    }

    //interact with this interactable
    public override void Interact()
    {
        //base code from interactable virtual method
        base.Interact();

        //new code here
        if (!chestIsOpen)
        {
            Open();
            openTextObj.SetActive(false);
        }
        else
        {
            Close();
        }
        chestIsOpen = !chestIsOpen;
    }

    //called when player enters area
    public override void EnteredArea(GameObject playerObj)
    {
        base.EnteredArea(playerObj);

        //display "press 'E' to open" text
        openTextObj.SetActive(true);
    }

    //called when player leaves area
    public override void LeftArea(GameObject playerObj)
    {
        base.LeftArea(playerObj);

        //If chest is open when player leaves area, close it
        if (chestIsOpen)
        {
            Interact();
        }

        //hide "press 'E' to open" text
        openTextObj.SetActive(false);
    }

    private void Open()
    {
        Debug.Log("Open Chest");
        //show chest inventory and allow player to take items from the chest (maybe have chest get destroyed after, or just let player store things in the chest)
        if (inventory != null)
        {
            inventory.OpenChest(this);
        }
        else
        {
            Debug.Log("there is no chest inventory");
        }
    }


    public void Close()
    {
        inventory.CloseChest();
    }

    //reset the items in chest to list
    public void ResetItems(Dictionary<int, List<Item>> newItems)
    {
        items.Clear();

        foreach(List<Item> itemList in newItems.Values)
        {
            foreach (Item item in itemList)
            {
                items.Add(item);
            }
        }
    }

    //remove item from the chest
    public void RemoveItem(Item item)
    {

    }

    //add item to the chest
    public void AddItem(Item item)
    {
        //check if there is space
        if (items.Count < space)
        {
            //add item to chest
        }
    }

    public List<Item> Items
    {
        get
        {
            return items;
        }
    }
}
