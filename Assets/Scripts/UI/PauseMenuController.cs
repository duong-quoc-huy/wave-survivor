using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private Button pauseButton;

    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button retryButton;

    [SerializeField]
    private Button mainMenuButton;

    [Header("Confirmation UI")]
    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    [Header("Scene Navigation")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenuScene";

    private bool isPaused;

    private void Awake()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (confirmationDialog != null)
            confirmationDialog.Hide();

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(
                RequestRetry
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(
                RequestMainMenu
            );
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (confirmationDialog != null &&
                confirmationDialog.IsOpen)
            {
                confirmationDialog.Cancel();
                return;
            }

            TogglePause();
        }

        if (!isPaused && pauseButton != null)
        {
            pauseButton.interactable =
                Time.timeScale > 0f;
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        // Do not open Pause while another screen,
        // such as Level Up or End Panel, paused the game.
        if (isPaused || Time.timeScale <= 0f)
            return;

        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        if (confirmationDialog != null &&
            confirmationDialog.IsOpen)
        {
            confirmationDialog.Hide();
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            pauseButton.interactable = true;
        }
    }

    private void RequestRetry()
    {
        if (!isPaused)
            return;

        if (confirmationDialog == null)
        {
            Debug.LogError(
                "PauseMenuController is missing " +
                "its ConfirmationDialog reference.",
                this
            );

            return;
        }

        confirmationDialog.Show(
            "RESTART RUN?",
            "Current run progress will be lost.\n" +
            "Do you want to restart?",
            "RESTART",
            RetryGame
        );
    }

    private void RequestMainMenu()
    {
        if (!isPaused)
            return;

        if (confirmationDialog == null)
        {
            Debug.LogError(
                "PauseMenuController is missing " +
                "its ConfirmationDialog reference.",
                this
            );

            return;
        }

        confirmationDialog.Show(
            "RETURN TO MAIN MENU?",
            "Current run progress will be lost.\n" +
            "Return to the Main Menu?",
            "MAIN MENU",
            ReturnToMainMenu
        );
    }

    public void RetryGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }
}