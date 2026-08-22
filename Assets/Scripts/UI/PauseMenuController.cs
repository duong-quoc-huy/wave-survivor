using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Navigation")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenuScene";

    private bool isPaused;

    private void Awake()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

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
            retryButton.onClick.AddListener(RetryGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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
        // Prevent Pause from opening while an upgrade
        // or result screen has already paused the game.
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