using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class StageEnemyEntry
{
    [SerializeField]
    private EnemyHealth enemyPrefab;

    [SerializeField, Min(0f)]
    private float unlockTime;

    [SerializeField, Min(0f)]
    private float spawnWeight = 1f;

    public EnemyHealth EnemyPrefab => enemyPrefab;
    public float UnlockTime => unlockTime;
    public float SpawnWeight => spawnWeight;
}

[CreateAssetMenu(
    fileName = "StageConfiguration",
    menuName = "Wave Survivor/Stage Configuration"
)]
public class StageConfiguration : ScriptableObject
{
    [Header("Identity")]
    [SerializeField, Range(1, 5)]
    private int stageId = 1;

    [SerializeField]
    private string stageName = "BEGINNER ARENA";

    [Header("Run")]
    [SerializeField, Min(1f)]
    private float survivalTime = 300f;

    [Header("Arena")]
    [SerializeField]
    private Vector2 arenaHalfSize = new Vector2(14f, 9f);

    [Header("Arena Floor Visuals")]
    [SerializeField]
    private bool overrideArenaVisuals;

    [SerializeField]
    private TileBase floorBaseTile;

    [SerializeField]
    private TileBase[] floorVariationTiles;

    [SerializeField]
    private TileBase floorBorderTile;

    [SerializeField, Range(0f, 1f)]
    private float floorVariationChance = 0.12f;

    [SerializeField]
    private int floorRandomSeed = 12345;

    [SerializeField]
    private Color floorTint = Color.white;

    [Header("Arena Decorations")]
    [SerializeField]
    private bool overrideArenaDecorations;

    [SerializeField]
    private TileBase[] arenaDecorationTiles;

    [SerializeField, Min(0)]
    private int arenaDecorationCount = 12;

    [SerializeField, Min(0)]
    private int decorationEdgeInset = 1;

    [SerializeField]
    private int decorationRandomSeed = 54321;

    [Header("Enemy Roster")]
    [SerializeField]
    private StageEnemyEntry[] enemyRoster;

    [Header("Enemy Spawn Timing")]
    [SerializeField, Min(0f)]
    private float startDelay = 1f;

    [SerializeField, Min(0.1f)]
    private float initialSpawnInterval = 2f;

    [SerializeField, Min(0.1f)]
    private float minimumSpawnInterval = 0.75f;

    [SerializeField, Min(0f)]
    private float intervalDecreasePerMinute = 0.25f;

    [SerializeField, Min(1)]
    private int maxActiveEnemies = 60;

    [Header("Spider Settings")]
    [SerializeField, Min(0f)]
    private float spiderUnlockTime = 60f;

    [SerializeField, Range(0f, 1f)]
    private float spiderSpawnChance = 0.35f;

    [Header("Boss")]
    [SerializeField]
    private bool bossStage;

    [SerializeField]
    private EnemyHealth bossPrefab;

    [SerializeField, Min(0f)]
    private float bossSpawnTime = 240f;

    public int StageId => stageId;
    public string StageName => stageName;
    public float SurvivalTime => survivalTime;
    public Vector2 ArenaHalfSize => arenaHalfSize;

    public bool OverrideArenaVisuals => overrideArenaVisuals;
    public TileBase FloorBaseTile => floorBaseTile;
    public TileBase[] FloorVariationTiles => floorVariationTiles;
    public TileBase FloorBorderTile => floorBorderTile;
    public float FloorVariationChance => floorVariationChance;
    public int FloorRandomSeed => floorRandomSeed;
    public Color FloorTint => floorTint;

    public bool OverrideArenaDecorations => overrideArenaDecorations;

    public TileBase[] ArenaDecorationTiles => arenaDecorationTiles;

    public int ArenaDecorationCount => arenaDecorationCount;

    public int DecorationEdgeInset => decorationEdgeInset;

    public int DecorationRandomSeed => decorationRandomSeed;

    public float StartDelay => startDelay;
    public float InitialSpawnInterval => initialSpawnInterval;
    public float MinimumSpawnInterval => minimumSpawnInterval;
    public float IntervalDecreasePerMinute => intervalDecreasePerMinute;

    public int MaxActiveEnemies => maxActiveEnemies;
    public float SpiderUnlockTime => spiderUnlockTime;
    public float SpiderSpawnChance => spiderSpawnChance;
    public bool BossStage => bossStage;
    public EnemyHealth BossPrefab => bossPrefab;
    public float BossSpawnTime => bossSpawnTime;
    public StageEnemyEntry[] EnemyRoster => enemyRoster;
}