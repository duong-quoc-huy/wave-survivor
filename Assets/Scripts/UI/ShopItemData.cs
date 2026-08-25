using UnityEngine;

public enum ShopItemType
{
    Potion_Attack,
    Potion_Speed,
    Weapon,
    Character,
    Empty
}

[System.Serializable]
public class ShopItemData
{
    public string itemId;
    public string itemName;
    public Sprite itemIcon;
    public int price;
    public ShopItemType itemType;
    public bool isUnlocked;
}