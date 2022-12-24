using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "Inventory/Item/HealthPotion")]
public class HealthPotion : Item
{
    public bool healOverTime = true;
    public int time = 5;
    public int amount = 5;
    public override void Use()
    {
        base.Use();
        if (PlayerStats.instance != null && !inChest)
        {
            if (healOverTime)
            {
                PlayerStats.instance.Heal(amount, time);
            }
            else
            {
                PlayerStats.instance.Heal(amount);
            }

            PlayerInventory.instance.Remove(this);
        }
    }
}
