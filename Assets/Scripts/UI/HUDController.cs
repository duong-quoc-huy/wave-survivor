using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private RunTimer runTimer;

    [Header("Health UI")]
    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private TMP_Text healthText;

    [Header("Experience UI")]
    [SerializeField]
    private Slider experienceSlider;

    [SerializeField]
    private TMP_Text experienceText;

    [SerializeField]
    private TMP_Text levelText;

    [Header("Timer UI")]
    [SerializeField]
    private TMP_Text timerText;

    [Header("Ability Slots")]
    [SerializeField] private AbilitySlotUI eSkillSlot;
    [SerializeField] private AbilitySlotUI qSkillSlot;
    [SerializeField] private AbilitySlotUI speedPotionSlot;
    [SerializeField] private AbilitySlotUI attackPotionSlot;

    private PlayerAbilities playerAbilities;

    public void BindAbilities(GameObject player)
    {
        if (player == null) return;
        playerAbilities = player.GetComponent<PlayerAbilities>();
    }

    public void BindPlayer(GameObject player)
    {
        if (player == null) return;

        
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealth;
        }

        if (playerStats != null)
        {
            playerStats.ExperienceChanged -= UpdateExperience;
            playerStats.LevelChanged -= UpdateLevel;
        }

        
        playerHealth = player.GetComponent<PlayerHealth>();
        playerStats = player.GetComponent<PlayerStats>();

      
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }


        if (playerStats != null)
        {
            playerStats.ExperienceChanged += UpdateExperience;
            playerStats.LevelChanged += UpdateLevel;

            UpdateExperience(
                playerStats.CurrentExperience,
                playerStats.ExperienceToNextLevel
            );

            UpdateLevel(playerStats.Level);
        }

        if (playerAbilities != null)
        {
            if (eSkillSlot != null)
                eSkillSlot.UpdateSlot(playerAbilities.ECooldownRemaining, playerAbilities.ECooldownTotal);
            if (qSkillSlot != null)
                qSkillSlot.UpdateSlot(playerAbilities.QCooldownRemaining, playerAbilities.QCooldownTotal);
            if (speedPotionSlot != null)
                speedPotionSlot.UpdateSlot(0, 1, playerAbilities.SpeedPotionCount);
            if (attackPotionSlot != null)
                attackPotionSlot.UpdateSlot(0, 1, playerAbilities.AttackPotionCount);
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
        }

        if (playerStats != null)
        {
            playerStats.ExperienceChanged +=
                UpdateExperience;

            playerStats.LevelChanged += UpdateLevel;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealth;
        }

        if (playerStats != null)
        {
            playerStats.ExperienceChanged -=
                UpdateExperience;

            playerStats.LevelChanged -= UpdateLevel;
        }
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            UpdateHealth(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth
            );
        }

        if (playerStats != null)
        {
            UpdateExperience(
                playerStats.CurrentExperience,
                playerStats.ExperienceToNextLevel
            );

            UpdateLevel(playerStats.Level);
        }
    }

    private void Update()
    {
        if (runTimer == null || timerText == null)
        {
            return;
        }

        int totalSeconds =
            Mathf.FloorToInt(runTimer.ElapsedTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateHealth(
        int currentHealth,
        int maximumHealth
    )
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maximumHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{currentHealth} / {maximumHealth}";
        }
    }

    private void UpdateExperience(
        int currentExperience,
        int requiredExperience
    )
    {
        if (experienceSlider != null)
        {
            experienceSlider.maxValue =
                requiredExperience;

            experienceSlider.value =
                currentExperience;
        }

        if (experienceText != null)
        {
            experienceText.text =
                $"XP {currentExperience} / " +
                $"{requiredExperience}";
        }
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }
}