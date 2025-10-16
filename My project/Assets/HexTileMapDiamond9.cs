using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class HexTilemapDiamond9 : MonoBehaviour
{
    public Tilemap tilemap;     // assign your Hexagonal Flat Top Tilemap
    public TileBase tile;       // tile to paint in the 9 cells
    public bool rebuildOnEnable = true;

    void OnEnable()
    {
        if (rebuildOnEnable) Build();
    }

    [ContextMenu("Build 9-Hex Diamond")]
    public void Build()
    {
        if (!tilemap) tilemap = GetComponent<Tilemap>();
        if (!tilemap || !tile) { Debug.LogError("Assign Tilemap and Tile."); return; }

        // Safety: this must be on a Hexagonal (Flat Top) Grid (odd-q offset)
        var grid = tilemap.layoutGrid;
        if (grid && grid.cellLayout != GridLayout.CellLayout.Hexagon)
            Debug.LogWarning("Grid is not Hexagonal. Create Grid ▸ Hexagonal Flat Top.");

        tilemap.ClearAllTiles();

        // Build columns with counts [1,2,3,2,1] in axial (q,r), then convert to odd-q cells.
        var cells = new List<Vector3Int>();
        for (int q = -2; q <= 2; q++)
        {
            int count = 3 - Mathf.Abs(q);   // 1,2,3,2,1
            int rStart = -(count / 2);      // 1→0 | 2→-1..0 | 3→-1..1
            for (int i = 0; i < count; i++)
            {
                int r = rStart + i;
                cells.Add(AxialOddQToCell(q, r));
            }
        }

        // Shift so the leftmost/topmost starts at (0,0) on the tilemap
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var c in cells) { if (c.x < minX) minX = c.x; if (c.y < minY) minY = c.y; }
        var offset = new Vector3Int(-minX, -minY, 0);

        foreach (var c in cells)
            tilemap.SetTile(c + offset, tile);
    }

    // Axial (q,r) → Unity Hex Flat-Top (odd-q) offset coordinates
    static Vector3Int AxialOddQToCell(int q, int r)
    {
        int col = q;
        int row = r + ((q - (q & 1)) / 2); // odd columns are offset up
        return new Vector3Int(col, row, 0);
    }
}
