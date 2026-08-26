using UnityEngine;

public class StageMusicController : MonoBehaviour
{
    [Header("Stage Audio Settings")]
    [SerializeField] private AudioClip stageBGM;



    private void Start()
    {
        if (AudioManager.Instance != null && stageBGM != null)
        {
            AudioManager.Instance.PlayBGM(stageBGM);
        }
    }
}