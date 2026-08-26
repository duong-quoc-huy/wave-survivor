using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    [Header("Grid Spawning")]
    [SerializeField] private Transform contentContainer; 
    [SerializeField] private GameObject shopSlotPrefab;  
    [SerializeField] private List<ShopItemData> shopItems = new List<ShopItemData>();

    [Header("General Shop UI")]
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text purchaseMessageText;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private const int TotalGridSlots = 30;
    private Coroutine messageCoroutine;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainMenu);
        }

        RefreshShop();
    }

    public void RefreshShop()
    {
        if (currencyText != null)
            currencyText.text = $"GOLD: {LocalSaveSystem.GetGold()}";

        GenerateGridSlots();
    }

    private void GenerateGridSlots()
    {
        if (contentContainer == null || shopSlotPrefab == null) return;

        
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        
        for (int i = 0; i < TotalGridSlots; i++)
        {
            GameObject slotObj = Instantiate(shopSlotPrefab, contentContainer);
            ShopSlotUI slotUI = slotObj.GetComponent<ShopSlotUI>();

            if (slotUI != null)
            {
                ShopItemData itemData = (i < shopItems.Count) ? shopItems[i] : new ShopItemData { itemType = ShopItemType.Empty };
                slotUI.SetupSlot(itemData, HandlePurchase);
            }
        }
    }

    private void HandlePurchase(ShopItemData item)
    {
        if (item == null) return;

        if (!LocalSaveSystem.SpendGold(item.price))
        {
            ShowMessage("NOT ENOUGH GOLD", new Color32(255, 107, 107, 255));
            return;
        }

        switch (item.itemType)
        {
            case ShopItemType.Potion_Attack:
                LocalSaveSystem.AddPotion("AttackPotion", 1);
                break;
            case ShopItemType.Potion_Speed:
                LocalSaveSystem.AddPotion("SpeedPotion", 1); 
                break;
            case ShopItemType.Weapon:
                LocalSaveSystem.SetEquippedWeapon(item.itemId);
                break;
            case ShopItemType.Character:
                LocalSaveSystem.SetEquippedCharacter(item.itemId);
                break;
        }

        ShowMessage($"{item.itemName} PURCHASED!", new Color32(102, 224, 153, 255));
        RefreshShop();
    }

    private void ShowMessage(string msg, Color color)
    {
        if (purchaseMessageText == null) return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowMessageRoutine(msg, color));
    }

    private IEnumerator ShowMessageRoutine(string msg, Color color)
    {
        purchaseMessageText.gameObject.SetActive(true);
        purchaseMessageText.text = msg;
        purchaseMessageText.color = color;

        yield return new WaitForSeconds(1f);

        purchaseMessageText.text = string.Empty;
        purchaseMessageText.gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}