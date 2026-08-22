using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageCardUI : MonoBehaviour
{
    [Header("Card References")]
    [SerializeField] private Button stageButton;
    [SerializeField] private TMP_Text stageNumberText;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject lockOverlay;

    [Header("Status Colors")]
    [SerializeField]
    private Color completedColor =
        new Color32(85, 201, 139, 255);

    [SerializeField]
    private Color availableColor =
        new Color32(255, 209, 92, 255);

    [SerializeField]
    private Color lockedColor =
        new Color32(190, 198, 210, 255);

    private int stageId;
    private Action<int> selectedCallback;

    public int StageId => stageId;

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();

        if (stageButton != null)
        {
            stageButton.onClick.RemoveListener(
                HandleSelected
            );

            stageButton.onClick.AddListener(
                HandleSelected
            );
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
    }
#endif

    public void Configure(
        int newStageId,
        string displayName,
        bool isUnlocked,
        bool isCompleted,
        float bestSurvivalTime,
        int highestLevel,
        Action<int> onSelected
    )
    {
        stageId = newStageId;
        selectedCallback = onSelected;

        if (stageNumberText != null)
        {
            stageNumberText.text =
                $"STAGE {stageId}";
        }

        if (stageNameText != null)
        {
            stageNameText.text =
                displayName.ToUpperInvariant();
        }

        UpdateProgress(
            bestSurvivalTime,
            highestLevel
        );

        UpdateStatus(
            isUnlocked,
            isCompleted
        );

        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked);
        }

        if (stageButton != null)
        {
            stageButton.interactable = isUnlocked;
        }
    }

    private void UpdateProgress(
        float bestSurvivalTime,
        int highestLevel
    )
    {
        if (progressText == null)
            return;

        int safeLevel = Mathf.Max(
            1,
            highestLevel
        );

        if (bestSurvivalTime <= 0f)
        {
            progressText.text =
                $"NO RECORD\nLEVEL {safeLevel}";

            return;
        }

        int totalSeconds = Mathf.FloorToInt(
            bestSurvivalTime
        );

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        progressText.text =
            $"BEST {minutes:00}:{seconds:00}\n" +
            $"LEVEL {safeLevel}";
    }

    private void UpdateStatus(
        bool isUnlocked,
        bool isCompleted
    )
    {
        if (statusText == null)
            return;

        if (!isUnlocked)
        {
            statusText.text = "LOCKED";
            statusText.color = lockedColor;
            return;
        }

        if (isCompleted)
        {
            statusText.text = "COMPLETED";
            statusText.color = completedColor;
            return;
        }

        statusText.text = "AVAILABLE";
        statusText.color = availableColor;
    }

    private void HandleSelected()
    {
        if (stageButton == null ||
            !stageButton.interactable)
        {
            return;
        }

        selectedCallback?.Invoke(stageId);
    }

    private void CacheReferences()
    {
        if (stageButton == null)
        {
            stageButton =
                GetComponent<Button>();
        }

        if (stageNumberText == null)
        {
            Transform child =
                transform.Find("StageNumberText");

            if (child != null)
            {
                stageNumberText =
                    child.GetComponent<TMP_Text>();
            }
        }

        if (stageNameText == null)
        {
            Transform child =
                transform.Find("StageNameText");

            if (child != null)
            {
                stageNameText =
                    child.GetComponent<TMP_Text>();
            }
        }

        if (progressText == null)
        {
            Transform child =
                transform.Find("ProgressText");

            if (child != null)
            {
                progressText =
                    child.GetComponent<TMP_Text>();
            }
        }

        if (statusText == null)
        {
            Transform child =
                transform.Find("StatusText");

            if (child != null)
            {
                statusText =
                    child.GetComponent<TMP_Text>();
            }
        }

        if (lockOverlay == null)
        {
            Transform child =
                transform.Find("LockOverlay");

            if (child != null)
            {
                lockOverlay = child.gameObject;
            }
        }
    }

    private void OnDestroy()
    {
        if (stageButton != null)
        {
            stageButton.onClick.RemoveListener(
                HandleSelected
            );
        }
    }
}