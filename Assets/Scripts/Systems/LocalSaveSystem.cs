using System;
using System.IO;
using UnityEngine;

public static class LocalSaveSystem
{
    private const string SaveFileName =
        "wave_survivor_save.json";

    private const int CurrentSaveVersion = 1;
    private const int TotalStageCount = 5;

    private static GameProgressData cachedData;

    private const string GoldKey = "TOTAL_GOLD";
    private const string StageClearsPrefix = "STAGE_CLEARS_";

    public static int GetGold() => PlayerPrefs.GetInt(GoldKey, 0);

    public static void AddGold(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(GoldKey, GetGold() + amount);
        PlayerPrefs.Save();
    }

    public static bool SpendGold(int amount)
    {
        int currentGold = GetGold();
        if (amount <= 0 || currentGold < amount) return false;
        PlayerPrefs.SetInt(GoldKey, currentGold - amount);
        PlayerPrefs.Save();
        return true;
    }

    public static int GetStageClearCount(int stageId)
    {
        return PlayerPrefs.GetInt(StageClearsPrefix + stageId, 0);
    }

    public static void IncrementStageClear(int stageId)
    {
        int current = GetStageClearCount(stageId);
        PlayerPrefs.SetInt(StageClearsPrefix + stageId, current + 1);
        PlayerPrefs.Save();
    }

    // Calculates gold yield decay: 100% -> 80% -> 60% -> 40% -> 20% (min)
    public static float GetStageGoldMultiplier(int stageId)
    {
        int clearCount = GetStageClearCount(stageId);
        float multiplier = 1f - (clearCount * 0.2f);
        return Mathf.Max(0.2f, multiplier);
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