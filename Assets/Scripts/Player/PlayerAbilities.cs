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
    [SerializeField, Min(1f)] private float videoPrepareTimeout = 8f;

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
                }
            }

            if (cutsceneRawImage == null && cutscenePanel != null)
                cutsceneRawImage = cutscenePanel.GetComponent<RawImage>();

            // Keep the VideoPlayer on the always-active HUDCanvas. The hidden
            // CutsceneOverlay is only the display surface.
            if (videoPlayer == null ||
                (cutscenePanel != null && videoPlayer.gameObject == cutscenePanel))
            {
                videoPlayer = hudCanvas.GetComponent<VideoPlayer>();

                if (videoPlayer == null)
                    videoPlayer = hudCanvas.AddComponent<VideoPlayer>();
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

        VideoClip clip = characterData.skill2CutsceneVideo;

        // Keep the original 4K clip. The output texture follows the current
        // display resolution so a 1080p phone does not waste GPU memory on a
        // 4K UI texture, while a 4K display can still receive a 4K texture.
        int sourceWidth = clip.width > 0 ? (int)clip.width : 3840;
        int sourceHeight = clip.height > 0 ? (int)clip.height : 2160;
        int longestDisplaySide = Mathf.Max(Screen.width, Screen.height);
        int renderWidth = Mathf.Min(sourceWidth, Mathf.Max(1280, longestDisplaySide));
        int renderHeight = Mathf.Max(
            1,
            Mathf.RoundToInt(renderWidth * (sourceHeight / (float)sourceWidth))
        );

        cutsceneTexture = new RenderTexture(
            renderWidth,
            renderHeight,
            0,
            RenderTextureFormat.ARGB32
        );
        cutsceneTexture.name = "QSkillCutsceneTexture";
        cutsceneTexture.Create();

        videoPlayer.enabled = true;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = cutsceneTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.clip = clip;

        if (cutsceneRawImage != null)
        {
            cutsceneRawImage.texture = cutsceneTexture;
            cutsceneRawImage.color = Color.white;
        }

        // HUDCanvas stays active, so this prepares the 4K decoder before Q.
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
                    float damageReduction =
                        AdminConsole.ResolvePlayerDamageReduction(0.50f);

                    playerHealth.ActivateDamageReduction(
                        characterData.skill1Duration,
                        damageReduction
                    );

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
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (cutscenePanel != null && videoPlayer != null && characterData.skill2CutsceneVideo != null)
        {
            if (videoPlayer.clip != characterData.skill2CutsceneVideo)
            {
                videoPlayer.Stop();
                videoPlayer.clip = characterData.skill2CutsceneVideo;
            }

            if (!videoPlayer.isPrepared)
                videoPlayer.Prepare();

            float prepareElapsed = 0f;
            while (!videoPlayer.isPrepared && prepareElapsed < videoPrepareTimeout)
            {
                prepareElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (videoPlayer.isPrepared)
            {
                videoPlayer.frame = 0;
                cutscenePanel.SetActive(true);
                videoPlayer.Play();

                yield return new WaitForSecondsRealtime(2.5f);

                videoPlayer.Stop();
                cutscenePanel.SetActive(false);

                // Preload again for the next Q activation.
                videoPlayer.Prepare();
            }
            else
            {
                cutscenePanel.SetActive(false);
                Debug.LogWarning(
                    $"[PlayerAbilities] 4K cutscene preparation timed out after {videoPrepareTimeout:0.0}s."
                );
            }
        }

        Time.timeScale = previousTimeScale;

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
            float attackBoost =
                AdminConsole.ResolveAttackPotionMultiplier(0.30f);

            float attackDuration =
                AdminConsole.ResolveAttackPotionDuration(150f);

            AttackPotionCount--;

            playerStats.ApplyAttackPotion(
                attackBoost,
                attackDuration
            );

            Debug.Log(
                $"[PlayerAbilities] Attack Potion applied to " +
                $"{playerStats.gameObject.name}: " +
                $"+{attackBoost * 100f:0}% ATK for " +
                $"{attackDuration:0.#} seconds."
            );
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
            float speedBoost =
                AdminConsole.ResolveSpeedPotionMultiplier(0.50f);

            float speedDuration =
                AdminConsole.ResolveSpeedPotionDuration(150f);

            SpeedPotionCount--;

            playerStats.ApplySpeedPotion(
                speedBoost,
                speedDuration
            );

            Debug.Log(
                $"[PlayerAbilities] Speed Potion applied to " +
                $"{playerStats.gameObject.name}: " +
                $"+{speedBoost * 100f:0}% speed for " +
                $"{speedDuration:0.#} seconds."
            );
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
            if (videoPlayer != null && videoPlayer.targetTexture == cutsceneTexture)
                videoPlayer.targetTexture = null;

            if (cutsceneRawImage != null && cutsceneRawImage.texture == cutsceneTexture)
                cutsceneRawImage.texture = null;

            cutsceneTexture.Release();
            Destroy(cutsceneTexture);
        }
    }
}