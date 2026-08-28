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

    [Header("Stage Progress")]
    [SerializeField, Range(1, 5)]
    private int currentStageId = 1;

    [Header("Run Settings")]
    [SerializeField, Min(1f)]
    private float targetSurvivalTime = 300f;

    [Header("Result UI")]
    [SerializeField]
    private GameObject endPanel;

    [SerializeField]
    private Image resultWindowImage;

    [SerializeField]
    private Image resultIconImage;

    [SerializeField]
    private TMP_Text resultTitleText;

    [SerializeField]
    private TMP_Text resultStatsText;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;

    [Header("Result Artwork")]
    [SerializeField]
    private Sprite victoryWindowSprite;

    [SerializeField]
    private Sprite defeatWindowSprite;

    [SerializeField]
    private Sprite victoryIconSprite;

    [SerializeField]
    private Sprite defeatIconSprite;

    [SerializeField]
    private Button nextStageButton;

    [Header("Scene Navigation")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenuScene";

    private const int TotalStageCount = 5;

    private bool gameStarted;
    private bool runHasEnded;
    private bool lastRunWasVictory;


    private void PrepareSelectedStage()
    {
        int selectedStageId =
            StageRunContext.SelectedStageId;

        if (!LocalSaveSystem.IsStageUnlocked(
                selectedStageId
            ))
        {
            Debug.LogWarning(
                $"Selected Stage {selectedStageId} " +
                "is locked. Stage 1 will be used."
            );

            selectedStageId = 1;
            StageRunContext.SelectStage(1);
        }

        currentStageId = selectedStageId;

        Debug.Log(
            $"Starting Stage {currentStageId}."
        );
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        PrepareSelectedStage();

        gameStarted = true;
        runHasEnded = false;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveAllListeners();
            nextStageButton.onClick.AddListener(GoToNextStage);

            nextStageButton.gameObject.SetActive(false);
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
        if (!gameStarted ||
            runHasEnded ||
            runTimer == null)
        {
            return;
        }

        if (runTimer.ElapsedTime >= targetSurvivalTime)
            EndRun(true);
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
        lastRunWasVictory = playerWon;

        if (runTimer != null)
            runTimer.StopTimer();

        RecordProgress(playerWon);
        UpdateResultUI(playerWon);

        if (endPanel != null)
            endPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void RecordProgress(bool playerWon)
    {
        float survivalTime = runTimer != null
            ? runTimer.ElapsedTime
            : 0f;

        int reachedLevel = playerStats != null
            ? playerStats.Level
            : 1;

        // Record stage clear count for gold decay calculations
        if (playerWon)
        {
            LocalSaveSystem.IncrementStageClear(currentStageId);
        }

        Debug.Log(
            $"Recording Stage {currentStageId}: " +
            $"won={playerWon}, " +
            $"time={survivalTime:F2}, " +
            $"level={reachedLevel}"
        );

        LocalSaveSystem.RecordStageResult(
            currentStageId,
            playerWon,
            survivalTime,
            reachedLevel,
            false
        );
    }

    private void UpdateResultUI(bool playerWon)
    {
        UpdateResultArtwork(playerWon);
        UpdateNextStageButton(playerWon);


        if (resultTitleText != null)
        {
            if (playerWon)
            {
                resultTitleText.text = "VICTORY!";

                resultTitleText.color =
                    new Color32(
                        255,
                        211,
                        78,
                        255
                    );
            }
            else
            {
                resultTitleText.text = "GAME OVER";

                resultTitleText.color =
                    new Color32(
                        255,
                        107,
                        107,
                        255
                    );
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
                $"Survival Time: " +
                $"{minutes:00}:{seconds:00}\n" +
                $"Level Reached: {reachedLevel}";
        }
        if (playerWon && currentStageId == TotalStageCount)
        {
            resultStatsText.text +=
                "\nALL STAGES COMPLETED!";
        }
    }

    private void UpdateResultArtwork(bool playerWon)
    {
        Sprite selectedWindowSprite = playerWon
            ? victoryWindowSprite
            : defeatWindowSprite;

        Sprite selectedIconSprite = playerWon
            ? victoryIconSprite
            : defeatIconSprite;

        if (resultWindowImage != null &&
            selectedWindowSprite != null)
        {
            resultWindowImage.sprite =
                selectedWindowSprite;
        }

        if (resultIconImage != null &&
            selectedIconSprite != null)
        {
            resultIconImage.sprite =
                selectedIconSprite;

            resultIconImage.preserveAspect = true;
        }
    }

    public void ConfigureStage(StageConfiguration configuration)
    {
        if (configuration == null)
        {
            Debug.LogError(
                "GameManager received a null stage configuration.",
                this
            );

            return;
        }

        currentStageId = configuration.StageId;
        targetSurvivalTime =
            configuration.SurvivalTime;

        Debug.Log(
            $"GameManager configured for Stage " +
            $"{currentStageId}: " +
            $"{configuration.StageName}. " +
            $"Survival time: " +
            $"{targetSurvivalTime} seconds."
        );
    }

    private void UpdateNextStageButton(bool playerWon)
    {
        if (nextStageButton == null)
            return;

        int nextStageId = currentStageId + 1;

        bool canGoToNextStage =
            playerWon &&
            nextStageId <= TotalStageCount &&
            LocalSaveSystem.IsStageUnlocked(nextStageId);

        nextStageButton.gameObject.SetActive(canGoToNextStage);
        nextStageButton.interactable = canGoToNextStage;
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void GoToNextStage()
    {
        if (!runHasEnded || !lastRunWasVictory)
        {
            Debug.LogWarning(
                "Next Stage can only be used after a victory.",
                this
            );

            return;
        }

        int nextStageId = currentStageId + 1;

        if (nextStageId > TotalStageCount)
        {
            Debug.Log(
                "The player has completed the final stage.",
                this
            );

            return;
        }

        if (!LocalSaveSystem.IsStageUnlocked(nextStageId))
        {
            Debug.LogWarning(
                $"Stage {nextStageId} has not been unlocked.",
                this
            );

            return;
        }

        if (!StageRunContext.SelectStage(nextStageId))
        {
            Debug.LogError(
                $"Failed to select Stage {nextStageId}.",
                this
            );

            return;
        }

        // Prevent accidental double taps on mobile.
        if (nextStageButton != null)
            nextStageButton.interactable = false;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }


    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }






    public void BindPlayerReferences(GameObject player)
    {
        if (player == null) return;

        playerHealth = player.GetComponent<PlayerHealth>();
        playerStats = player.GetComponent<PlayerStats>();

        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDeath;
            playerHealth.Died += HandlePlayerDeath;
        }

        Debug.Log("GameManager successfully bound to spawned player.", this);
    }
}