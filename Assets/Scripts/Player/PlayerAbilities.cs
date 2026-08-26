using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

public class PlayerAbilities : MonoBehaviour
{
    [Header("Data Registries")]
    [SerializeField] private CharacterData[] allCharacterData;

    [Header("UI Slots (Auto-assigned at runtime)")]
    [SerializeField] private AbilitySlotUI eSkillSlot;
    [SerializeField] private AbilitySlotUI qSkillSlot;

    [Header("Cutscene UI (Auto-assigned at runtime)")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private RawImage cutsceneRawImage;
    [SerializeField] private VideoPlayer videoPlayer;

    private CharacterData characterData;
    private PlayerHealth playerHealth;
    private PlayerStats playerStats;

    private float skill1CooldownTimer = 0f;
    private float skill2CooldownTimer = 0f;
    private bool isExecutingSkill2 = false;

    private RenderTexture cutsceneTexture;

    public float ECooldownRemaining => skill1CooldownTimer;
    public float ECooldownTotal => characterData != null ? characterData.skill1Cooldown : 1f;
    public float QCooldownRemaining => skill2CooldownTimer;
    public float QCooldownTotal => characterData != null ? characterData.skill2Cooldown : 1f;
    public int SpeedPotionCount { get; private set; } = 3;
    public int AttackPotionCount { get; private set; } = 3;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        EnsurePlayerStatsBound();

        if (playerStats != null)
        {
            Debug.Log($"[PlayerAbilities] Bound to {gameObject.name} (ID: {playerStats.GetInstanceID()})");
        }
    }

    private void Start()
    {
        LoadCharacterData();
        AutoFindSceneReferences();
        InitializeUISlots();
        PrewarmCutsceneVideo();
    }

    private void EnsurePlayerStatsBound()
    {
        if (playerStats != null) return;

        // 1. Check if attached directly to the player GameObject
        playerStats = GetComponent<PlayerStats>();

        // 2. Fallback: Find runtime spawned player tagged "Player"
        if (playerStats == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerStats = playerObj.GetComponent<PlayerStats>();
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
    }

    private void AutoFindSceneReferences()
    {
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas != null)
        {
            if (eSkillSlot == null)
            {
                Transform eTrans = hudCanvas.transform.Find("ESkillSlot");
                if (eTrans != null) eSkillSlot = eTrans.GetComponent<AbilitySlotUI>();
            }

            if (qSkillSlot == null)
            {
                Transform qTrans = hudCanvas.transform.Find("QSkillSlot");
                if (qTrans != null) qSkillSlot = qTrans.GetComponent<AbilitySlotUI>();
            }

            if (cutscenePanel == null)
            {
                Transform cutsceneTrans = hudCanvas.transform.Find("CutsceneOverlay");
                if (cutsceneTrans != null)
                {
                    cutscenePanel = cutsceneTrans.gameObject;
                    cutsceneRawImage = cutscenePanel.GetComponent<RawImage>();
                    videoPlayer = cutscenePanel.GetComponent<VideoPlayer>();
                }
            }
        }
    }

    private void LoadCharacterData()
    {
        string equippedId = LocalSaveSystem.GetEquippedCharacter();
        characterData = allCharacterData.FirstOrDefault(c => c != null && c.characterId.Equals(equippedId, System.StringComparison.OrdinalIgnoreCase));

        if (characterData == null && allCharacterData.Length > 0)
            characterData = allCharacterData[0];
    }

    private void InitializeUISlots()
    {
        if (characterData == null) return;

        if (eSkillSlot != null)
            eSkillSlot.SetupSlot(characterData.skill1Icon, "E");

        if (qSkillSlot != null)
            qSkillSlot.SetupSlot(characterData.skill2Icon, "Q");
    }

    private void PrewarmCutsceneVideo()
    {
        if (videoPlayer == null || characterData == null || characterData.skill2CutsceneVideo == null) return;

        cutsceneTexture = new RenderTexture(1280, 720, 16);
        cutsceneTexture.Create();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = cutsceneTexture;
        videoPlayer.clip = characterData.skill2CutsceneVideo;

        if (cutsceneRawImage != null)
        {
            cutsceneRawImage.texture = cutsceneTexture;
            cutsceneRawImage.color = Color.white;
        }

        videoPlayer.Prepare();
    }

