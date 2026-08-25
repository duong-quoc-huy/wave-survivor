using System;
using System.IO;
using UnityEngine;

public static class LocalSaveSystem
{
    private const string SaveFileName =
        "wave_survivor_save.json";

    private const int CurrentSaveVersion = 2;
    private const int TotalStageCount = 5;

    private static GameProgressData cachedData;

    private const string GoldKey = "TOTAL_GOLD";
    private const string StageClearsPrefix = "STAGE_CLEARS_";

    private const string SelectedCharKey = "EQUIPPED_CHAR_ID";
    private const string SelectedWeaponKey = "EQUIPPED_WEAPON_ID";

   
    private const string SpeedPotionKey = "POTION_SPEED_COUNT";
    private const string AttackPotionKey = "POTION_ATTACK_COUNT";


    private const string SkillAtkLevelKey = "SKILL_ATK_LEVEL";
    private const string SkillSpeedLevelKey = "SKILL_SPEED_LEVEL";

    public static int GetGold()
    {
        return Mathf.Max(0, Data.gold);
    }

    public static void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Data.gold += amount;
        Save();
    }

    public static bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (Data.gold < amount)
            return false;

        Data.gold -= amount;
        Save();

        return true;
    }


    public static int GetStageClearCount(int stageId) => PlayerPrefs.GetInt(StageClearsPrefix + stageId, 0);

    public static void IncrementStageClear(int stageId)
    {
        PlayerPrefs.SetInt(StageClearsPrefix + stageId, GetStageClearCount(stageId) + 1);
        PlayerPrefs.Save();
    }

    // Calculates gold yield decay: 100% -> 80% -> 60% -> 40% -> 20% (min)
    public static float GetStageGoldMultiplier(int stageId)
    {
        int clearCount = GetStageClearCount(stageId);
        return Mathf.Max(0.2f, 1f - (clearCount * 0.2f));
    }

    public static string GetEquippedCharacter() => PlayerPrefs.GetString(SelectedCharKey, "Warrior");
    public static void SetEquippedCharacter(string charId) { PlayerPrefs.SetString(SelectedCharKey, charId); PlayerPrefs.Save(); }

    public static string GetEquippedWeapon() => PlayerPrefs.GetString(SelectedWeaponKey, "Dagger");
    public static void SetEquippedWeapon(string weaponId) { PlayerPrefs.SetString(SelectedWeaponKey, weaponId); PlayerPrefs.Save(); }

    // Potions
    public static int GetPotionCount(string potionType) => PlayerPrefs.GetInt("POTION_" + potionType, 0);

    public static void AddPotion(string potionType, int amount)
    {
        int current = GetPotionCount(potionType);
        PlayerPrefs.SetInt("POTION_" + potionType, current + amount);
        PlayerPrefs.Save();
    }

    public static bool ConsumePotion(string potionType)
    {
        int current = GetPotionCount(potionType);
        if (current <= 0) return false;
        PlayerPrefs.SetInt("POTION_" + potionType, current - 1);
        PlayerPrefs.Save();
        return true;
    }

    // Skill Tree Progression
    public static int GetSkillAtkLevel() => PlayerPrefs.GetInt(SkillAtkLevelKey, 0);
    public static int GetSkillSpeedLevel() => PlayerPrefs.GetInt(SkillSpeedLevelKey, 0);

    public static bool UpgradeSkillAtk(int cost)
    {
        int lvl = GetSkillAtkLevel();
        if (lvl >= 3 || !SpendGold(cost)) return false;
        PlayerPrefs.SetInt(SkillAtkLevelKey, lvl + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool UpgradeSkillSpeed(int cost)
    {
        int lvl = GetSkillSpeedLevel();
        if (lvl >= 3 || !SpendGold(cost)) return false;
        PlayerPrefs.SetInt(SkillSpeedLevelKey, lvl + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static int GetBonusDamage()
    {
        int lvl = GetSkillAtkLevel();
        return lvl switch { 1 => 5, 2 => 10, 3 => 20, _ => 0 };
    }

    public static float GetBonusSpeed()
    {
        int lvl = GetSkillSpeedLevel();
        return lvl switch { 1 => 5f, 2 => 10f, 3 => 20f, _ => 0f };
    }


    public static string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            SaveFileName
        );

    public static GameProgressData Data
    {
        get
        {
            if (cachedData == null)
                cachedData = LoadFromDisk();

            return cachedData;
        }
    }

    public static bool IsStageUnlocked(int stageId)
    {
        if (stageId < 1 || stageId > TotalStageCount)
            return false;

        return stageId <= Data.highestUnlockedStage;
    }

    public static StageProgressData GetStageProgress(
        int stageId
    )
    {
        EnsureValidData(Data);

        StageProgressData progress =
            Data.stages.Find(
                stage => stage.stageId == stageId
            );

        if (progress != null)
            return progress;

        progress = new StageProgressData(stageId);
        Data.stages.Add(progress);

        return progress;
    }

    public static void RecordStageResult(
        int stageId,
        bool completed,
        float survivalTime,
        int levelReached,
        bool bossDefeated = false
    )
    {
        if (stageId < 1 || stageId > TotalStageCount)
        {
            Debug.LogError(
                $"Cannot record invalid stage ID: {stageId}"
            );

            return;
        }

        StageProgressData progress =
            GetStageProgress(stageId);

        progress.bestSurvivalTime = Mathf.Max(
            progress.bestSurvivalTime,
            survivalTime
        );

        progress.highestLevelReached = Mathf.Max(
            progress.highestLevelReached,
            levelReached
        );

        if (completed)
        {
            progress.completed = true;

            Data.highestUnlockedStage = Mathf.Clamp(
                Mathf.Max(
                    Data.highestUnlockedStage,
                    stageId + 1
                ),
                1,
                TotalStageCount
            );
        }

        if (bossDefeated)
            progress.bossDefeated = true;

        Save();
    }

    public static void SetSelectedCharacter(
        int characterId
    )
    {
        if (!Data.unlockedCharacterIds.Contains(
                characterId
            ))
        {
            Debug.LogWarning(
                $"Character {characterId} is locked."
            );

            return;
        }

        Data.selectedCharacterId = characterId;
        Save();
    }

    public static void Save()
    {
        EnsureValidData(Data);

        try
        {
            string directory =
                Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(
                Data,
                true
            );

            File.WriteAllText(SavePath, json);

            Debug.Log(
                $"Progress saved to: {SavePath}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Failed to save progress.\n" +
                exception.Message
            );
        }
    }

    public static void Reload()
    {
        cachedData = LoadFromDisk();
    }

    public static void ResetProgress()
    {
        cachedData = CreateDefaultData();
        Save();

        Debug.Log("Local progress was reset.");
    }

    private static GameProgressData LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            GameProgressData newData =
                CreateDefaultData();

            WriteInitialSave(newData);

            return newData;
        }

        try
        {
            string json =
                File.ReadAllText(SavePath);

            GameProgressData loadedData =
                JsonUtility.FromJson<GameProgressData>(
                    json
                );

            if (loadedData == null)
            {
                Debug.LogWarning(
                    "The save file was empty. " +
                    "Default progress was created."
                );

                return CreateDefaultData();
            }

            EnsureValidData(loadedData);

            return loadedData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "The save file could not be loaded. " +
                "Default progress was created.\n" +
                exception.Message
            );

            return CreateDefaultData();
        }
    }

    private static void WriteInitialSave(
        GameProgressData data
    )
    {
        try
        {
            string directory =
                Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(
                data,
                true
            );

            File.WriteAllText(SavePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "The initial save file could not " +
                "be created.\n" +
                exception.Message
            );
        }
    }

    private static GameProgressData CreateDefaultData()
    {
        GameProgressData data =
            new GameProgressData();

        EnsureValidData(data);

        return data;
    }

    private static void EnsureValidData(GameProgressData data)
    {
        if (data.saveVersion < 2 &&
    PlayerPrefs.HasKey(GoldKey))
        {
            data.gold = Mathf.Max(
                data.gold,
                PlayerPrefs.GetInt(GoldKey, 0)
            );
        }

        data.gold = Mathf.Max(0, data.gold);
        data.saveVersion = CurrentSaveVersion;

        data.highestUnlockedStage = Mathf.Clamp(
            data.highestUnlockedStage,
            1,
            TotalStageCount
        );

        data.musicVolume = Mathf.Clamp01(
            data.musicVolume
        );

        data.sfxVolume = Mathf.Clamp01(
            data.sfxVolume
        );

        if (data.unlockedCharacterIds == null)
        {
            data.unlockedCharacterIds =
                new System.Collections.Generic.List<int>();
        }

        if (!data.unlockedCharacterIds.Contains(0))
            data.unlockedCharacterIds.Add(0);

        if (data.stages == null)
        {
            data.stages =
                new System.Collections.Generic
                    .List<StageProgressData>();
        }

        for (int stageId = 1; stageId <= TotalStageCount; stageId++)
        {
            bool alreadyExists =
                data.stages.Exists(
                    stage =>
                        stage != null &&
                        stage.stageId == stageId
                );

            if (!alreadyExists)
            {
                data.stages.Add(
                    new StageProgressData(stageId)
                );
            }
        }
    }
}