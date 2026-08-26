using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private Button slotButton;

    private void Awake()
    {
        if (slotButton == null) slotButton = GetComponent<Button>();
    }

    public void Setup(Sprite icon, int quantity, bool isSelected, System.Action onClick)
    {
        if (slotButton == null) slotButton = GetComponent<Button>();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? $"x{quantity}" : "";
            quantityText.gameObject.SetActive(quantity > 1);
        }

        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            if (onClick != null) slotButton.onClick.AddListener(() => onClick.Invoke());
        }
    }
}