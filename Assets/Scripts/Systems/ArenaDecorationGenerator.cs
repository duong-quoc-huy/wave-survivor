using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ArenaDecorationGenerator : MonoBehaviour
{
    [Header("Stage 1 Default Decorations")]
    [SerializeField] private TileBase crateTile;
    [SerializeField] private TileBase tombstoneTile;
    [SerializeField] private TileBase barrelTile;

    private TileBase[] runtimeDecorationTiles;
    private Vector2 runtimeArenaHalfSize;
    private int runtimeDecorationCount;
    private int runtimeEdgeInset;
    private int runtimeRandomSeed;

    public void ConfigureStage(StageConfiguration configuration)
    {
        if (configuration == null ||
            !configuration.OverrideArenaDecorations)
        {
            // Stage 1 keeps the decorations already stored in the scene.
            return;
        }

        runtimeDecorationTiles =
            configuration.ArenaDecorationTiles;

        runtimeArenaHalfSize =
            configuration.ArenaHalfSize;

        runtimeDecorationCount =
            configuration.ArenaDecorationCount;

        runtimeEdgeInset =
            configuration.DecorationEdgeInset;

        runtimeRandomSeed =
            configuration.DecorationRandomSeed;

        GenerateStageDecorations();
    }

    private void GenerateStageDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();

        List<TileBase> usableTiles = GetUsableRuntimeTiles();

        if (usableTiles.Count == 0 ||
            runtimeDecorationCount <= 0)
        {
            tilemap.CompressBounds();

            Debug.LogWarning(
                "This stage overrides arena decorations, " +
                "but it has no usable decoration tiles.",
                this
            );

            return;
        }

        int halfWidth = Mathf.Max(
            1,
            Mathf.FloorToInt(runtimeArenaHalfSize.x)
        );

        int halfHeight = Mathf.Max(
            1,
            Mathf.FloorToInt(runtimeArenaHalfSize.y)
        );

        int safeInset = Mathf.Clamp(
            runtimeEdgeInset,
            0,
            Mathf.Min(halfWidth - 1, halfHeight - 1)
        );

        int horizontalExtent = Mathf.Max(
            1,
            halfWidth - safeInset
        );

        int verticalExtent = Mathf.Max(
            1,
            halfHeight - safeInset
        );

        System.Random random =
            new System.Random(runtimeRandomSeed);

        int tileOffset = random.Next(usableTiles.Count);

        HashSet<Vector3Int> occupiedCells =
            new HashSet<Vector3Int>();

        int placedCount = 0;
        int attempts = 0;
        int maximumAttempts =
            Mathf.Max(40, runtimeDecorationCount * 12);

        while (placedCount < runtimeDecorationCount &&
               attempts < maximumAttempts)
        {
            attempts++;

            Vector3Int position = GetRandomEdgePosition(
                random,
                horizontalExtent,
                verticalExtent
            );

            if (!occupiedCells.Add(position))
                continue;

            // Cycle through the available tiles so every configured
            // decoration type is represented. The seed still changes
            // the starting tile and all placement positions.
            int tileIndex =
                (tileOffset + placedCount) % usableTiles.Count;

            TileBase selectedTile = usableTiles[tileIndex];

            tilemap.SetTile(position, selectedTile);
            placedCount++;
        }

        tilemap.CompressBounds();

        Debug.Log(
            $"Generated {placedCount} stage decorations.",
            this
        );
    }

    private Vector3Int GetRandomEdgePosition(
        System.Random random,
        int horizontalExtent,
        int verticalExtent
    )
    {
        int side = random.Next(4);

        switch (side)
        {
            case 0:
                return new Vector3Int(
                    -horizontalExtent,
                    random.Next(
                        -verticalExtent,
                        verticalExtent + 1
                    ),
                    0
                );

            case 1:
                return new Vector3Int(
                    horizontalExtent,
                    random.Next(
                        -verticalExtent,
                        verticalExtent + 1
                    ),
                    0
                );

            case 2:
                return new Vector3Int(
                    random.Next(
                        -horizontalExtent,
                        horizontalExtent + 1
                    ),
                    -verticalExtent,
                    0
                );

            default:
                return new Vector3Int(
                    random.Next(
                        -horizontalExtent,
                        horizontalExtent + 1
                    ),
                    verticalExtent,
                    0
                );
        }
    }

    private List<TileBase> GetUsableRuntimeTiles()
    {
        List<TileBase> usableTiles =
            new List<TileBase>();

        if (runtimeDecorationTiles == null)
            return usableTiles;

        foreach (TileBase tile in runtimeDecorationTiles)
        {
            if (tile != null)
                usableTiles.Add(tile);
        }

        return usableTiles;
    }

    [ContextMenu("Generate Stage 1 Decorations")]
    public void GenerateStageOneDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();

        PlaceTile(tilemap, crateTile, -10, 10);
        PlaceTile(tilemap, tombstoneTile, 0, 10);
        PlaceTile(tilemap, barrelTile, 9, 10);
        PlaceTile(tilemap, tombstoneTile, 12, 10);

        PlaceTile(tilemap, barrelTile, -12, -11);
        PlaceTile(tilemap, tombstoneTile, -8, -11);
        PlaceTile(tilemap, crateTile, 9, -11);

        PlaceTile(tilemap, crateTile, -16, -4);
        PlaceTile(tilemap, tombstoneTile, 15, 4);
        PlaceTile(tilemap, barrelTile, 15, -4);

        tilemap.CompressBounds();
    }

    private void PlaceTile(
        Tilemap tilemap,
        TileBase tile,
        int x,
        int y
    )
    {
        if (tile == null)
            return;

        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    [ContextMenu("Clear Decorations")]
    public void ClearDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
    }
}