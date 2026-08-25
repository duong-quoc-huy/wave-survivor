using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text countText;

    public void UpdateSlot(float remaining, float total, int count = -1)
    {
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = total > 0 ? remaining / total : 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text = remaining > 0 ? Mathf.CeilToInt(remaining).ToString() : "";
        }

        if (countText != null)
        {
            countText.text = count >= 0 ? count.ToString() : "";
        }
    }
}