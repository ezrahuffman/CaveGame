using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    #region Singleton

    public static EquipmentManager instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
    public OnEquipmentChanged onEquipmentChanged;
    public Equipment[] currentEquipment { private set; get; }
    PlayerInventory inventory;
    PlayerController playerCont;
    private bool torchEquiped = false;

    private void Start()
    {
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new Equipment[numSlots];
        inventory = PlayerInventory.instance;
        playerCont = FindObjectOfType<PlayerController>();
    }

    public void Equip(Equipment newItem)
    {
        Debug.Log($"equip {newItem.name}");
        if (newItem.equipmentSlot == EquipmentSlot.Torch)
        {
            torchEquiped = true;
            FindObjectOfType<PlayerController>().EquipTorch(torchEquiped);
        }

        
        int slotIndex = (int)newItem.equipmentSlot;


        //swap with current item
        Equipment oldItem = null;
        if(currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
            Unequip(slotIndex);
            //inventory.Add(oldItem);
        }

        currentEquipment[slotIndex] = newItem;

        if(onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newItem, oldItem);
        }

        playerCont.SetAnimations();
    }

    public void Unequip(int slotIndex)
    {
        Debug.Log($"unequip {currentEquipment[slotIndex].name}");
        if (slotIndex == (int)EquipmentSlot.Torch)
        {
            torchEquiped = false;
            FindObjectOfType<PlayerController>().EquipTorch(torchEquiped);
        }

        if (currentEquipment[slotIndex] != null)
        {
            Equipment oldItem = currentEquipment[slotIndex];
            if (oldItem.name != "Hands")
            {
                inventory.Add(oldItem);
            }

            currentEquipment[slotIndex] = null;

            if(onEquipmentChanged != null)
            {
                onEquipmentChanged.Invoke(null, oldItem);
            }
        }

        playerCont.SetAnimations();
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
        }

        return retStr;
    }

    public void UnequipAll()
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            Unequip(i);
        }
    }

    public bool hasTorch
    {
        get
        {
            return torchEquiped;
        }
    }

    public Weapon curWeapon
    {
        get
        {
            return currentEquipment[(int)EquipmentSlot.Weapon] as Weapon;
        }
    }
}
