using UnityEngine;

public class StageRuntimeConfigurator : MonoBehaviour
{
    [Header("Stage Configurations")]
    [SerializeField]
    private StageConfiguration[] stageConfigurations;

    [Header("Game References")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private EnemySpawner enemySpawner;

    private void Awake()
    {
        ApplySelectedStage();
    }

    private void ApplySelectedStage()
    {
        int selectedStageId =
            StageRunContext.SelectedStageId;

        if (!LocalSaveSystem.IsStageUnlocked(
                selectedStageId
            ))
        {
            Debug.LogWarning(
                $"Stage {selectedStageId} is locked. " +
                "Stage 1 will be loaded instead.",
                this
            );

            selectedStageId = 1;
            StageRunContext.SelectStage(1);
        }

        StageConfiguration configuration =
            FindConfiguration(selectedStageId);

        if (configuration == null)
        {
            Debug.LogError(
                $"No configuration was found for " +
                $"Stage {selectedStageId}.",
                this
            );

            return;
        }

        if (gameManager != null)
        {
            gameManager.ConfigureStage(
                configuration
            );
        }
        else
        {
            Debug.LogError(
                "StageRuntimeConfigurator is missing " +
                "the GameManager reference.",
                this
            );
        }

        if (enemySpawner != null)
        {
            enemySpawner.ConfigureStage(
                configuration
            );
        }
        else
        {
            Debug.LogError(
                "StageRuntimeConfigurator is missing " +
                "the EnemySpawner reference.",
                this
            );
        }

        Debug.Log(
            $"Applied Stage {configuration.StageId}: " +
            $"{configuration.StageName}."
        );
    }

    private StageConfiguration FindConfiguration(
        int stageId
    )
    {
        if (stageConfigurations == null)
            return null;

        foreach (
            StageConfiguration configuration
            in stageConfigurations
        )
        {
            if (configuration != null &&
                configuration.StageId == stageId)
            {
                return configuration;
            }
        }

        return null;
    }
}