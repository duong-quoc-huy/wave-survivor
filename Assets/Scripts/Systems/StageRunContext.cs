using UnityEngine;

public static class StageRunContext
{
    private const int MinimumStageId = 1;
    private const int MaximumStageId = 5;

    public static int SelectedStageId
    {
        get;
        private set;
    } = 1;

    public static bool SelectStage(int stageId)
    {
        if (stageId < MinimumStageId ||
            stageId > MaximumStageId)
        {
            Debug.LogWarning(
                $"Cannot select invalid stage ID: {stageId}"
            );

            return false;
        }

        SelectedStageId = stageId;

        Debug.Log(
            $"Stage {SelectedStageId} selected."
        );

        return true;
    }
}