using UnityEngine;

public class PlayerInventory : Inventory
{
    #region Singleton

    public static PlayerInventory instance;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("there is already an instance of inventory system, but you are trying to create another");
            return;
        }
        instance = this;
    }

    public void AddDefault()
    {
        foreach (Item item in defaultItems)
        {
            instance.Add(item);
        }
    }

    #endregion
    public Item[] defaultItems;
}
