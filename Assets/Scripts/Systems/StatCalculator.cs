using UnityEngine;

public static class StatCalculator
{
    public static float GetTotalHP(CharacterData charData)
    {
        if (charData == null) return 10f;
        float baseHp = AdminConsole.ResolvePlayerBaseHp(
            Mathf.RoundToInt(charData.baseMaxHP)
        );
        // Base HP + Skill Tree Level Bonus (5 HP per level)
        float skillTreeBonus = LocalSaveSystem.GetSkillAtkLevel() * 5f;
        return baseHp + skillTreeBonus;
    }

    public static float GetTotalAttack(CharacterData charData, WeaponData weaponData)
    {
        if (charData == null) return 5f;
        float baseAttack =
            AdminConsole.ResolvePlayerBaseAtk(charData.baseAttack);
        float weaponBonus = weaponData != null ? weaponData.baseAttackBonus : 0f;
        float skillTreeBonus = LocalSaveSystem.GetBonusDamage();
        return baseAttack + weaponBonus + skillTreeBonus;
    }

    public static float GetTotalSpeed(CharacterData charData)
    {
        if (charData == null) return 3f;
        float baseSpeed =
            AdminConsole.ResolvePlayerBaseSpeed(charData.baseMoveSpeed);
        float skillTreeBonus = LocalSaveSystem.GetBonusSpeed();
        return baseSpeed + skillTreeBonus;
    }
}
