using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private RunTimer runTimer;

    [Header("Run Settings")]
    [SerializeField, Min(1f)]
    private float targetSurvivalTime = 300f;

    [Header("Start Menu")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Result UI")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultStatsText;
    [SerializeField] private Button restartButton;

    private bool gameStarted;
    private bool runHasEnded;

    private void Awake()
    {
        Time.timeScale = 0f;

        gameStarted = false;
        runHasEnded = false;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
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
            playerHealth.Died += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= HandlePlayerDeath;
    }

    private void Update()
    {
        if (!gameStarted || runHasEnded || runTimer == null)
            return;

        if (runTimer.ElapsedTime >= targetSurvivalTime)
            EndRun(true);
    }

    private void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void HandlePlayerDeath()
    {
        if (gameStarted)
            EndRun(false);
    }

    private void EndRun(bool playerWon)
    {
        if (runHasEnded)
            return;

        runHasEnded = true;
        gameStarted = false;

        if (runTimer != null)
            runTimer.StopTimer();

        UpdateResultUI(playerWon);

        if (endPanel != null)
            endPanel.SetActive(true);

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

        int totalSeconds = runTimer != null
            ? Mathf.FloorToInt(runTimer.ElapsedTime)
            : 0;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        int reachedLevel = playerStats != null
            ? playerStats.Level
            : 1;

        if (resultStatsText != null)
        {
            resultStatsText.text =
                $"Survival Time: {minutes:00}:{seconds:00}\n" +
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

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}