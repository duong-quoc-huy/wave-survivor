using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterDetailUI : MonoBehaviour
{
    [Header("Panel Container")]
    [SerializeField] private GameObject panelContainer;

    [Header("UI References")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text speedText;

    [Header("Data Registries")]
    [SerializeField] private CharacterData[] allCharacterData;
    [SerializeField] private WeaponData[] allWeaponData;

    private CharacterData currentCharacterData;
    private WeaponData currentWeaponData;
    private PlayerStats runtimePlayerStats;
    private bool isOpen = false;


    private void Start()
    {
        if (panelContainer != null)
            panelContainer.SetActive(false);

        FetchEquippedData();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            TogglePanel();
        }

     
        if (isOpen)
        {
            if (runtimePlayerStats == null)
            {
                FindRuntimePlayer();
            }
            RefreshStats();
            
        }
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;

        if (panelContainer != null)
            panelContainer.SetActive(isOpen);

        if (isOpen)
        {
            FetchEquippedData();
            FindRuntimePlayer();
            RefreshStats();
        }
    }

    private void FindRuntimePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            runtimePlayerStats = player.GetComponent<PlayerStats>();
        }

        Debug.Log($"[CharacterDetailUI] Bound to {player.name} (ID: {runtimePlayerStats.GetInstanceID()})");
    }

    public void FetchEquippedData()
    {
        string equippedCharId = LocalSaveSystem.GetEquippedCharacter();
        currentCharacterData = allCharacterData.FirstOrDefault(c => c != null && c.characterId.Equals(equippedCharId, System.StringComparison.OrdinalIgnoreCase));

        if (currentCharacterData == null && allCharacterData.Length > 0)
            currentCharacterData = allCharacterData[0];

        string equippedWeaponId = LocalSaveSystem.GetEquippedWeapon();
        currentWeaponData = allWeaponData.FirstOrDefault(w => w != null && w.weaponId.Equals(equippedWeaponId, System.StringComparison.OrdinalIgnoreCase));

        if (currentWeaponData == null && allWeaponData.Length > 0)
            currentWeaponData = allWeaponData[0];
    }

    public void RefreshStats()
    {
        if (currentCharacterData == null) return;

        // Fetch runtime multipliers if player exists in stage
        float atkMultiplier = runtimePlayerStats != null ? runtimePlayerStats.AtkPercentMultiplier : 0f;

        Debug.Log($"[CharacterDetailUI] Live AtkMultiplier: {atkMultiplier}");
        float speedMultiplier = runtimePlayerStats != null ? runtimePlayerStats.SpeedPercentMultiplier : 0f;
        int bonusSkillAtk = runtimePlayerStats != null ? runtimePlayerStats.BonusAttack : 0;

        // --- 1. HP DISPLAY ---
        float baseHP = currentCharacterData.baseMaxHP;
        float totalHP = StatCalculator.GetTotalHP(currentCharacterData);
        float hpBonus = totalHP - baseHP;

        hpText.text = hpBonus > 0
            ? $"HP BASE: {baseHP} (Current: {baseHP} + {hpBonus} = {totalHP})"
            : $"HP BASE: {baseHP}";

        // --- 2. ATK DISPLAY ---
        float baseATK = currentCharacterData.baseAttack;
        float flatTotalATK = StatCalculator.GetTotalAttack(currentCharacterData, currentWeaponData) + bonusSkillAtk;
        float finalATK = flatTotalATK * (1f + atkMultiplier);
        float flatBonus = flatTotalATK - baseATK;

        if (atkMultiplier > 0f)
        {
            
            atkText.text = $"ATK BASE: {baseATK} (Current: {finalATK:F1})";
        }
        else if (flatBonus > 0)
        {
            
            atkText.text = $"ATK BASE: {baseATK} (Current: {baseATK} + {flatBonus} = {flatTotalATK})";
        }
        else
        {
            atkText.text = $"ATK BASE: {baseATK}";
        }

        // --- 3. SPEED DISPLAY ---
        float baseSpeed = currentCharacterData.baseMoveSpeed;
        float flatTotalSpeed = StatCalculator.GetTotalSpeed(currentCharacterData);
        float finalSpeed = flatTotalSpeed * (1f + speedMultiplier);
        float speedBonus = flatTotalSpeed - baseSpeed;

        if (speedMultiplier > 0f)
        {
            speedText.text = $"SPEED BASE: {baseSpeed} (Current: {finalSpeed:F1})";
        }
        else if (speedBonus > 0)
        {
            speedText.text = $"SPEED BASE: {baseSpeed} (Current: {baseSpeed} + {speedBonus} = {flatTotalSpeed})";
        }
        else
        {
            speedText.text = $"SPEED BASE: {baseSpeed}";
        }
    }
}