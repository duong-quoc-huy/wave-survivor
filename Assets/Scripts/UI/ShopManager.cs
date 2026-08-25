using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI Displays")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text atkSkillText;
    [SerializeField] private TMP_Text speedSkillText;

    [Header("Prices")]
    [SerializeField] private int potionPrice = 25;
    [SerializeField] private int skillAtkBaseCost = 50;
    [SerializeField] private int skillSpeedBaseCost = 40;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {LocalSaveSystem.GetGold()}";

        if (atkSkillText != null)
            atkSkillText.text = $"Base ATK (Lvl {LocalSaveSystem.GetSkillAtkLevel()}/3)";

        if (speedSkillText != null)
            speedSkillText.text = $"Move Speed (Lvl {LocalSaveSystem.GetSkillSpeedLevel()}/3)";
    }

    public void BuySpeedPotion()
    {
        if (LocalSaveSystem.SpendGold(potionPrice))
        {
            LocalSaveSystem.AddPotion("SPEED", 1);
            RefreshUI();
        }
    }

    public void BuyAttackPotion()
    {
        if (LocalSaveSystem.SpendGold(potionPrice))
        {
            LocalSaveSystem.AddPotion("ATTACK", 1);
            RefreshUI();
        }
    }

    public void UpgradeBaseAttack()
    {
        int currentLvl = LocalSaveSystem.GetSkillAtkLevel();
        int cost = skillAtkBaseCost * (currentLvl + 1);
        if (LocalSaveSystem.UpgradeSkillAtk(cost))
        {
            RefreshUI();
        }
    }

    public void UpgradeMoveSpeed()
    {
        int currentLvl = LocalSaveSystem.GetSkillSpeedLevel();
        int cost = skillSpeedBaseCost * (currentLvl + 1);
        if (LocalSaveSystem.UpgradeSkillSpeed(cost))
        {
            RefreshUI();
        }
    }

    public void SelectCharacter(string charId)
    {
        LocalSaveSystem.SetEquippedCharacter(charId);
        Debug.Log($"Equipped Character: {charId}");
    }

    public void SelectWeapon(string weaponId)
    {
        LocalSaveSystem.SetEquippedWeapon(weaponId);
        Debug.Log($"Equipped Weapon: {weaponId}");
    }
}