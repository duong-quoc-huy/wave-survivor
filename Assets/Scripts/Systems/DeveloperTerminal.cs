using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Developer-only, in-game cheat terminal for Wave Survivor.
/// Attach this component to a DeveloperTools GameObject in MainMenuScene.
/// It persists across scenes and does not require AdminConsole cheats to be enabled.
///
/// Desktop: press F1 or Backquote (`) to open/close.
/// Mobile: tap the DEV button (Development Build only by default).
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-9000)]
public sealed class DeveloperTerminal : MonoBehaviour
{
    [Header("Availability")]
    [SerializeField] private bool terminalEnabled = true;
    [SerializeField] private bool enableInEditor = true;
    [SerializeField] private bool enableInDevelopmentBuild = true;
    [Tooltip("Keep this disabled for public builds.")]
    [SerializeField] private bool enableInReleaseBuild;

    [Header("Opening the terminal")]
    [SerializeField] private bool showDevButton = true;
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private Key toggleKey = Key.F1;
    [SerializeField] private Key alternateToggleKey = Key.Backquote;

    [Header("Terminal appearance")]
    [SerializeField, Range(0.5f, 1f)] private float windowWidthPercent = 0.88f;
    [SerializeField, Range(0.5f, 1f)] private float windowHeightPercent = 0.82f;
    [SerializeField, Min(10)] private int maximumLogLines = 80;

    private readonly List<string> logLines = new List<string>();
    private readonly Dictionary<string, Func<string[], string>> commands =
        new Dictionary<string, Func<string[], string>>(StringComparer.OrdinalIgnoreCase);

    private Rect windowRect;
    private Vector2 logScrollPosition;
    private string commandInput = string.Empty;
    private bool terminalOpen;
    private bool focusInputNextFrame;
    private bool runtimeGodMode;
    private bool enemiesFrozen;
    private float timeScaleBeforeOpen = 1f;
    private float enemySpeedBeforeFreeze = 0.5f;

    // Runtime overrides are intentionally independent from AdminConsole.
    // Each override is also applied once to newly spawned runtime objects.
    private bool playerHpOverrideActive;
    private bool playerAttackOverrideActive;
    private bool playerSpeedOverrideActive;
    private bool enemyHpOverrideActive;
    private bool enemySpeedOverrideActive;
    private bool enemyDamageOverrideActive;
    private bool xpRequirementOverrideActive;
    private bool xpMultiplierOverrideActive;
    private bool xpDropOverrideActive;
    private bool goldDropOverrideActive;
    private bool goldValueOverrideActive;

    private int runtimePlayerHp;
    private float runtimePlayerAttack;
    private float runtimePlayerSpeed;
    private int runtimeEnemyHp;
    private float runtimeEnemySpeed;
    private int runtimeEnemyDamage;
    private int runtimeXpRequirement;
    private float runtimeXpMultiplier;
    private int runtimeXpDrop;
    private float runtimeGoldDropChance;
    private int runtimeGoldValue;

    private int trackedPlayerHealthId = int.MinValue;
    private int trackedPlayerStatsId = int.MinValue;
    private readonly HashSet<int> trackedEnemyControllerIds = new HashSet<int>();
    private readonly HashSet<int> trackedEnemyHealthIds = new HashSet<int>();
    private readonly HashSet<int> trackedXpOrbIds = new HashSet<int>();
    private float nextRuntimeRefreshTime;

    private static DeveloperTerminal instance;

    private const BindingFlags InstanceMembers =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private bool IsAvailable
    {
        get
        {
            if (!terminalEnabled)
                return false;

            if (Application.isEditor)
                return enableInEditor;

            if (Debug.isDebugBuild)
                return enableInDevelopmentBuild;

            return enableInReleaseBuild;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterCommands();
        AppendLog("Wave Survivor Developer Terminal ready.");
        AppendLog("Enter 'help' to list commands.");
    }

    private void Update()
    {
        if (!IsAvailable)
        {
            if (runtimeGodMode)
            {
                runtimeGodMode = false;
                ApplyGodModeToCurrentPlayer(false);
            }

            if (terminalOpen)
                CloseTerminal(true);

            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard[toggleKey].wasPressedThisFrame ||
             keyboard[alternateToggleKey].wasPressedThisFrame))
        {
            ToggleTerminal();
        }
    }

