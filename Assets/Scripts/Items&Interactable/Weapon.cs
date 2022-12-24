using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Equipment/Weapon")]
public class Weapon : Equipment
{
    public GameObject hitBox;

    [Min(0f)]
    public float attackSpeed;

}
