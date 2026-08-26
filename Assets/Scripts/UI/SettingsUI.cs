using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmPercentText;
    [SerializeField] private TMP_Text sfxPercentText;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject settingsPanel;


    private void OnEnable()
    {
        Debug.Log("[SettingsUI] SettingsPanel was just activated!");
    }

    private void Start()
    {
        // Load initial values from AudioManager
        if (AudioManager.Instance != null)
        {
            float bgmVol = AudioManager.Instance.GetBGMVolume();
            float sfxVol = AudioManager.Instance.GetSFXVolume();

            if (bgmSlider != null)
            {
                bgmSlider.value = bgmVol;
                UpdateBGMText(bgmVol);
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVol;
                UpdateSFXText(sfxVol);
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseSettings);
        }
    }

    public void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
        UpdateBGMText(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        UpdateSFXText(value);
    }

    private void UpdateBGMText(float value)
    {
        if (bgmPercentText != null)
            bgmPercentText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    private void UpdateSFXText(float value)
    {
        if (sfxPercentText != null)
            sfxPercentText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}