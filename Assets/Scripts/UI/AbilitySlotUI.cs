using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text countText;

    private float cooldownDuration;
    private float currentCooldown = 0f;

    private void Awake()
    {
        AutoFindChildReferences();
    }

    public void AutoFindChildReferences()
    {
        if (iconImage == null)
        {
            Transform t = transform.Find("IconImage");
            if (t != null) iconImage = t.GetComponent<Image>();
        }

        if (cooldownOverlay == null)
        {
            Transform t = transform.Find("CooldownOverlay");
            if (t != null) cooldownOverlay = t.GetComponent<Image>();
        }

        if (cooldownText == null)
        {
            Transform t = transform.Find("CooldownText");
            if (t != null) cooldownText = t.GetComponent<TMP_Text>();
        }

        if (keyText == null)
        {
            Transform t = transform.Find("KeyText");
            if (t == null) t = transform.Find("KeyBadge/KeyText");
            if (t != null) keyText = t.GetComponent<TMP_Text>();
        }

        if (countText == null)
        {
            Transform t = transform.Find("CountText");
            if (t != null) countText = t.GetComponent<TMP_Text>();
        }
    }

    public void SetupSlot(Sprite icon, string keyLabel)
    {
        AutoFindChildReferences();

        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }

        if (keyText != null)
        {
            keyText.text = keyLabel;
        }

        ResetCooldown();
    }

    // Called by HUDController to sync slot data
    public void UpdateSlot(float remainingCooldown, float totalCooldown, int stackCount = -1)
    {
        AutoFindChildReferences();

        currentCooldown = Mathf.Max(0f, remainingCooldown);
        cooldownDuration = totalCooldown;

        if (cooldownDuration > 0f && currentCooldown > 0f)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = currentCooldown / cooldownDuration;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.CeilToInt(currentCooldown).ToString();
            }
        }
        else
        {
            ResetCooldown();
        }

        if (countText != null)
        {
            if (stackCount >= 0)
            {
                countText.gameObject.SetActive(true);
                countText.text = stackCount.ToString();
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }
    }

    public void TriggerCooldown(float duration)
    {
        cooldownDuration = duration;
        currentCooldown = duration;
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = cooldownDuration > 0 ? (currentCooldown / cooldownDuration) : 0f;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = Mathf.CeilToInt(currentCooldown).ToString();
            }

            if (currentCooldown <= 0f)
            {
                ResetCooldown();
            }
        }
    }

    private void ResetCooldown()
    {
        currentCooldown = 0f;
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
}