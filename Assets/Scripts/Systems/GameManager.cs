using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private RunTimer runTimer;

    [Header("Run Settings")]
    [SerializeField, Min(1f)]
    private float targetSurvivalTime = 300f;

    [Header("Result UI")]
    [SerializeField]
    private GameObject endPanel;

    [SerializeField]
    private TMP_Text resultTitleText;

    [SerializeField]
    private TMP_Text resultStatsText;

    [SerializeField]
    private Button restartButton;

    private bool runHasEnded;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (
            runHasEnded ||
            runTimer == null
        )
        {
            return;
        }

        if (
            runTimer.ElapsedTime >=
            targetSurvivalTime
        )
        {
            EndRun(true);
        }
    }

    private void HandlePlayerDeath()
    {
        EndRun(false);
    }

    private void EndRun(bool playerWon)
    {
        if (runHasEnded)
        {
            return;
        }

        runHasEnded = true;

        if (runTimer != null)
        {
            runTimer.StopTimer();
        }

        UpdateResultUI(playerWon);

        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void UpdateResultUI(bool playerWon)
    {
        if (resultTitleText != null)
        {
            if (playerWon)
            {
                resultTitleText.text = "VICTORY!";
                resultTitleText.color =
                    new Color(1f, 0.82f, 0.3f);
            }
            else
            {
                resultTitleText.text = "GAME OVER";
                resultTitleText.color =
                    new Color(1f, 0.42f, 0.42f);
            }
        }

        int totalSeconds = 0;

        if (runTimer != null)
        {
            totalSeconds =
                Mathf.FloorToInt(
                    runTimer.ElapsedTime
                );
        }

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        int reachedLevel = 1;

        if (playerStats != null)
        {
            reachedLevel = playerStats.Level;
        }

        if (resultStatsText != null)
        {
            resultStatsText.text =
                $"Survival Time: " +
                $"{minutes:00}:{seconds:00}\n" +
                $"Level Reached: {reachedLevel}";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}