using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterDetailUI : MonoBehaviour
{
    [Header("Panel Container")]
    [SerializeField] private GameObject panelContainer; // Points to CharacterDetailPanel

    [Header("UI References")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text speedText;

    [Header("Data Registries")]
    [SerializeField] private CharacterData[] allCharacterData;
    [SerializeField] private WeaponData[] allWeaponData;

    private CharacterData currentCharacterData;
    private WeaponData currentWeaponData;
    private bool isOpen = false;

    private void Start()
    {
        // Start hidden
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
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;

        if (panelContainer != null)
            panelContainer.SetActive(isOpen);

        if (isOpen)
        {
            FetchEquippedData();
            RefreshStats();
        }
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

        float baseHP = currentCharacterData.baseMaxHP;
        float totalHP = StatCalculator.GetTotalHP(currentCharacterData);
        float hpBonus = totalHP - baseHP;
        hpText.text = hpBonus > 0
            ? $"HP BASE: {baseHP} (Current: {baseHP} + {hpBonus} = {totalHP})"
            : $"HP BASE: {baseHP}";

        float baseATK = currentCharacterData.baseAttack;
        float totalATK = StatCalculator.GetTotalAttack(currentCharacterData, currentWeaponData);
        float atkBonus = totalATK - baseATK;
        atkText.text = atkBonus > 0
            ? $"ATK BASE: {baseATK} (Current: {baseATK} + {atkBonus} = {totalATK})"
            : $"ATK BASE: {baseATK}";

        float baseSpeed = currentCharacterData.baseMoveSpeed;
        float totalSpeed = StatCalculator.GetTotalSpeed(currentCharacterData);
        float speedBonus = totalSpeed - baseSpeed;
        speedText.text = speedBonus > 0
            ? $"SPEED BASE: {baseSpeed} (Current: {baseSpeed} + {speedBonus} = {totalSpeed})"
            : $"SPEED BASE: {baseSpeed}";
    }
}