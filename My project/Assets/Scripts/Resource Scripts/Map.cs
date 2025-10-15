using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 5;
    public int height = 7;

    private Dictionary<Vector3Int, MapTile> tiles = new Dictionary<Vector3Int, MapTile>();
    private List<MapTileData> allTileAssets = new List<MapTileData>();

    void Start()
    {
        LoadAllTileAssets();
        GenerateRandomMap();
        PrintTileSummary();
    }

    void LoadAllTileAssets()
    {
        allTileAssets.Clear();
        MapTileData[] loadedTiles = Resources.LoadAll<MapTileData>("Tiles");

        if (loadedTiles.Length == 0)
        {
            Debug.LogError("No MapTileData assets found in Resources/Tiles/");
            return;
        }

        allTileAssets.AddRange(loadedTiles);
        Debug.Log($"Loaded {allTileAssets.Count} tile assets.");
    }

    void GenerateRandomMap()
    {
        if (!tilePrefab)
        {
            Debug.LogError("Missing tilePrefab reference!");
            return;
        }

        tiles.Clear();

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                // pick a random tile asset
                MapTileData data = allTileAssets[Random.Range(0, allTileAssets.Count)];

                // hex cube coordinate (x + y + z = 0)
                int x = q;
                int z = r;
                int y = -x - z;
                var cubeCoord = new Vector3Int(x, y, z);

                // instantiate
                var obj = Instantiate(tilePrefab, transform);
                obj.name = $"Tile_{data.tileName}_{q}_{r}";
                obj.transform.position = new Vector3(q * 1.1f, 0, r * 1.0f);

                // apply data
                var mapTile = obj.GetComponent<MapTile>();
                mapTile.tileName = data.tileName;
                mapTile.tileType = data.tileType;
                mapTile.onFire = data.onFire;
                mapTile.burnt = data.burnt;
                mapTile.fuelLoad = data.fuelLoad;
                mapTile.cubeCoord = cubeCoord;

                var renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer)
                {
                    var sprite = Resources.Load<Sprite>($"Tiles/Images/{data.tileName}");
                    if (sprite) renderer.sprite = sprite;
                    else Debug.LogWarning($"Missing sprite for {data.tileName}");
                }

                tiles[cubeCoord] = mapTile;
            }
        }

        Debug.Log($"Generated random map with {tiles.Count} tiles.");
    }

    void PrintTileSummary()
    {
        Debug.Log("----- TILE SUMMARY -----");
        foreach (var kv in tiles)
        {
            Vector3Int c = kv.Key;
            MapTile t = kv.Value;

            Debug.Log(
                $"Coord ({c.x},{c.y},{c.z}) | " +
                $"Name: {t.tileName} | " +
                $"Type: {t.tileType} | " +
                $"OnFire: {t.onFire} | " +
                $"Burnt: {t.burnt} | " +
                $"FuelLoad: {t.fuelLoad}"
            );
        }
        Debug.Log("------------------------");
    }
    
}