    private void LateUpdate()
    {
        if (!IsAvailable)
            return;

        // Reapply the shield flags because a normal skill coroutine may expire
        // while runtime God Mode is enabled.
        if (runtimeGodMode)
            ApplyGodModeToCurrentPlayer(true);

        if (Time.unscaledTime >= nextRuntimeRefreshTime)
        {
            nextRuntimeRefreshTime = Time.unscaledTime + 0.2f;
            ApplyRuntimeOverridesToNewObjects();
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (runtimeGodMode)
        {
            runtimeGodMode = false;
            ApplyGodModeToCurrentPlayer(false);
        }

        if (terminalOpen)
            CloseTerminal(true);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void HandleSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode
    )
    {
        trackedPlayerHealthId = int.MinValue;
        trackedPlayerStatsId = int.MinValue;
        trackedEnemyControllerIds.Clear();
        trackedEnemyHealthIds.Clear();
        trackedXpOrbIds.Clear();
        nextRuntimeRefreshTime = 0f;
    }

    private void OnGUI()
    {
        if (!IsAvailable)
            return;

        int buttonFontSize = Mathf.Clamp(Screen.height / 36, 15, 26);

        if (showDevButton && !terminalOpen)
        {
            GUIStyle devStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = buttonFontSize,
                fontStyle = FontStyle.Bold
            };

            float buttonWidth = Mathf.Clamp(Screen.width * 0.09f, 88f, 150f);
            float buttonHeight = Mathf.Clamp(Screen.height * 0.075f, 46f, 72f);

            if (GUI.Button(
                    new Rect(12f, 12f, buttonWidth, buttonHeight),
                    "DEV",
                    devStyle))
            {
                OpenTerminal();
            }
        }

        if (!terminalOpen)
            return;

        float width = Mathf.Min(
            Screen.width - 20f,
            Mathf.Max(560f, Screen.width * windowWidthPercent)
        );

        float height = Mathf.Min(
            Screen.height - 20f,
            Mathf.Max(420f, Screen.height * windowHeightPercent)
        );

        windowRect = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        windowRect = GUI.Window(
            GetInstanceID(),
            windowRect,
            DrawTerminalWindow,
            "WAVE SURVIVOR - DEVELOPER TERMINAL"
        );
    }

    private void DrawTerminalWindow(int windowId)
    {
        int fontSize = Mathf.Clamp(Screen.height / 44, 14, 23);
        GUIStyle logStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = fontSize,
            wordWrap = true,
            richText = false
        };

