using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("Text")]
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text messageText;

    [Header("Buttons")]
    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private TMP_Text confirmButtonText;

    [SerializeField]
    private Button cancelButton;

    private Action confirmAction;
    private Action cancelAction;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(
                HandleConfirm
            );

            confirmButton.onClick.AddListener(
                HandleConfirm
            );
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(
                HandleCancel
            );

            cancelButton.onClick.AddListener(
                HandleCancel
            );
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(
                HandleConfirm
            );
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(
                HandleCancel
            );
        }
    }

    public void Show(
        string title,
        string message,
        string confirmLabel,
        Action onConfirm,
        Action onCancel = null
    )
    {
        confirmAction = onConfirm;
        cancelAction = onCancel;

        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        if (confirmButtonText != null)
            confirmButtonText.text = confirmLabel;

        gameObject.SetActive(true);

        if (confirmButton != null)
            confirmButton.Select();
    }

    public void Hide()
    {
        confirmAction = null;
        cancelAction = null;

        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        HandleCancel();
    }

    private void HandleConfirm()
    {
        Action savedAction = confirmAction;

        Hide();

        savedAction?.Invoke();
    }

    private void HandleCancel()
    {
        Action savedAction = cancelAction;

        Hide();

        savedAction?.Invoke();
    }
}