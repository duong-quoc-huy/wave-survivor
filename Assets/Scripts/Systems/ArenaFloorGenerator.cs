using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ArenaFloorGenerator : MonoBehaviour
{
    [Header("Floor Tiles")]
    [SerializeField] private TileBase baseTile;
    [SerializeField] private TileBase variationTile;

    [Header("Border Tile")]
    [SerializeField] private TileBase borderTile;

    [SerializeField, Min(1)]
    private int borderThickness = 1;

    [Header("Arena Size")]
    [SerializeField, Min(1)]
    private int width = 30;

    [SerializeField, Min(1)]
    private int height = 20;

    [Header("Floor Variation")]
    [SerializeField, Range(0f, 1f)]
    private float variationChance = 0.08f;

    [SerializeField] private int randomSeed = 12345;

    [ContextMenu("Generate Floor")]
    private void GenerateFloor()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (baseTile == null)
        {
            Debug.LogError(
                "ArenaFloorGenerator requires a Base Tile.",
                this
            );

            return;
        }

        tilemap.ClearAllTiles();

        int startX = -(width / 2);
        int startY = -(height / 2);

        GenerateBorder(tilemap, startX, startY);
        GenerateFloorTiles(tilemap, startX, startY);

        tilemap.CompressBounds();
    }

    private void GenerateBorder(
        Tilemap tilemap,
        int startX,
        int startY
    )
    {
        if (borderTile == null)
        {
            return;
        }

        int minimumX = -borderThickness;
        int maximumX = width + borderThickness;
        int minimumY = -borderThickness;
        int maximumY = height + borderThickness;

        for (int x = minimumX; x < maximumX; x++)
        {
            for (int y = minimumY; y < maximumY; y++)
            {
                bool isOutsideFloor =
                    x < 0 ||
                    x >= width ||
                    y < 0 ||
                    y >= height;

                if (!isOutsideFloor)
                {
                    continue;
                }

                Vector3Int position = new Vector3Int(
                    startX + x,
                    startY + y,
                    0
                );

                tilemap.SetTile(position, borderTile);
            }
        }
    }

    private void GenerateFloorTiles(
        Tilemap tilemap,
        int startX,
        int startY
    )
    {
        System.Random random =
            new System.Random(randomSeed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool useVariation =
                    variationTile != null &&
                    random.NextDouble() < variationChance;

                TileBase selectedTile =
                    useVariation
                        ? variationTile
                        : baseTile;

                Vector3Int position = new Vector3Int(
                    startX + x,
                    startY + y,
                    0
                );

                tilemap.SetTile(position, selectedTile);
            }
        }
    }
}