        GUIStyle inputStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = fontSize
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold
        };

        GUILayout.Space(6f);

        logScrollPosition = GUILayout.BeginScrollView(
            logScrollPosition,
            GUILayout.ExpandHeight(true)
        );

        GUILayout.TextArea(
            string.Join("\n", logLines),
            logStyle,
            GUILayout.ExpandHeight(true)
        );

        GUILayout.EndScrollView();

        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();

        GUILayout.Label(">", GUILayout.Width(20f));
        GUI.SetNextControlName("DeveloperTerminalInput");
        commandInput = GUILayout.TextField(
            commandInput,
            inputStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(Mathf.Max(32f, fontSize * 1.8f))
        );

        if (GUILayout.Button("EXECUTE", buttonStyle, GUILayout.Width(130f)))
            ExecuteCurrentInput();

        GUILayout.EndHorizontal();

        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("WIN STAGE", buttonStyle))
            ExecuteCommandLine("win");

        if (GUILayout.Button(runtimeGodMode ? "GOD: ON" : "GOD: OFF", buttonStyle))
            ExecuteCommandLine(runtimeGodMode ? "god off" : "god on");

        if (GUILayout.Button("HEAL", buttonStyle))
            ExecuteCommandLine("heal");

        if (GUILayout.Button("KILL ALL", buttonStyle))
            ExecuteCommandLine("killall");

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("+100 XP", buttonStyle))
            ExecuteCommandLine("xp 100");

        if (GUILayout.Button("+1000 GOLD", buttonStyle))
            ExecuteCommandLine("gold 1000");

        if (GUILayout.Button("RESET COOLDOWNS", buttonStyle))
            ExecuteCommandLine("cooldown reset");

        if (GUILayout.Button("CLOSE", buttonStyle))
            CloseTerminal(true);

        GUILayout.EndHorizontal();

        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.KeyDown &&
            (currentEvent.keyCode == KeyCode.Return ||
             currentEvent.keyCode == KeyCode.KeypadEnter) &&
            GUI.GetNameOfFocusedControl() == "DeveloperTerminalInput")
        {
            ExecuteCurrentInput();
            currentEvent.Use();
        }

        if (focusInputNextFrame)
        {
            GUI.FocusControl("DeveloperTerminalInput");
            focusInputNextFrame = false;
        }

        GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 32f));
    }

    private void RegisterCommands()
    {
        commands["help"] = HelpCommand;
        commands["win"] = WinCommand;
        commands["god"] = GodCommand;
        commands["heal"] = HealCommand;
        commands["damage"] = DamageCommand;
        commands["xp"] = XpCommand;
        commands["gold"] = GoldCommand;
        commands["killall"] = KillAllCommand;
        commands["freeze"] = FreezeCommand;
        commands["cooldown"] = CooldownCommand;
        commands["set"] = SetCommand;
        commands["apply"] = ApplyCommand;
        commands["timescale"] = TimeScaleCommand;
        commands["status"] = StatusCommand;
        commands["clear"] = ClearCommand;
        commands["close"] = CloseCommand;
    }

    private void ToggleTerminal()
    {
        if (terminalOpen)
            CloseTerminal(true);
        else
            OpenTerminal();
    }

    private void OpenTerminal()
    {
        if (terminalOpen || !IsAvailable)
            return;

        terminalOpen = true;
        focusInputNextFrame = true;

        if (pauseGameWhileOpen)
        {
            timeScaleBeforeOpen = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void CloseTerminal(bool restoreTimeScale)
    {
        if (!terminalOpen)
            return;

        terminalOpen = false;

        if (pauseGameWhileOpen && restoreTimeScale)
            Time.timeScale = timeScaleBeforeOpen;
    }

    private void ExecuteCurrentInput()
    {
        string line = commandInput;
        commandInput = string.Empty;
        ExecuteCommandLine(line);
        focusInputNextFrame = true;
    }

    private void ExecuteCommandLine(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return;

        string[] parts = commandLine.Trim().Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries
        );

        if (parts.Length == 0)
            return;

        AppendLog($"> {commandLine.Trim()}");

        if (!commands.TryGetValue(parts[0], out Func<string[], string> command))
        {
            AppendLog($"Unknown command: {parts[0]}. Enter 'help'.");
            return;
        }

        try
        {
            string result = command(parts);
            if (!string.IsNullOrEmpty(result))
                AppendLog(result);
        }
        catch (Exception exception)
        {
            AppendLog($"ERROR: {exception.GetBaseException().Message}");
            Debug.LogException(exception, this);
        }
    }

    private string HelpCommand(string[] args)
    {
        return
            "COMMANDS:\n" +
            "win - complete and record the current stage\n" +
            "god on|off|toggle - toggle runtime invulnerability\n" +
            "heal [amount] - heal the current player\n" +
            "damage <amount> - damage the current player\n" +
            "xp <amount> - add experience\n" +
            "gold <amount> - add saved gold\n" +
            "killall - eliminate all active enemies\n" +
            "freeze on|off - freeze or restore enemy movement\n" +
            "cooldown reset - reset E and Q cooldowns\n" +
            "set hp|atk|speed <value> - change current and future player stats\n" +
            "set enemyhp|enemyspeed|enemydamage <value>\n" +
            "set xpreq|xpmult|xpdrop <value>\n" +
            "set golddrop|goldvalue <value>\n" +
            "apply - apply Inspector cheat values to active objects\n" +
            "timescale <value> - change gameplay time scale\n" +
            "status - display current runtime values\n" +
            "clear - clear terminal output\n" +
            "close - close the terminal";
    }

    private string WinCommand(string[] args)
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            return "GameManager was not found. Open a gameplay stage first.";

        MethodInfo forceVictory = gameManager.GetType().GetMethod(
            "ForceVictory",
            InstanceMembers,
            null,
            Type.EmptyTypes,
            null
        );

        MethodInfo endRun = gameManager.GetType().GetMethod(
            "EndRun",
            InstanceMembers,
            null,
            new[] { typeof(bool) },
            null
        );

        // Restore gameplay first; the official Victory flow will pause it again.
        CloseTerminal(true);

        if (forceVictory != null)
        {
            forceVictory.Invoke(gameManager, null);
            return "Victory triggered through GameManager.ForceVictory().";
        }

        if (endRun != null)
        {
            endRun.Invoke(gameManager, new object[] { true });
            return "Victory triggered through the official EndRun(true) flow.";
        }

        terminalOpen = true;
        if (pauseGameWhileOpen)
            Time.timeScale = 0f;

        return "Victory method was not found. Expected ForceVictory() or EndRun(bool).";
    }

    private string GodCommand(string[] args)
    {
        if (args.Length < 2 || args[1].Equals("toggle", StringComparison.OrdinalIgnoreCase))
            runtimeGodMode = !runtimeGodMode;
        else if (args[1].Equals("on", StringComparison.OrdinalIgnoreCase))
            runtimeGodMode = true;
        else if (args[1].Equals("off", StringComparison.OrdinalIgnoreCase))
            runtimeGodMode = false;
        else
            return "Usage: god on|off|toggle";

        ApplyGodModeToCurrentPlayer(runtimeGodMode);
        return runtimeGodMode ? "Runtime God Mode enabled." : "Runtime God Mode disabled.";
    }

    private string HealCommand(string[] args)
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
            return "PlayerHealth was not found.";

        int amount = health.MaxHealth;
        if (args.Length >= 2 && !TryParseInt(args[1], out amount))
            return "Usage: heal [positive amount]";

        if (amount <= 0)
            return "Heal amount must be greater than zero.";

        health.Heal(amount);
        return $"Player healed. HP: {health.CurrentHealth}/{health.MaxHealth}.";
    }

    private string DamageCommand(string[] args)
    {
        if (args.Length < 2 || !TryParseInt(args[1], out int amount) || amount <= 0)
            return "Usage: damage <positive amount>";

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
            return "PlayerHealth was not found.";

        if (runtimeGodMode)
            return "Damage ignored because Runtime God Mode is enabled.";

        health.TakeDamage(amount);
        return $"Applied {amount} damage. HP: {health.CurrentHealth}/{health.MaxHealth}.";
    }

    private string XpCommand(string[] args)
    {
        if (args.Length < 2 || !TryParseInt(args[1], out int amount) || amount <= 0)
            return "Usage: xp <positive amount>";

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null)
            return "PlayerStats was not found.";

        stats.AddExperience(amount);
        return $"Added {amount} XP. Level: {stats.Level}, XP: " +
               $"{stats.CurrentExperience}/{stats.ExperienceToNextLevel}.";
    }

    private string GoldCommand(string[] args)
    {
        if (args.Length < 2 || !TryParseInt(args[1], out int amount) || amount <= 0)
            return "Usage: gold <positive amount>";

        LocalSaveSystem.AddGold(amount);
        return $"Added {amount} gold. Total: {LocalSaveSystem.GetGold()}.";
    }

    private string KillAllCommand(string[] args)
    {
        GameObject[] enemies;

        try
        {
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
        }
        catch (UnityException)
        {
            return "The Enemy tag does not exist in this project.";
        }

        int eliminated = 0;

        foreach (GameObject enemyObject in enemies)
        {
            if (enemyObject == null)
                continue;

            // Use exactly one health implementation per enemy to avoid duplicate drops.
            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(int.MaxValue);
                eliminated++;
                continue;
            }

            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(int.MaxValue);
                eliminated++;
                continue;
            }

            Destroy(enemyObject);
            eliminated++;
        }

        return $"Eliminated {eliminated} active enemies.";
    }

    private string FreezeCommand(string[] args)
    {
        bool shouldFreeze;

        if (args.Length < 2)
            shouldFreeze = !enemiesFrozen;
        else if (args[1].Equals("on", StringComparison.OrdinalIgnoreCase))
            shouldFreeze = true;
        else if (args[1].Equals("off", StringComparison.OrdinalIgnoreCase))
            shouldFreeze = false;
        else
            return "Usage: freeze on|off";

        AdminConsole admin = AdminConsole.Instance;

        if (shouldFreeze && !enemiesFrozen)
        {
            enemySpeedBeforeFreeze = admin != null
                ? admin.enemyMoveSpeed
                : enemySpeedBeforeFreeze;
        }

        enemiesFrozen = shouldFreeze;

        if (admin != null)
        {
            admin.enemyMoveSpeed = shouldFreeze
                ? 0f
                : Mathf.Max(0f, enemySpeedBeforeFreeze);
        }

        EnemyController[] enemies = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in enemies)
        {
            if (shouldFreeze)
                enemy.SetSpeedMultiplier(0f);
            else
            {
                if (enemySpeedOverrideActive)
                    SetFieldValue(enemy, "moveSpeed", runtimeEnemySpeed);

                enemy.ResetSpeedMultiplier();
            }
        }

        return shouldFreeze
            ? $"Enemy movement frozen ({enemies.Length} active)."
            : $"Enemy movement restored ({enemies.Length} active).";
    }

    private string CooldownCommand(string[] args)
    {
        if (args.Length < 2 || !args[1].Equals("reset", StringComparison.OrdinalIgnoreCase))
            return "Usage: cooldown reset";

        PlayerAbilities abilities = FindFirstObjectByType<PlayerAbilities>();
        if (abilities == null)
            return "PlayerAbilities was not found.";

        SetFieldValue(abilities, "skill1CooldownTimer", 0f);
        SetFieldValue(abilities, "skill2CooldownTimer", 0f);
        return "E and Q cooldown timers reset.";
    }

    private string SetCommand(string[] args)
    {
        if (args.Length < 3 || !TryParseFloat(args[2], out float value))
            return "Usage: set <property> <numeric value>. Enter 'help' for properties.";

        string property = args[1].ToLowerInvariant();

        switch (property)
        {
            case "hp":
                return SetPlayerHp(value);
            case "atk":
                return SetPlayerAttack(value);
            case "speed":
                return SetPlayerSpeed(value);
            case "enemyhp":
                return SetEnemyHp(value);
            case "enemyspeed":
                return SetEnemySpeed(value);
            case "enemydamage":
                return SetEnemyDamage(value);
            case "xpreq":
                return SetXpRequirement(value);
            case "xpmult":
                return SetXpMultiplier(value);
            case "xpdrop":
                return SetXpDrop(value);
            case "golddrop":
                return SetGoldDropChance(value);
            case "goldvalue":
                return SetGoldValue(value);
            default:
                return $"Unknown set property: {property}. Enter 'help'.";
        }
    }

    private string ApplyCommand(string[] args)
    {
        AdminConsole admin = AdminConsole.Instance;
        if (admin == null)
            return "AdminConsole was not found. Use individual 'set' commands instead.";

        SetPlayerHp(admin.playerBaseHp);
        SetPlayerAttack(admin.playerBaseAtk);
        SetPlayerSpeed(admin.playerBaseSpeed);
        SetEnemyHp(admin.enemyMaxHp);
        SetEnemySpeed(admin.enemyMoveSpeed);
        SetEnemyDamage(admin.enemyContactDamage);
        SetXpRequirement(admin.startingXpRequirement);
        SetXpMultiplier(admin.xpRequirementMultiplier);
        SetXpDrop(admin.enemyXpDropValue);
        SetGoldDropChance(admin.goldDropChance);
        SetGoldValue(admin.baseGoldValue);

        if (runtimeGodMode)
            ApplyGodModeToCurrentPlayer(true);

        return "Inspector cheat values applied to active runtime objects.";
    }

    private string TimeScaleCommand(string[] args)
    {
        if (args.Length < 2 || !TryParseFloat(args[1], out float value))
            return "Usage: timescale <value from 0 to 20>";

        value = Mathf.Clamp(value, 0f, 20f);

        if (terminalOpen && pauseGameWhileOpen)
            timeScaleBeforeOpen = value;
        else
            Time.timeScale = value;

        return terminalOpen && pauseGameWhileOpen
            ? $"Gameplay time scale will become {value:0.###} when the terminal closes."
            : $"Time scale set to {value:0.###}.";
    }

    private string StatusCommand(string[] args)
    {
        StringBuilder builder = new StringBuilder();
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();

        builder.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        builder.AppendLine($"Time scale: {Time.timeScale:0.###}");
        builder.AppendLine($"God Mode: {(runtimeGodMode ? "ON" : "OFF")}");

        if (health != null)
            builder.AppendLine($"HP: {health.CurrentHealth}/{health.MaxHealth}");
        else
            builder.AppendLine("HP: no active player");

        if (stats != null)
        {
            builder.AppendLine($"Level: {stats.Level}");
            builder.AppendLine($"XP: {stats.CurrentExperience}/{stats.ExperienceToNextLevel}");
            builder.AppendLine($"ATK: {stats.CurrentAtk:0.##}");
            builder.AppendLine($"Speed: {stats.CurrentSpeed:0.##}");
        }

        builder.Append($"Saved gold: {LocalSaveSystem.GetGold()}");
        return builder.ToString();
    }

    private string ClearCommand(string[] args)
    {
        logLines.Clear();
        return "Terminal output cleared.";
    }

    private string CloseCommand(string[] args)
    {
        CloseTerminal(true);
        return string.Empty;
    }

    private string SetPlayerHp(float rawValue)
    {
        int value = Mathf.Max(1, Mathf.RoundToInt(rawValue));
        playerHpOverrideActive = true;
        runtimePlayerHp = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.playerBaseHp = value;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            SetFieldValue(stats, "baseHp", (float)value);
            RaiseFieldEvent(stats, "OnStatsChanged");
        }

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            SetFieldValue(health, "maxHealth", value);
            SetFieldValue(health, "currentHealth", value);
            RaiseFieldEvent(health, "HealthChanged", value, value);
            trackedPlayerHealthId = health.GetInstanceID();
        }

        return $"Player HP set to {value} for the current and future player.";
    }

    private string SetPlayerAttack(float rawValue)
    {
        float value = Mathf.Max(0f, rawValue);
        playerAttackOverrideActive = true;
        runtimePlayerAttack = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.playerBaseAtk = value;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            SetFieldValue(stats, "baseAtk", value);
            RaiseFieldEvent(stats, "OnStatsChanged");
            trackedPlayerStatsId = stats.GetInstanceID();
        }

        return $"Player base ATK set to {value:0.##}.";
    }

    private string SetPlayerSpeed(float rawValue)
    {
        float value = Mathf.Max(0f, rawValue);
        playerSpeedOverrideActive = true;
        runtimePlayerSpeed = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.playerBaseSpeed = value;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            SetFieldValue(stats, "baseSpeed", value);
            RaiseFieldEvent(stats, "OnStatsChanged");
            trackedPlayerStatsId = stats.GetInstanceID();
        }

        return $"Player base speed set to {value:0.##}.";
    }

    private string SetEnemyHp(float rawValue)
    {
        int value = Mathf.Max(1, Mathf.RoundToInt(rawValue));
        enemyHpOverrideActive = true;
        runtimeEnemyHp = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.enemyMaxHp = value;

        HashSet<GameObject> updated = new HashSet<GameObject>();

        EnemyController[] controllers = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in controllers)
        {
            SetFieldValue(enemy, "maxHealth", value);
            SetFieldValue(enemy, "currentHealth", value);
            updated.Add(enemy.gameObject);
            trackedEnemyControllerIds.Add(enemy.GetInstanceID());
        }

        EnemyHealth[] healthComponents = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in healthComponents)
        {
            SetFieldValue(enemy, "maxHealth", value);
            SetFieldValue(enemy, "currentHealth", value);
            updated.Add(enemy.gameObject);
            trackedEnemyHealthIds.Add(enemy.GetInstanceID());
        }

        return $"Enemy HP set to {value} ({updated.Count} active objects updated).";
    }

    private string SetEnemySpeed(float rawValue)
    {
        float value = Mathf.Max(0f, rawValue);
        enemySpeedOverrideActive = true;
        runtimeEnemySpeed = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.enemyMoveSpeed = value;

        if (enemiesFrozen)
            enemySpeedBeforeFreeze = value;

        EnemyController[] enemies = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in enemies)
        {
            SetFieldValue(enemy, "moveSpeed", value);
            if (!enemiesFrozen)
                enemy.ResetSpeedMultiplier();

            trackedEnemyControllerIds.Add(enemy.GetInstanceID());
        }

        return $"Enemy speed set to {value:0.##} ({enemies.Length} active).";
    }

    private string SetEnemyDamage(float rawValue)
    {
        int value = Mathf.Max(0, Mathf.RoundToInt(rawValue));
        enemyDamageOverrideActive = true;
        runtimeEnemyDamage = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.enemyContactDamage = value;

        EnemyController[] enemies = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in enemies)
        {
            SetFieldValue(enemy, "contactDamage", value);
            trackedEnemyControllerIds.Add(enemy.GetInstanceID());
        }

        return $"Enemy contact damage set to {value} ({enemies.Length} active).";
    }

    private string SetXpRequirement(float rawValue)
    {
        int value = Mathf.Max(1, Mathf.RoundToInt(rawValue));
        xpRequirementOverrideActive = true;
        runtimeXpRequirement = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.startingXpRequirement = value;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            SetFieldValue(stats, "startingExperienceRequirement", value);
            SetFieldValue(stats, "<ExperienceToNextLevel>k__BackingField", value);
            RaiseFieldEvent(stats, "ExperienceChanged", stats.CurrentExperience, value);
            trackedPlayerStatsId = stats.GetInstanceID();
        }

        return $"Current XP requirement set to {value}.";
    }

    private string SetXpMultiplier(float rawValue)
    {
        float value = Mathf.Max(1f, rawValue);
        xpMultiplierOverrideActive = true;
        runtimeXpMultiplier = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.xpRequirementMultiplier = value;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            SetFieldValue(stats, "requirementMultiplier", value);
            trackedPlayerStatsId = stats.GetInstanceID();
        }

        return $"XP requirement multiplier set to {value:0.###}.";
    }

    private string SetXpDrop(float rawValue)
    {
        int value = Mathf.Max(0, Mathf.RoundToInt(rawValue));
        xpDropOverrideActive = true;
        runtimeXpDrop = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.enemyXpDropValue = value;

        EnemyController[] enemies = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in enemies)
        {
            SetFieldValue(enemy, "experienceValue", value);
            trackedEnemyControllerIds.Add(enemy.GetInstanceID());
        }

        XPOrb[] orbs = FindObjectsByType<XPOrb>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (XPOrb orb in orbs)
        {
            SetFieldValue(orb, "experienceValue", value);
            trackedXpOrbIds.Add(orb.GetInstanceID());
        }

        return $"Enemy/XP-orb reward set to {value}.";
    }

    private string SetGoldDropChance(float rawValue)
    {
        float value = Mathf.Clamp01(rawValue);
        goldDropOverrideActive = true;
        runtimeGoldDropChance = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.goldDropChance = value;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            SetFieldValue(enemy, "goldDropChance", value);
            trackedEnemyHealthIds.Add(enemy.GetInstanceID());
        }

        return $"Gold drop chance set to {value * 100f:0.#}%.";
    }

    private string SetGoldValue(float rawValue)
    {
        int value = Mathf.Max(1, Mathf.RoundToInt(rawValue));
        goldValueOverrideActive = true;
        runtimeGoldValue = value;

        AdminConsole admin = AdminConsole.Instance;
        if (admin != null)
            admin.baseGoldValue = value;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            SetFieldValue(enemy, "baseGoldValue", value);
            trackedEnemyHealthIds.Add(enemy.GetInstanceID());
        }

        return $"Base gold value set to {value}.";
    }

    private void ApplyGodModeToCurrentPlayer(bool enabled)
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
            return;

        SetFieldValue(health, "isSkill1Active", enabled);
        SetFieldValue(health, "currentDamageReductionPercent", enabled ? 1f : 0f);

        if (enabled)
            health.Heal(health.MaxHealth);
    }

    private void ApplyRuntimeOverridesToNewObjects()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health == null)
        {
            trackedPlayerHealthId = int.MinValue;
        }
        else if (health.GetInstanceID() != trackedPlayerHealthId)
        {
            trackedPlayerHealthId = health.GetInstanceID();

            if (playerHpOverrideActive)
            {
                SetFieldValue(health, "maxHealth", runtimePlayerHp);
                SetFieldValue(health, "currentHealth", runtimePlayerHp);
                RaiseFieldEvent(
                    health,
                    "HealthChanged",
                    runtimePlayerHp,
                    runtimePlayerHp
                );
            }

            if (runtimeGodMode)
                ApplyGodModeToCurrentPlayer(true);
        }

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null)
        {
            trackedPlayerStatsId = int.MinValue;
        }
        else if (stats.GetInstanceID() != trackedPlayerStatsId)
        {
            trackedPlayerStatsId = stats.GetInstanceID();

            if (playerHpOverrideActive)
                SetFieldValue(stats, "baseHp", (float)runtimePlayerHp);

            if (playerAttackOverrideActive)
                SetFieldValue(stats, "baseAtk", runtimePlayerAttack);

            if (playerSpeedOverrideActive)
                SetFieldValue(stats, "baseSpeed", runtimePlayerSpeed);

            if (xpRequirementOverrideActive)
            {
                SetFieldValue(stats, "startingExperienceRequirement", runtimeXpRequirement);
                SetFieldValue(
                    stats,
                    "<ExperienceToNextLevel>k__BackingField",
                    runtimeXpRequirement
                );
                RaiseFieldEvent(
                    stats,
                    "ExperienceChanged",
                    stats.CurrentExperience,
                    runtimeXpRequirement
                );
            }

            if (xpMultiplierOverrideActive)
                SetFieldValue(stats, "requirementMultiplier", runtimeXpMultiplier);

            RaiseFieldEvent(stats, "OnStatsChanged");
        }

        EnemyController[] controllers = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyController enemy in controllers)
        {
            int id = enemy.GetInstanceID();
            if (!trackedEnemyControllerIds.Add(id))
                continue;

            if (enemyHpOverrideActive)
            {
                SetFieldValue(enemy, "maxHealth", runtimeEnemyHp);
                SetFieldValue(enemy, "currentHealth", runtimeEnemyHp);
            }

            if (enemySpeedOverrideActive)
                SetFieldValue(enemy, "moveSpeed", runtimeEnemySpeed);

            if (enemyDamageOverrideActive)
                SetFieldValue(enemy, "contactDamage", runtimeEnemyDamage);

            if (xpDropOverrideActive)
                SetFieldValue(enemy, "experienceValue", runtimeXpDrop);

            if (enemiesFrozen)
                enemy.SetSpeedMultiplier(0f);
        }

        EnemyHealth[] enemyHealthComponents = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemyHealthComponents)
        {
            int id = enemy.GetInstanceID();
            if (!trackedEnemyHealthIds.Add(id))
                continue;

            if (enemyHpOverrideActive)
            {
                SetFieldValue(enemy, "maxHealth", runtimeEnemyHp);
                SetFieldValue(enemy, "currentHealth", runtimeEnemyHp);
            }

            if (goldDropOverrideActive)
                SetFieldValue(enemy, "goldDropChance", runtimeGoldDropChance);

            if (goldValueOverrideActive)
                SetFieldValue(enemy, "baseGoldValue", runtimeGoldValue);
        }

        XPOrb[] orbs = FindObjectsByType<XPOrb>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (XPOrb orb in orbs)
        {
            int id = orb.GetInstanceID();
            if (!trackedXpOrbIds.Add(id))
                continue;

            if (xpDropOverrideActive)
                SetFieldValue(orb, "experienceValue", runtimeXpDrop);
        }

        PruneDestroyedRuntimeIds();
    }

    private void PruneDestroyedRuntimeIds()
    {
        // Instance IDs can eventually be reused. Clear the small caches whenever
        // no object of that component type remains in the active scene.
        if (FindFirstObjectByType<EnemyController>() == null)
            trackedEnemyControllerIds.Clear();

        if (FindFirstObjectByType<EnemyHealth>() == null)
            trackedEnemyHealthIds.Clear();

        if (FindFirstObjectByType<XPOrb>() == null)
            trackedXpOrbIds.Clear();
    }

    private static bool SetFieldValue(object target, string fieldName, object value)
    {
        if (target == null)
            return false;

        FieldInfo field = target.GetType().GetField(fieldName, InstanceMembers);
        if (field == null)
            return false;

        field.SetValue(target, value);
        return true;
    }

    private static void RaiseFieldEvent(object target, string eventFieldName, params object[] args)
    {
        if (target == null)
            return;

        FieldInfo eventField = target.GetType().GetField(eventFieldName, InstanceMembers);
        if (eventField == null)
            return;

        if (eventField.GetValue(target) is Delegate eventDelegate)
            eventDelegate.DynamicInvoke(args);
    }

    private void AppendLog(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        string[] splitLines = message.Replace("\r", string.Empty).Split('\n');
        foreach (string line in splitLines)
            logLines.Add(line);

        while (logLines.Count > Mathf.Max(10, maximumLogLines))
            logLines.RemoveAt(0);

        logScrollPosition.y = float.MaxValue;
    }

    private static bool TryParseInt(string text, out int value)
    {
        return int.TryParse(
            text,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private static bool TryParseFloat(string text, out float value)
    {
        return float.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );
    }
}
