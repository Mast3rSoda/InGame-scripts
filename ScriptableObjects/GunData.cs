using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Name")]
    public new string name;
    
    [Header("Shooting")]
    public float maxDistance;
    public float fireRate;

    [Header("Ammo")]
    public float currentAmmo;
    public float magSize;


}
