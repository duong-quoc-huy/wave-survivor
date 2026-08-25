using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay; // Image Type must be set to "Filled"
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text keyText; // Shows "E" or "Q"
    [SerializeField] private TMP_Text countText; // Shows potion/item quantity (e.g., "3")

    private float cooldownDuration;
    private float currentCooldown = 0f;

    public bool IsOnCooldown => currentCooldown > 0f;

    public void SetupSlot(Sprite icon, string keyLabel)
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }

        if (keyText != null)
        {
            keyText.text = keyLabel;
        }

        ResetCooldown();
    }

    // Called by HUDController to sync remaining cooldowns and item counts
    public void UpdateSlot(float remainingCooldown, float totalCooldown, int stackCount = -1)
    {
        currentCooldown = Mathf.Max(0f, remainingCooldown);
        cooldownDuration = totalCooldown;

        if (cooldownDuration > 0f && currentCooldown > 0f)
        {
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = currentCooldown / cooldownDuration;

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

        // Display stack count if provided (used for potions)
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
                cooldownOverlay.fillAmount = currentCooldown / cooldownDuration;

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
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }
}