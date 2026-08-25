using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    private enum UpgradeType
    {
        MoveSpeed,
        AttackSpeed,
        ProjectileDamage,
        AttackRange,
        MaxHealth
    }

    [Header("Player References")]
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private WeaponController weaponController;

    [Header("UI References")]
    [SerializeField]
    private GameObject upgradePanel;

    [SerializeField]
    private Button[] choiceButtons;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.LeveledUp += ShowUpgradeChoices;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.LeveledUp -= ShowUpgradeChoices;
        }

        Time.timeScale = 1f;
    }

    private void ShowUpgradeChoices(int newLevel)
    {
        if (
            upgradePanel == null ||
            choiceButtons == null ||
            choiceButtons.Length < 3
        )
        {
            Debug.LogError(
                "UpgradeManager UI references are incomplete.",
                this
            );

            return;
        }

        List<UpgradeType> choices =
            CreateShuffledUpgradeList();

        upgradePanel.SetActive(true);
        Time.timeScale = 0f;

        for (int i = 0; i < 3; i++)
        {
            UpgradeType selectedUpgrade = choices[i];
            Button button = choiceButtons[i];

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    GetUpgradeDescription(selectedUpgrade);
            }

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                () => SelectUpgrade(selectedUpgrade)
            );
        }

        Debug.Log(
            $"Showing upgrade choices for level {newLevel}.",
            this
        );
    }

    private List<UpgradeType>
        CreateShuffledUpgradeList()
    {
        List<UpgradeType> upgrades =
            new List<UpgradeType>
            {
                UpgradeType.MoveSpeed,
                UpgradeType.AttackSpeed,
                UpgradeType.ProjectileDamage,
                UpgradeType.AttackRange,
                UpgradeType.MaxHealth
            };

        for (int i = upgrades.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            UpgradeType temporary = upgrades[i];
            upgrades[i] = upgrades[randomIndex];
            upgrades[randomIndex] = temporary;
        }

        return upgrades;
    }

    private string GetUpgradeDescription(
        UpgradeType upgrade
    )
    {
        switch (upgrade)
        {
            case UpgradeType.MoveSpeed:
                return "SWIFT BOOTS\nMove Speed +0.5";

            case UpgradeType.AttackSpeed:
                return "RAPID FIRE\nAttack Speed +15%";

            case UpgradeType.ProjectileDamage:
                return "SHARPENED DAGGERS\nDamage +1";

            case UpgradeType.AttackRange:
                return "LONG REACH\nAttack Range +1.5";

            case UpgradeType.MaxHealth:
                return "VITALITY\nMax HP +2";

            default:
                return "Unknown Upgrade";
        }
    }

    private void SelectUpgrade(UpgradeType upgrade)
    {
        ApplyUpgrade(upgrade);

        Debug.Log($"Selected upgrade: {upgrade}", this);

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ApplyUpgrade(UpgradeType upgrade)
    {
        switch (upgrade)
        {
            case UpgradeType.MoveSpeed:
                playerController.IncreaseMoveSpeed(0.5f);
                break;

            case UpgradeType.AttackSpeed:
                weaponController.IncreaseAttackSpeed(0.15f);
                break;

            case UpgradeType.ProjectileDamage:
                weaponController.IncreaseProjectileDamage(1);
                break;

            case UpgradeType.AttackRange:
                weaponController.IncreaseAttackRange(1.5f);
                break;

            case UpgradeType.MaxHealth:
                playerHealth.IncreaseMaxHealth(2);
                break;
        }
    }

    public void BindPlayer(GameObject player)
    {
        if (player == null) return;

        if (playerStats != null)
        {
            playerStats.LeveledUp -= ShowUpgradeChoices;
        }

        playerStats = player.GetComponent<PlayerStats>();
        playerController = player.GetComponent<PlayerController>();
        playerHealth = player.GetComponent<PlayerHealth>();
        weaponController = player.GetComponent<WeaponController>();

        // Subscribe to level up event
        if (playerStats != null)
        {
            playerStats.LeveledUp += ShowUpgradeChoices;
        }
    }
}