using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int startingExperienceRequirement = 5;

    [SerializeField, Min(1.01f)]
    private float requirementMultiplier = 1.5f;

    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    public event Action<int, int> ExperienceChanged;
    public event Action<int> LevelChanged;
    public event Action<int> LeveledUp;

    private void Awake()
    {
        ExperienceToNextLevel =
            startingExperienceRequirement;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentExperience += amount;

        while (
            CurrentExperience >= ExperienceToNextLevel
        )
        {
            CurrentExperience -= ExperienceToNextLevel;
            Level++;

            ExperienceToNextLevel = Mathf.CeilToInt(
                ExperienceToNextLevel *
                requirementMultiplier
            );

            Debug.Log($"Player reached level {Level}.", this);

            LevelChanged?.Invoke(Level);
            LeveledUp?.Invoke(Level);
        }

        Debug.Log(
            $"Player XP: {CurrentExperience}/" +
            $"{ExperienceToNextLevel}",
            this
        );

        ExperienceChanged?.Invoke(
            CurrentExperience,
            ExperienceToNextLevel
        );
    }
}