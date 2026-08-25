using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponId; 
    public string weaponName;
    public float baseAttackBonus;
}