using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item/DefaultItem" )]
public class Item : ScriptableObject
{

    public bool stackable;      //whether or not this item can be stacked
    public int maxCount;        //maximum number of times this item can be stacked
    private int stackID;

    new public string name = "New Item";
    public Sprite icon = null;
    public bool isDefaultItem = false;
    public bool inChest = false;   //if in chest, don't use item, instead move it to inventory
    //TODO: see if there is a better condition for when the player is in a chest, so that each item doesn't need to be checked
    //TODO: See if there is a better way to not use the item when in chest, currently testing public bool inChest

    //Use this item
    public virtual void Use()
    {
        //check if the player is currently in a chest or not
        if (ChestInventory.instance.Chest != null)
        {
            inChest = ChestInventory.instance.Chest.chestIsOpen;
        }
        else
        {
            inChest = false;
        }

        //use the item
        //if player is in chest, don't actually use the item
        if (inChest)        
        {
            //if item is in chest, move to inventory
            if (isInChest())
            {
                //TODO: check that items aren't removed when other inventory is full

                //remove item from one inventory and add to the other
                Item removedItem = ChestInventory.instance.Remove(this);
                PlayerInventory.instance.Add(removedItem);
            }
            else if(isInPlayer())   //if item is in inventory, move to chest
            {
                Item removedItem = PlayerInventory.instance.Remove(this);
                ChestInventory.instance.Add(removedItem);
            }
        }
        else
        {
            //if player is not in chest, use item
            //NOTE: this structure is temperary, because this is a virtual method
            //Debug.Log("Using " + name);
        }

        //Note this is virtual, the above is for all items, so it is okay
    }

    public void RemoveFromInventory()
    {
        PlayerInventory.instance.Remove(this);
    }


    //check if item is in chest
    public bool isInChest()
    {
        //set items and itemNames to Chest Inventory values
        Dictionary<int, List<Item>> items = new Dictionary<int, List<Item>>();
        Dictionary<string, List<Item>> itemNames = new Dictionary<string, List<Item>>();

        items = ChestInventory.instance.items;
        itemNames = ChestInventory.instance.itemNames;

        //check that an item of this type exists in chest
        if (itemNames.ContainsKey(name))
        {
            //search stacks for stacks of this item type
            foreach (List<Item> itemList in items.Values)
            {
                Item firstItem = itemList[0];

                //search stacks for this specific item
                if (firstItem.name == name)
                {
                    foreach (Item item in itemList)
                    {
                        if (item == this)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    //check if item is in Player inventory
    public bool isInPlayer()
    {
        //set items and itemNames to Chest Inventory values
        Dictionary<int, List<Item>> items = new Dictionary<int, List<Item>>();
        Dictionary<string, List<Item>> itemNames = new Dictionary<string, List<Item>>();

        items = PlayerInventory.instance.items;
        itemNames = PlayerInventory.instance.itemNames;

        //check that an item of this type exists in chest
        if (itemNames.ContainsKey(name))
        {
            //search stacks for stacks of this item type
            foreach (List<Item> itemList in items.Values)
            {
                Item firstItem = itemList[0];

                //search stacks for this specific item
                if (firstItem.name == name)
                {
                    foreach (Item item in itemList)
                    {
                        if (item == this)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    //determine whether or not the item is stackable
    public bool IsStackable(Inventory inventory)
    {


        //TODO: try to make this more efficient
        //check each stack of the same type of item


        Dictionary<int, List<Item>> items = new Dictionary<int, List<Item>>();
        Dictionary<string, List<Item>> itemNames = new Dictionary<string, List<Item>>();

        //check if instance is chest inventory or player inventory
        if (inventory == ChestInventory.instance)
        {
            items = ChestInventory.instance.items;
            itemNames = ChestInventory.instance.itemNames;
            //Debug.Log("chest inventory");
        }
        else
        {
            //Debug.Log("player inventory");
            items = PlayerInventory.instance.items;
            itemNames = PlayerInventory.instance.itemNames;
        }

        //check that inventory contains item of this type
        if (itemNames.ContainsKey(name))
        {
            //check each stack
            foreach (List<Item> itemList in inventory.items.Values)
            {
                Item firstItem = itemList[0];

                //check stacks that are of this item type
                if (firstItem.name == name)
                {
                    //check to see if there is room in this stack
                    if (items[firstItem.GetInstanceID()].Count < maxCount)
                    {
                        stackID = firstItem.GetInstanceID();
                        return true;
                    }
                }
            }
        }
        return false;

    }

    #region DebugDictionary
    private void PrintItems(Dictionary<int, List<Item>> items)
    {
        string output = "";
        foreach (List<Item> itemList in items.Values)
        {
            output += itemList[0].name + ": ";
            foreach(Item item in itemList)
            {
                output += item.GetInstanceID() + ", ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }


    private void PrintKeys(Dictionary<int, List<Item>> items)
    {
        string output = "";
        foreach (int key in items.Keys)
        {
            output += key + ", ";
        }
        Debug.Log(output);
    }

    #endregion


    public int StackID
    {
        get
        {
            return stackID;
        }
    }

}
