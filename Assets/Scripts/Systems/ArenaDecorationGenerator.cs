using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ArenaDecorationGenerator : MonoBehaviour
{
    [Header("Decoration Tiles")]
    [SerializeField] private TileBase crateTile;
    [SerializeField] private TileBase tombstoneTile;
    [SerializeField] private TileBase barrelTile;

    [ContextMenu("Generate Decorations")]
    private void GenerateDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        tilemap.ClearAllTiles();

        // Decorations along the top border.
        PlaceTile(tilemap, crateTile, -10, 10);
        PlaceTile(tilemap, tombstoneTile, 0, 10);
        PlaceTile(tilemap, barrelTile, 9, 10);
        PlaceTile(tilemap, tombstoneTile, 12, 10);

        // Decorations along the bottom border.
        PlaceTile(tilemap, barrelTile, -12, -11);
        PlaceTile(tilemap, tombstoneTile, -8, -11);
        PlaceTile(tilemap, crateTile, 9, -11);

        // Decorations along the left and right borders.
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
        {
            return;
        }

        Vector3Int position = new Vector3Int(x, y, 0);

        tilemap.SetTile(position, tile);
    }

    [ContextMenu("Clear Decorations")]
    private void ClearDecorations()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
    }
}