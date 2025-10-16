using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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

    public Button saveButton;

    private bool _initialized; // guard

    private int fixedIndex = 0;

    private string[] fixedOrder = new string[]
        {
            "town_center",
            "river",
            "school",
            "grassland",
            "building",
            "indigenous_land",
            "farmland",
            "fire_station",
            "hospital"
        };
    

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        if (tilesParent && !tilesParent.gameObject.scene.IsValid())
            tilesParent = null;

    }

    void Start()
    {
        EnsureInitialized();
        Generate_1_2_3_2_1();

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);

        var cam = Camera.main;
        if (cam && cam.GetComponent<Physics2DRaycaster>() == null)
            cam.gameObject.AddComponent<Physics2DRaycaster>();
        
        // --- DEBUG: input pipeline + canvas state ---
    
        if (EventSystem.current == null)
            Debug.LogWarning("DEBUG Map: No EventSystem found in scene.");
        else
            Debug.Log($"DEBUG Map: EventSystem present -> {EventSystem.current.gameObject.name}");

        if (!cam) Debug.LogWarning("DEBUG Map: Camera.main is NULL.");
        else
        {
            var pr2d = cam.GetComponent<Physics2DRaycaster>();
            Debug.Log($"DEBUG Map: Camera.main '{cam.name}', Physics2DRaycaster attached: {(pr2d ? "YES" : "NO")}" +
                    (pr2d ? $" (eventMask={pr2d.eventMask})" : ""));
        }

        // List canvases + raycasters so we can see who might block pointer hits
        var canvases = FindObjectsOfType<Canvas>(true);
        foreach (var c in canvases)
        {
            var gr = c.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log($"DEBUG Map: Canvas '{c.name}' active={c.gameObject.activeInHierarchy}, enabled={c.enabled}, " +
                    $"renderMode={c.renderMode}, worldCam={(c.worldCamera ? c.worldCamera.name : "null")}, " +
                    $"planeDistance={c.planeDistance}, sortingOrder={c.sortingOrder}, GraphicRaycaster={(gr ? "YES" : "NO")}");
        }
    }
    

    void Save()
    {
        SceneManager.LoadScene("PreparationPhase");
    }

    public float GetAverageFuelLoad()
    {
        if (tiles == null || tiles.Count == 0)
            return 0f;

        float totalFuel = 0f;
        int count = 0;

        foreach (var kv in tiles)
        {
            var tile = kv.Value;
            totalFuel += tile.fuelLoad;
            count++;
        }

        return count > 0 ? totalFuel / count : 0f;
    }


    public void EnsureInitialized()
    {
        if (_initialized) return;

        EnsureParent();
        LoadAllTileAssets();
        AutoFillOffsetsFromSpriteIfNeeded();

        if (!tilePrefab)
        {
            Debug.LogError("Map: tilePrefab not assigned on the persistent Map.");
            return;
        }
        if (allTileAssets.Count == 0)
        {
            Debug.LogError("Map: no MapTileData in Resources/Tiles/.");
            return;
        }


        _initialized = tiles.Count > 0;
        Debug.Log($"Map initialized with {tiles.Count} tiles. (InstanceID {GetInstanceID()})");
    }

    void EnsureParent()
    {
        if (tilesParent && !tilesParent.gameObject.scene.IsValid())
            tilesParent = null;

        if (!tilesParent)
        {
            tilesParent = new GameObject("Tiles (Runtime)").transform;
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


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _initialized = false;
    }


    [Header("Seven-hex ring")]
    public float stepDistance = 20f;
    public float angleBiasDeg = -30f;

    // Cube unit directions in flat-top order (E, NE, NW, W, SW, SE)
    static readonly Vector3Int[] CubeDirsFlatTop =
    {
        new Vector3Int( 1,-1, 0),  // 0°  (E)
        new Vector3Int( 1, 0,-1),  // 60° (NE)
        new Vector3Int( 0, 1,-1),  // 120°(NW)
        new Vector3Int(-1, 1, 0),  // 180°(W)
        new Vector3Int(-1, 0, 1),  // 240°(SW)
        new Vector3Int( 0,-1, 1),  // 300°(SE)
    };

    // Simple 2D rotator
    static Vector2 Rotate2D(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    // Flat-top axial basis: q at 0°, r at 60°
    static Vector2 AxialToPlanar_FlatTop(int q, int r)
    {
        const float SQRT3_2 = 0.8660254037844386f; // √3/2
        Vector2 basisQ = new Vector2(1f, 0f);            // 0°
        Vector2 basisR = new Vector2(0.5f, SQRT3_2);     // 60°
        return q * basisQ + r * basisR;
    }

    // Cube -> world using biased angle (hard-rotated by angleBiasDeg)
    public Vector3 WorldFromCube_Biased(Vector3Int cube, float step = 20f)
    {
        // center tile
        if (cube == Vector3Int.zero)
            return new Vector3(startX, startY, 0f);

        // axial (q,r)
        int q = cube.x;
        int r = cube.z;

        // direction in flat-top frame, then rotate by the bias
        Vector2 dir = AxialToPlanar_FlatTop(q, r);
        dir = Rotate2D(dir, angleBiasDeg);

        // how many steps away this cube is
        int steps = (Mathf.Abs(cube.x) + Mathf.Abs(cube.y) + Mathf.Abs(cube.z)) / 2;

        // normalize and scale to steps * step distance
        Vector2 offset = dir.normalized * (step * steps);

        return new Vector3(startX + offset.x, startY + offset.y, 0f);
    }

 

    // Helper to spawn a tile (uses your existing data/prefab; adjust as needed)
    void CreateTileAtCube(Vector3Int cube, Vector3 worldPos, bool logAngle, float angleDeg = 0f)
    {
        // pick next name in the fixed order
        string tileName = fixedOrder[fixedIndex % fixedOrder.Length];
        fixedIndex++;

        // find corresponding MapTileData by name
        var data = allTileAssets.Find(x => x.tileName == tileName);
        if (data == null)
        {
            Debug.LogError($"No MapTileData found for {tileName}");
            return;
        }

        // instantiate tile object
        // instantiate tile object
        var obj = Instantiate(tilePrefab, tilesParent);
        obj.name = $"Tile_{data.tileName}_{cube.x}_{cube.z}";
        obj.transform.position = worldPos;
        obj.transform.localScale = Vector3.one * tileScale;

        Debug.Log($"DEBUG Map: Spawning '{data.tileName}' at world {worldPos} cube {cube} layer={obj.layer}");

        // assign sprite
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr)
        {
            var sprite = Resources.Load<Sprite>($"Tiles/Images/{data.tileName}");
            if (sprite) sr.sprite = sprite;
            sr.sortingOrder = 1;
            Debug.Log($"DEBUG Map: SpriteRenderer OK. sortingLayer='{sr.sortingLayerName}', order={sr.sortingOrder}, color={sr.color}");
        }
        else
        {
            Debug.LogWarning("DEBUG Map: Tile has NO SpriteRenderer (hover won't tint).");
        }

        // ensure there's a 2D collider
        Collider2D col = obj.GetComponent<Collider2D>();
        if (!col)
        {
            col = obj.AddComponent<PolygonCollider2D>(); // or BoxCollider2D
            ((PolygonCollider2D)col).isTrigger = true;
            Debug.Log("DEBUG Map: Added PolygonCollider2D (isTrigger=true).");
        }
        else
        {
            Debug.Log($"DEBUG Map: Found existing collider -> {col.GetType().Name}.");
        }

        // add/verify the hover highlight behavior
        if (!obj.TryGetComponent<TileHoverHighlight>(out var hover))
        {
            hover = obj.AddComponent<TileHoverHighlight>();
            Debug.Log("DEBUG Map: Added TileHoverHighlight.");
        }
        else
        {
            Debug.Log("DEBUG Map: TileHoverHighlight already present.");
        }


        // assign sprite
        // var sr = obj.GetComponent<SpriteRenderer>();
        if (sr)
        {
            var sprite = Resources.Load<Sprite>($"Tiles/Images/{data.tileName}");
            if (sprite) sr.sprite = sprite;
            sr.sortingOrder = 1;
        }

        // ensure there's a 2D collider so OnMouseEnter/Exit can fire
        if (!obj.GetComponent<Collider2D>())
        {
            var poly = obj.AddComponent<PolygonCollider2D>(); // or BoxCollider2D
            poly.isTrigger = true; // optional
        }

        // add the hover highlight behavior
        if (!obj.GetComponent<Collider2D>()) obj.AddComponent<PolygonCollider2D>(); // or BoxCollider2D
        if (!obj.GetComponent<TileHoverHighlight>()) obj.AddComponent<TileHoverHighlight>();


        // assign tile data
        var t = obj.GetComponent<MapTile>();
        t.tileName   = data.tileName;
        t.tileType   = data.tileType;
        t.onFire     = data.onFire;
        t.burnt      = data.burnt;
        t.fuelLoad   = data.fuelLoad;
        t.cubeCoord  = cube;

        tiles[cube] = t;

        if (logAngle)
            Debug.Log($"Placed {t.tileName} (fixed index {fixedIndex - 1}) at angle {angleDeg:0}° -> world {worldPos}");
    }


    // Build 1,2,3,2,1 vertical-slice pattern (9 tiles total)
    public void Generate_1_2_3_2_1()
    {
        EnsureParent();
        LoadAllTileAssets();

        // wipe old
        tiles.Clear();
        for (int i = tilesParent.childCount - 1; i >= 0; --i)
            Destroy(tilesParent.GetChild(i).gameObject);

        // center
        CreateTileAtCube(Vector3Int.zero, new Vector3(startX, startY, 0f), logAngle:false);

        // six neighbors at distance 1 (E, NE, NW, W, SW, SE)
        for (int i = 0; i < 6; i++)
        {
            Vector3Int cube = CubeDirsFlatTop[i];
            Vector3 world   = WorldFromCube_Biased(cube, stepDistance);
            CreateTileAtCube(cube, world, logAngle:true, angleDeg: i * 60f + angleBiasDeg);
        }

        // Global up/down relative to center tile

        Vector3 centerWorld = WorldFromCube_Biased(Vector3Int.zero, stepDistance);

        Vector3 northWorld = centerWorld + new Vector3(stepDistance * 0.866f * 2f, 0f, 0f);  // tweak 0.866f if your tile height differs
        Vector3 southWorld = centerWorld + new Vector3(-stepDistance * 0.866f * 2f, 0f, 0f);

        // Assign fake cube-coords just for storage (optional)
        Vector3Int northCube = new Vector3Int(0, 0, 999); 
        Vector3Int southCube = new Vector3Int(0, 0, -999);

        CreateTileAtCube(northCube, northWorld, logAngle:true, angleDeg:90f);
        CreateTileAtCube(southCube, southWorld, logAngle:true, angleDeg:270f);

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool showMap = scene.name == "Map";
        Debug.Log($"DEBUG Map: OnSceneLoaded -> '{scene.name}', showMap={showMap}");
        SetMapVisibility(showMap);

        if (showMap)
        {
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            bool found = false;
            foreach (var canvas in allCanvases)
            {
                if (canvas.gameObject.scene.name != scene.name) continue;
                if (!canvas.gameObject.activeInHierarchy) canvas.gameObject.SetActive(true);
                canvas.enabled = true;
                found = true;
                Debug.Log($"DEBUG Map: Re-enabled Canvas '{canvas.name}' in scene '{scene.name}'.");
            }
            if (!found) Debug.LogWarning("DEBUG Map: No Canvas found in Map scene.");
        }
    }

    public void SetMapVisibility(bool visible)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!sr.gameObject.name.StartsWith("Tile_")) // <-- avoid disabling tiles
                sr.enabled = visible;
        }

        foreach (var col in GetComponentsInChildren<Collider2D>(true))
        {
            if (!col.gameObject.name.StartsWith("Tile_")) // <-- avoid disabling tile colliders
                col.enabled = visible;
        }

        foreach (var canvas in GetComponentsInChildren<Canvas>(true))
            canvas.enabled = visible;

        Debug.Log($"Map visibility set to {visible}");
    }







}