using UnityEngine;

public static class StatCalculator
{
    public static float GetTotalHP(CharacterData charData)
    {
        if (charData == null) return 10f;
        // Base HP + Skill Tree Level Bonus (5 HP per level)
        float skillTreeBonus = LocalSaveSystem.GetSkillAtkLevel() * 5f;
        return charData.baseMaxHP + skillTreeBonus;
    }

    public static float GetTotalAttack(CharacterData charData, WeaponData weaponData)
    {
        if (charData == null) return 5f;
        float weaponBonus = weaponData != null ? weaponData.baseAttackBonus : 0f;
        float skillTreeBonus = LocalSaveSystem.GetBonusDamage();
        return charData.baseAttack + weaponBonus + skillTreeBonus;
    }

    public static float GetTotalSpeed(CharacterData charData)
    {
        if (charData == null) return 3f;
        float skillTreeBonus = LocalSaveSystem.GetBonusSpeed();
        return charData.baseMoveSpeed + skillTreeBonus;
    }
}