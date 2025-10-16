using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Tile Position Settings")]
    public float startX = 0f;
    public float startY = 0f;
    public float tileScale = 1f;
    [SerializeField] private Transform tilesParent;

    [Header("Hex Offsets (flat-top)")]
    public float xOffset = 0f;
    public float yOffset = 0f;
    public float evenRowXOffset = 0f;


    [Header("Map Settings")]
    public GameObject tilePrefab;
    public int width = 5;
    public int height = 7;

    public readonly Dictionary<Vector3Int, MapTile> tiles = new();
    private readonly List<MapTileData> allTileAssets = new();

    public static Map Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    void Start()
    {
        EnsureParent();
        LoadAllTileAssets();
        AutoFillOffsetsFromSpriteIfNeeded();
        GenerateRandomMap();
        PrintTileSummary();
    }

    void EnsureParent()
    {
        if (!tilesParent)
        {
            var existing = GameObject.Find("Tiles");
            tilesParent = existing ? existing.transform : new GameObject("Tiles").transform;
            tilesParent.SetParent(transform, false);
        }
    }

    void LoadAllTileAssets()
    {
        allTileAssets.Clear();
        var loaded = Resources.LoadAll<MapTileData>("Tiles");
        if (loaded.Length == 0)
        {
            Debug.LogError("No MapTileData assets found in Resources/Tiles/");
            return;
        }
        allTileAssets.AddRange(loaded);
    }

    void AutoFillOffsetsFromSpriteIfNeeded()
    {
        if (!tilePrefab) return;
        var sr = tilePrefab.GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite) return;

        // World-space size of the sprite at scale = 1
        var baseSize = sr.sprite.bounds.size;
        float hexWidthWorld  = baseSize.x * tileScale;
        float hexHeightWorld = baseSize.y * tileScale;


        if (xOffset <= 0f) xOffset = hexWidthWorld * 0.75f;
        if (yOffset <= 0f) yOffset = hexHeightWorld;


    }

    void GenerateRandomMap()
    {
        if (!tilePrefab || allTileAssets.Count == 0) return;

        tiles.Clear();

        for (int row = 0; row < height; row++)
        {
            bool isEvenRow = (row % 2) == 0;
            float rowXShift = isEvenRow ? evenRowXOffset : 0f;

            for (int col = 0; col < width; col++)
            {
                var data = allTileAssets[Random.Range(0, allTileAssets.Count)];

                // Position (flat-top, row-staggered)
                float xPos = startX + rowXShift + col * xOffset;
                float yPos = startY - row * yOffset;

                // Instantiate
                var obj = Instantiate(tilePrefab, tilesParent);
                obj.name = $"Tile_{data.tileName}_{col}_{row}";
                obj.transform.position = new Vector3(xPos, yPos, 0f);
                obj.transform.localScale = Vector3.one * tileScale;

                // Assign sprite
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr)
                {
                    var sprite = Resources.Load<Sprite>($"Tiles/Images/{data.tileName}");
                    if (sprite) sr.sprite = sprite;
                    sr.sortingOrder = 1;
                }

                // Assign data to component
                var mapTile = obj.GetComponent<MapTile>();
                mapTile.tileName = data.tileName;
                mapTile.tileType = data.tileType;
                mapTile.onFire   = data.onFire;
                mapTile.burnt    = data.burnt;
                mapTile.fuelLoad = data.fuelLoad;

                // Cube coord (even-r offset → axial → cube)
                // axial q = col - (row/2), r = row
                int qAx = col - (row / 2);
                int rAx = row;
                int x = qAx;
                int z = rAx;
                int y = -x - z;
                mapTile.cubeCoord = new Vector3Int(x, y, z);

                tiles[mapTile.cubeCoord] = mapTile;
            }
        }
    }

    void PrintTileSummary()
    {
        Debug.Log("TILE SUMMARY");
        foreach (var kv in tiles)
        {
            var c = kv.Key;
            var t = kv.Value;
            Debug.Log($"Coord ({c.x},{c.y},{c.z}) | Name: {t.tileName} | Type: {t.tileType} | OnFire: {t.onFire} | Burnt: {t.burnt} | FuelLoad: {t.fuelLoad}");
        }
        Debug.Log("_____________________________");
    }
}
