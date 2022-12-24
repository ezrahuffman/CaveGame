using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Equipment",  menuName = "Inventory/Equipment/Default")]
public class Equipment : Item
{
    public EquipmentSlot equipmentSlot;

    public int armorModifier;
    public int damageModifier;
    public int torchModifier;
    public bool hideInEquipmentManager;

    public override void Use()
    {
        base.Use();

        //only use item if not in chest
        if (!inChest)
        {
            if (isInPlayer())
            {
                //equip the item
                EquipmentManager.instance.Equip(this);
                //remove from inventory
                RemoveFromInventory();
            }
            else
            {
                EquipmentManager.instance.Unequip((int)equipmentSlot);
            }

        }
    }
}

//TODO: Weapon and Sheild should be part of hands (they can both be dualweilded)... unless they can't (i.e. big sword/sheild)
public enum EquipmentSlot { Head, Chest, Legs, Weapon, Shield, Feet, Torch}