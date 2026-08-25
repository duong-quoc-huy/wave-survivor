using UnityEngine;

public class PlatformUIController : MonoBehaviour
{
    [Header("Platform-Specific UI")]
    [SerializeField]
    private GameObject mobileControls;

    private void Awake()
    {
        if (mobileControls == null)
        {
            Debug.LogWarning(
                "PlatformUIController has no Mobile Controls assigned.",
                this
            );

            return;
        }

        mobileControls.SetActive(
            ShouldShowMobileControls()
        );
    }

    private bool ShouldShowMobileControls()
    {
#if UNITY_EDITOR
        return SystemInfo.deviceType == DeviceType.Handheld;
#else
    return Application.isMobilePlatform;
#endif
    }
}