    private void Update()
    {
        if (isExecutingSkill2 || characterData == null) return;

        if (skill1CooldownTimer > 0f) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0f) skill2CooldownTimer -= Time.deltaTime;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame) UseSkill1();
            if (Keyboard.current.qKey.wasPressedThisFrame) UseSkill2();

            // --- POTION KEY BINDINGS ---
            if (Keyboard.current.digit1Key.wasPressedThisFrame) UseAttackPotion();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) UseSpeedPotion();
        }
    }

    public void UseSkill1()
    {
        if (skill1CooldownTimer > 0f || characterData == null) return;

        skill1CooldownTimer = characterData.skill1Cooldown;
        if (eSkillSlot != null) eSkillSlot.TriggerCooldown(characterData.skill1Cooldown);

        StartCoroutine(ExecuteSkill1Routine());
    }

    private IEnumerator ExecuteSkill1Routine()
    {
        EnsurePlayerStatsBound();

        switch (characterData.characterId.ToLower())
        {
            case "warrior":
            case "knight":
                if (playerHealth != null)
                {
                    playerHealth.ActivateDamageReduction(characterData.skill1Duration, 0.50f);
                    playerHealth.Heal(5);
                }

                if (playerStats != null)
                {
                    playerStats.ApplyTemporaryAttackBoost(5, characterData.skill1Duration);
                }

                yield return new WaitForSeconds(characterData.skill1Duration);
                break;

            case "mage":
                StunAllEnemies(5f);
                break;

            case "spellcaster":
                StartCoroutine(RegenRoutine(3f, 5f));
                break;
        }
    }

    public void UseSkill2()
    {
        if (skill2CooldownTimer > 0f || characterData == null || isExecutingSkill2) return;

        skill2CooldownTimer = characterData.skill2Cooldown;
        if (qSkillSlot != null) qSkillSlot.TriggerCooldown(characterData.skill2Cooldown);

        StartCoroutine(ExecuteSkill2Cutscene());
    }

    private IEnumerator ExecuteSkill2Cutscene()
    {
        isExecutingSkill2 = true;
        Time.timeScale = 0f;

        if (cutscenePanel != null && videoPlayer != null && characterData.skill2CutsceneVideo != null)
        {
            cutscenePanel.SetActive(true);
            videoPlayer.Play();

            yield return new WaitForSecondsRealtime(2.5f);

            videoPlayer.Stop();
            cutscenePanel.SetActive(false);
            videoPlayer.Prepare();
        }

        Time.timeScale = 1f;

        switch (characterData.characterId.ToLower())
        {
            case "warrior":
                DamageEnemiesByMaxHP(0.60f);
                break;
            case "knight":
            case "spellcaster":
                WipeAllEnemies();
                break;
            case "mage":
                DamageEnemiesByMaxHP(0.50f);
                StartCoroutine(RegenRoutine(3f, 10f));
                break;
        }

        isExecutingSkill2 = false;
    }

    private void StunAllEnemies(float duration)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null) enemy.ApplyStun(duration);
        }
    }

    public void UseAttackPotion()
    {
        if (AttackPotionCount <= 0)
        {
            Debug.LogWarning("[PlayerAbilities] Cannot use Attack Potion: Count is 0!");
            return;
        }

        EnsurePlayerStatsBound();

        if (playerStats != null)
        {
            AttackPotionCount--;
            playerStats.ApplyAttackPotion(0.30f, 150f);
            Debug.Log($"[PlayerAbilities] Potion applied to {playerStats.gameObject.name}! AtkMultiplier is now: {playerStats.AtkPercentMultiplier}");
        }
        else
        {
            Debug.LogError("[PlayerAbilities] Failed to apply potion: PlayerStats component not found on tagged Player!", this);
        }
    }

    public void UseSpeedPotion()
    {
        if (SpeedPotionCount <= 0)
        {
            Debug.LogWarning("[PlayerAbilities] Cannot use Speed Potion: Count is 0!");
            return;
        }

        EnsurePlayerStatsBound();

        if (playerStats != null)
        {
            SpeedPotionCount--;
            playerStats.ApplySpeedPotion(0.50f, 150f);
            Debug.Log($"[PlayerAbilities] Potion applied to {playerStats.gameObject.name}! SpeedMultiplier is now: {playerStats.SpeedPercentMultiplier}");
        }
        else
        {
            Debug.LogError("[PlayerAbilities] Failed to apply potion: PlayerStats component not found on tagged Player!", this);
        }
    }

    private void WipeAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(int.MaxValue);
            }
            else
            {
                Destroy(enemyObj);
            }
        }
    }

    private void DamageEnemiesByMaxHP(float percent)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                int damageAmount = Mathf.CeilToInt(enemy.MaxHealth * percent);
                enemy.TakeDamage(damageAmount);
            }
        }
    }

    private IEnumerator RegenRoutine(float hpPerSec, float totalDuration)
    {
        float elapsed = 0f;
        EnsurePlayerStatsBound();

        while (elapsed < totalDuration)
        {
            if (playerHealth != null) playerHealth.Heal(Mathf.RoundToInt(hpPerSec));
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }

    private void OnDestroy()
    {
        if (cutsceneTexture != null)
        {
            cutsceneTexture.Release();
            Destroy(cutsceneTexture);
        }
    }
}