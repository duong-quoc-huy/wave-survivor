using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private RunTimer runTimer;

    [Header("UI References")]
    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private TMP_Text healthText;

    [SerializeField]
    private TMP_Text timerText;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
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
    }



    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealth;
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
}