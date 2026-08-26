using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default BGM Clip")]
    [SerializeField] private AudioClip defaultBgmClip;

    private const string BGM_KEY = "BGM_Volume";
    private const string SFX_KEY = "SFX_Volume";

    private void Awake()
    {
        // Singleton pattern: Ensure only one AudioManager exists across scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-add AudioSource components if missing
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
    }

    private void Start()
    {
        // Load saved volume preferences (default to 1.0 if not saved)
        float savedBGMVolume = PlayerPrefs.GetFloat(BGM_KEY, 1.0f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 1.0f);

        SetBGMVolume(savedBGMVolume);
        SetSFXVolume(savedSFXVolume);

        if (defaultBgmClip != null && !bgmSource.isPlaying)
        {
            PlayBGM(defaultBgmClip);
        }
    }

    // --- BGM Controls ---
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGM_KEY, volume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(BGM_KEY, 1.0f);
    }

    // --- SFX Controls (Ready for future implementation) ---
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_KEY, volume);
        PlayerPrefs.Save();
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_KEY, 1.0f);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}