using UnityEngine;

public class StageRuntimeConfigurator : MonoBehaviour
{
    [Header("Stage Configurations")]
    [SerializeField]
    private StageConfiguration[] stageConfigurations;

    [Header("Gameplay References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Arena Visual References")]
    [SerializeField]
    private ArenaFloorGenerator floorGenerator;

    [SerializeField]
    private ArenaDecorationGenerator decorationGenerator;

    private void Awake()
    {
        ApplySelectedStage();
    }

    private void ApplySelectedStage()
    {
        int selectedStageId = StageRunContext.SelectedStageId;

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

        ApplyGameplayConfiguration(configuration);
        ApplyArenaVisuals(configuration);

        Debug.Log(
            $"Applied Stage {configuration.StageId}: " +
            $"{configuration.StageName}."
        );
    }

    private void ApplyGameplayConfiguration(StageConfiguration configuration)
    {
        if (gameManager != null)
            gameManager.ConfigureStage(configuration);
        else
            Debug.LogError(
                "StageRuntimeConfigurator is missing " +
                "the GameManager reference.",
                this
            );

        if (enemySpawner != null)
            enemySpawner.ConfigureStage(configuration);
        else
            Debug.LogError(
                "StageRuntimeConfigurator is missing " +
                "the EnemySpawner reference.",
                this
            );
    }

    private void ApplyArenaVisuals(StageConfiguration configuration)
    {
        if (floorGenerator != null)
        {
            floorGenerator.ConfigureTheme(
                configuration.FloorBaseTile,
                configuration.FloorVariationTile,
                configuration.FloorBorderTile,
                configuration.FloorTint,
                configuration.FloorVariationChance,
                configuration.VisualRandomSeed
            );

            floorGenerator.ConfigureSize(
                configuration.ArenaHalfSize
            );

            floorGenerator.GenerateFloor();
        }
        else
        {
            Debug.LogWarning(
                "StageRuntimeConfigurator has no " +
                "ArenaFloorGenerator reference.",
                this
            );
        }

        if (decorationGenerator != null)
        {
            decorationGenerator.ConfigureTheme(
                configuration.DecorationTileA,
                configuration.DecorationTileB,
                configuration.DecorationTileC,
                configuration.DecorationTint
            );

            decorationGenerator.GenerateDecorations();
        }
        else
        {
            Debug.LogWarning(
                "StageRuntimeConfigurator has no " +
                "ArenaDecorationGenerator reference.",
                this
            );
        }
    }

    private StageConfiguration FindConfiguration(int stageId)
    {
        if (stageConfigurations == null)
            return null;

        foreach (StageConfiguration configuration in stageConfigurations)
        {
            if (configuration != null && configuration.StageId == stageId)
            {
                return configuration;
            }
        }

        return null;
    }
}