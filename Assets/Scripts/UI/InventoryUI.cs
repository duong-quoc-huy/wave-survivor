using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    public int quantity;
}

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Detail Panel References")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private Button useButton;

    private Sprite speedPotionIcon;
    private Sprite attackPotionIcon;

    private bool isOpen = false;
    private List<InventoryItemData> currentItems = new List<InventoryItemData>();
    private int selectedIndex = 0;

    private void Awake()
    {
        AutoLoadResources();
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (useButton != null) useButton.onClick.AddListener(UseSelectedItem);
    }

    private void AutoLoadResources()
    {
        // Dynamically fetch sprites from Assets/Resources/Icons/
        speedPotionIcon = Resources.Load<Sprite>("Icons/SpeedPotion");
        attackPotionIcon = Resources.Load<Sprite>("Icons/AttackPotion");

        if (speedPotionIcon == null) Debug.LogWarning("[Inventory] Missing 'SpeedPotion' sprite in Assets/Resources/Icons/");
        if (attackPotionIcon == null) Debug.LogWarning("[Inventory] Missing 'AttackPotion' sprite in Assets/Resources/Icons/");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Toggle Inventory Panel
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }

        // DEBUG CHEAT: Press 'P' to add 3 test potions to save file
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            LocalSaveSystem.AddPotion("SpeedPotion", 3);
            LocalSaveSystem.AddPotion("AttackPotion", 3);
            Debug.Log("[Debug] Added 3 Speed & Attack Potions to save data!");

            if (isOpen) RefreshInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
        {
            RefreshInventory();
        }
    }

    public void RefreshInventory()
    {
        FetchOwnedItems();
        PopulateGrid();

        if (currentItems.Count > 0)
        {
            SelectSlot(Mathf.Clamp(selectedIndex, 0, currentItems.Count - 1));
        }
        else
        {
            ClearDetails();
        }
    }

    private void FetchOwnedItems()
    {
        currentItems.Clear();

        int speedCount = LocalSaveSystem.GetPotionCount("SpeedPotion");
        if (speedCount > 0)
        {
            currentItems.Add(new InventoryItemData
            {
                itemId = "SpeedPotion",
                itemName = "Speed Potion",
                description = "Increases movement speed by 50% for 2.5 minutes.",
                icon = speedPotionIcon,
                quantity = speedCount
            });
        }

        int attackCount = LocalSaveSystem.GetPotionCount("AttackPotion");
        if (attackCount > 0)
        {
            currentItems.Add(new InventoryItemData
            {
                itemId = "AttackPotion",
                itemName = "Attack Potion",
                description = "Increases attack damage by 30% for 2.5 minutes.",
                icon = attackPotionIcon,
                quantity = attackCount
            });
        }
    }

    private void PopulateGrid()
    {
        if (gridContainer == null || slotPrefab == null) return;

        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        int totalSlots = Mathf.Max(15, currentItems.Count);

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, gridContainer);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            int index = i;
            if (i < currentItems.Count && slotUI != null)
            {
                var item = currentItems[i];
                bool isSelected = (index == selectedIndex);
                slotUI.Setup(item.icon, item.quantity, isSelected, () => SelectSlot(index));
            }
            else if (slotUI != null)
            {
                slotUI.Setup(null, 0, false, null);
            }
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= currentItems.Count) return;

        selectedIndex = index;
        PopulateGrid();

        var item = currentItems[selectedIndex];

        if (detailIcon != null)
        {
            detailIcon.sprite = item.icon;
            detailIcon.enabled = (item.icon != null);
        }

        if (detailTitleText != null) detailTitleText.text = item.itemName;
        if (detailDescriptionText != null) detailDescriptionText.text = $"\"{item.description}\"";
        if (useButton != null) useButton.interactable = true;
    }

    private void ClearDetails()
    {
        if (detailIcon != null) detailIcon.enabled = false;
        if (detailTitleText != null) detailTitleText.text = "Empty Inventory";
        if (detailDescriptionText != null) detailDescriptionText.text = "No items available in your bag.";
        if (useButton != null) useButton.interactable = false;
    }

    private void UseSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= currentItems.Count) return;

        var item = currentItems[selectedIndex];
        LocalSaveSystem.AddPotion(item.itemId, -1);

        Debug.Log($"Used 1x {item.itemName}.");

        RefreshInventory();
    }
}