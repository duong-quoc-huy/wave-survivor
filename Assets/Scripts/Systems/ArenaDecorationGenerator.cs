using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ArenaDecorationGenerator : MonoBehaviour
{
    [Header("Decoration Tiles")]
    [FormerlySerializedAs("crateTile")]
    [SerializeField] private TileBase decorationTileA;

    [FormerlySerializedAs("tombstoneTile")]
    [SerializeField] private TileBase decorationTileB;

    [FormerlySerializedAs("barrelTile")]
    [SerializeField] private TileBase decorationTileC;

    [Header("Decoration Appearance")]
    [SerializeField]
    private Color decorationTint = Color.white;

    public void ConfigureTheme(
        TileBase newTileA,
        TileBase newTileB,
        TileBase newTileC,
        Color newTint
    )
    {
        // Keep current scene values if a stage asset has not been
        // visually configured yet.
        if (newTileA != null)
            decorationTileA = newTileA;

        if (newTileB != null)
            decorationTileB = newTileB;

        if (newTileC != null)
            decorationTileC = newTileC;

        decorationTint = newTint;
    }

    [ContextMenu("Generate Decorations")]
    public void GenerateDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        tilemap.color = decorationTint;
        tilemap.ClearAllTiles();

        // Decorations along the top border.
        PlaceTile(tilemap, decorationTileA, -10, 10);
        PlaceTile(tilemap, decorationTileB, 0, 10);
        PlaceTile(tilemap, decorationTileC, 9, 10);
        PlaceTile(tilemap, decorationTileB, 12, 10);

        // Decorations along the bottom border.
        PlaceTile(tilemap, decorationTileC, -12, -11);
        PlaceTile(tilemap, decorationTileB, -8, -11);
        PlaceTile(tilemap, decorationTileA, 9, -11);

        // Decorations along the left and right borders.
        PlaceTile(tilemap, decorationTileA, -16, -4);
        PlaceTile(tilemap, decorationTileB, 15, 4);
        PlaceTile(tilemap, decorationTileC, 15, -4);

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

        tilemap.SetTile(
            new Vector3Int(x, y, 0),
            tile
        );
    }

    [ContextMenu("Clear Decorations")]
    public void ClearDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
    }
}