using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private string stageSelectionSceneName = "StageSelectScene";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    /// <summary>
    /// Unified selection method called by UI Card Buttons.
    /// </summary>
    public void SelectCharacter(int characterIndex)
    {
        if (characterPrefabs == null || characterIndex < 0 || characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError($"Character selection index {characterIndex} is invalid or unassigned.", this);
            return;
        }

        GameObject chosenPrefab = characterPrefabs[characterIndex];
        if (chosenPrefab == null)
        {
            Debug.LogError($"Character prefab at index {characterIndex} is null!", this);
            return;
        }

        string selectedName = chosenPrefab.name;

        // 1. Update Persistent LocalSaveSystem & force disk write
        LocalSaveSystem.SetEquippedCharacter(selectedName);
        PlayerPrefs.Save();

        // 2. Update In-Memory StageRunContext if used by runtime managers
        StageRunContext.SelectCharacter(chosenPrefab);

        Debug.Log($"[CharacterSelection] Successfully saved & selected: '{selectedName}'");

        // 3. Load Stage Select Scene
        SceneManager.LoadScene(stageSelectionSceneName);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}