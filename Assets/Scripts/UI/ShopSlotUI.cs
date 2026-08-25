using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject emptyOverlay;

    private ShopItemData currentItem;
    private Action<ShopItemData> onPurchaseCallback;

    public void SetupSlot(ShopItemData item, Action<ShopItemData> onPurchase)
    {
        currentItem = item;
        onPurchaseCallback = onPurchase;

        if (item == null || item.itemType == ShopItemType.Empty)
        {
            if (emptyOverlay != null) emptyOverlay.SetActive(true);
            if (itemIcon != null) itemIcon.gameObject.SetActive(false);
            if (itemNameText != null) itemNameText.text = "";
            if (priceText != null) priceText.text = "";
            if (buyButton != null) buyButton.interactable = false;
            return;
        }

        if (emptyOverlay != null) emptyOverlay.SetActive(false);
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = item.itemIcon;
        }

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (priceText != null) priceText.text = $"{item.price} GOLD";

        if (buyButton != null)
        {
            buyButton.interactable = true;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        onPurchaseCallback?.Invoke(currentItem);
    }
}