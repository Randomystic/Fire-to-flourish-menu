using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Use cube coordinates (x,y,z) with x+y+z = 0 for hex grids
    public Dictionary<Vector3Int, MapTile> tiles = new Dictionary<Vector3Int, MapTile>();

    public MapTile GetTile(int x, int y, int z)
    {
        tiles.TryGetValue(new Vector3Int(x, y, z), out var t);
        return t; // null if not found
    }

    public List<MapTile> GetNeighbors(int x, int y, int z)
    {
        var origin = new Vector3Int(x, y, z);
        var dirs = new[]
        {
            new Vector3Int( 1, -1,  0),
            new Vector3Int( 1,  0, -1),
            new Vector3Int( 0,  1, -1),
            new Vector3Int(-1,  1,  0),
            new Vector3Int(-1,  0,  1),
            new Vector3Int( 0, -1,  1),
        };

        var result = new List<MapTile>();
        foreach (var d in dirs)
        {
            if (tiles.TryGetValue(origin + d, out var n))
                result.Add(n);
        }
        return result;
    }
}
