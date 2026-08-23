using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private string stageSelectionSceneName = "StageSelectionScene";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void SelectCharacterByIndex(int index)
    {
        if (index < 0 || index >= characterPrefabs.Length || characterPrefabs[index] == null)
        {
            Debug.LogError($"Character selection index {index} is invalid or unassigned.", this);
            return;
        }

        StageRunContext.SelectCharacter(characterPrefabs[index]);
        SceneManager.LoadScene(stageSelectionSceneName);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}