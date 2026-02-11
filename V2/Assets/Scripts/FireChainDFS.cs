using System.Collections.Generic;
using UnityEngine;

public static class FireChainDFS
{
    // Cube neighbor directions (x, y, z) with x+y+z=0
    private static readonly Vector3Int[] CubeDirs =
    {
        new Vector3Int(+1, -1,  0),
        new Vector3Int(+1,  0, -1),
        new Vector3Int( 0, +1, -1),
        new Vector3Int(-1, +1,  0),
        new Vector3Int(-1,  0, +1),
        new Vector3Int( 0, -1, +1),
    };

    // Returns: longest_chain, longest_chain_tiles
    public static (int longest_chain, List<string> longest_chain_tiles) FindLongestFireChain(
        Dictionary<string, Vector3Int> nonFireTiles,
        Dictionary<string, Vector3Int> fireTiles)
    {
        if (fireTiles == null || fireTiles.Count == 0)
            return (0, new List<string>());

        // Fast lookup: coord -> id (assumes unique coords)
        var coordToId = new Dictionary<Vector3Int, string>(fireTiles.Count);
        foreach (var kv in fireTiles)
            coordToId[kv.Value] = kv.Key;

        var visited = new HashSet<string>();
        int bestLen = 0;
        List<string> bestPath = new List<string>();

        foreach (var startId in fireTiles.Keys)
        {
            if (visited.Contains(startId)) continue;

            // Collect one connected component using DFS, then pick its size + ids.
            var component = new List<string>();
            DFSComponent(startId, fireTiles, coordToId, visited, component);

            if (component.Count > bestLen)
            {
                bestLen = component.Count;
                bestPath = component; // chain = connected cluster (minimal interpretation)
            }
        }

        return (bestLen, bestPath);
    }

    private static void DFSComponent(
        string currentId,
        Dictionary<string, Vector3Int> fireTiles,
        Dictionary<Vector3Int, string> coordToId,
        HashSet<string> visited,
        List<string> component)
    {
        visited.Add(currentId);
        component.Add(currentId);

        Vector3Int c = fireTiles[currentId];

        for (int i = 0; i < CubeDirs.Length; i++)
        {
            Vector3Int neighborCoord = c + CubeDirs[i];

            if (!coordToId.TryGetValue(neighborCoord, out string neighborId))
                continue;

            if (visited.Contains(neighborId))
                continue;

            DFSComponent(neighborId, fireTiles, coordToId, visited, component);
        }
    }
}


// var (longest_chain, longest_chain_tiles) =
//     FireChainDFS.FindLongestFireChain(nonFireTiles, fireTiles);

// Debug.Log(longest_chain);
// Debug.Log(string.Join(", ", longest_chain_tiles));
