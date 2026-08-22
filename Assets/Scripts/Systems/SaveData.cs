using System;
using System.Collections.Generic;

[Serializable]
public class StageProgressData
{
    public int stageId;
    public bool completed;
    public float bestSurvivalTime;
    public int highestLevelReached;
    public bool bossDefeated;

    public StageProgressData()
    {
    }

    public StageProgressData(int newStageId)
    {
        stageId = newStageId;
        completed = false;
        bestSurvivalTime = 0f;
        highestLevelReached = 1;
        bossDefeated = false;
    }
}

[Serializable]
public class GameProgressData
{
    public int saveVersion = 1;

    public int highestUnlockedStage = 1;
    public int selectedCharacterId = 0;

    public bool musicEnabled = true;
    public bool sfxEnabled = true;

    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public List<int> unlockedCharacterIds =
        new List<int>();

    public List<StageProgressData> stages =
        new List<StageProgressData>();

    public GameProgressData()
    {
        unlockedCharacterIds.Add(0);

        for (int stageId = 1; stageId <= 5; stageId++)
        {
            stages.Add(
                new StageProgressData(stageId)
            );
        }
    }
}