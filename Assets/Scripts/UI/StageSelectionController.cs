using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectionController : MonoBehaviour
{
    private const int TotalStageCount = 5;

    [Header("Stage Cards")]
    [SerializeField]
    private StageCardUI[] stageCards = new StageCardUI[TotalStageCount];

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    [SerializeField]
    private string gameSceneName = "GameScene";

    [SerializeField]
    private string mainMenuSceneName = "MainMenuScene";

    private static readonly string[] StageNames =
    {
        "BEGINNER ARENA",
        "HAUNTED HALL",
        "SPIDER NEST",
        "CURSED KEEP",
        "FINAL SANCTUM"
    };

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                ReturnToMainMenu
            );

            backButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }
    }

    private void Start()
    {
        RefreshStageCards();
    }

    public void OpenSkillTreeScene()
    {
        SceneManager.LoadScene("SkillTreeScene");
    }

    public void RefreshStageCards()
    {
        LocalSaveSystem.Reload();

        for (
            int index = 0;
            index < TotalStageCount;
            index++
        )
        {
            int stageId = index + 1;

            if (index >= stageCards.Length ||
                stageCards[index] == null)
            {
                Debug.LogWarning(
                    $"Stage Card {stageId} " +
                    "has not been assigned.",
                    this
                );

                continue;
            }

            StageProgressData progress =
                LocalSaveSystem.GetStageProgress(
                    stageId
                );

            bool isUnlocked =
                LocalSaveSystem.IsStageUnlocked(
                    stageId
                );

            stageCards[index].Configure(
                stageId,
                StageNames[index],
                isUnlocked,
                progress.completed,
                progress.bestSurvivalTime,
                progress.highestLevelReached,
                HandleStageSelected
            );
        }
    }

    private void HandleStageSelected(int stageId)
    {
        if (!LocalSaveSystem.IsStageUnlocked(
                stageId
            ))
        {
            Debug.LogWarning(
                $"Stage {stageId} is still locked."
            );

            return;
        }

        if (!StageRunContext.SelectStage(stageId))
            return;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            gameSceneName
        );
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private void OnDestroy()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                ReturnToMainMenu
            );
        }
    }
}