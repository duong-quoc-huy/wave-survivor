using UnityEngine;
using UnityEngine.Tilemaps;

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
    private Vector2 arenaHalfSize =
        new Vector2(14f, 9f);

    [Header("Arena Visuals - Floor")]
    [SerializeField] private TileBase floorBaseTile;
    [SerializeField] private TileBase floorVariationTile;
    [SerializeField] private TileBase floorBorderTile;

    [SerializeField]
    private Color floorTint = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float floorVariationChance = 0.08f;

    [SerializeField]
    private int visualRandomSeed = 12345;

    [Header("Arena Visuals - Decorations")]
    [SerializeField] private TileBase decorationTileA;
    [SerializeField] private TileBase decorationTileB;
    [SerializeField] private TileBase decorationTileC;

    [SerializeField]
    private Color decorationTint = Color.white;

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

    [SerializeField, Min(0f)]
    private float bossSpawnTime = 240f;

    public int StageId => stageId;
    public string StageName => stageName;
    public float SurvivalTime => survivalTime;
    public Vector2 ArenaHalfSize => arenaHalfSize;

    public TileBase FloorBaseTile => floorBaseTile;
    public TileBase FloorVariationTile =>
        floorVariationTile;
    public TileBase FloorBorderTile => floorBorderTile;
    public Color FloorTint => floorTint;
    public float FloorVariationChance =>
        floorVariationChance;
    public int VisualRandomSeed => visualRandomSeed;

    public TileBase DecorationTileA => decorationTileA;
    public TileBase DecorationTileB => decorationTileB;
    public TileBase DecorationTileC => decorationTileC;
    public Color DecorationTint => decorationTint;

    public float StartDelay => startDelay;
    public float InitialSpawnInterval =>
        initialSpawnInterval;
    public float MinimumSpawnInterval =>
        minimumSpawnInterval;
    public float IntervalDecreasePerMinute =>
        intervalDecreasePerMinute;
    public int MaxActiveEnemies => maxActiveEnemies;
    public float SpiderUnlockTime => spiderUnlockTime;
    public float SpiderSpawnChance => spiderSpawnChance;
    public bool BossStage => bossStage;
    public float BossSpawnTime => bossSpawnTime;
}