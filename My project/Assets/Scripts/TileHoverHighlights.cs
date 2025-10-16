using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler 
{
    [Header("Highlight Settings")]
    public Color hoverTint = new Color(1f, 1f, 1f, 0.7f); // Bright red at 70% opacity

    private SpriteRenderer sr;
    private Color baseColor;
    private int baseSortingOrder;

    [Header("Click-to-Cycle Settings")]
    public string resourcesFolder = "Tiles/Images"; 
    public int maxVariantCount = 3;  
    private string baseName; 
    private int variantIndex = 0;  

    // cache list of all base tile names found under resourcesFolder (e.g., town_center, river, ...)
    private static string[] s_allBaseNames;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (!sr)
        {
            Debug.LogWarning($"[Hover] No SpriteRenderer found on {name}. Attempting to find in children.");
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (sr)
        {
            baseColor = sr.color;
            baseSortingOrder = sr.sortingOrder;
            Debug.Log($"[Hover] Awake on '{name}', initial color={baseColor}, sortingOrder={baseSortingOrder}");

            // Detect base/variant from current sprite name
            if (sr.sprite != null)
            {
                string n = sr.sprite.name; // e.g. "town_center" or "town_center_2"
                baseName = n;
                variantIndex = 0;
                if (n.EndsWith("_2"))
                {
                    baseName = n.Substring(0, n.Length - 2);
                    variantIndex = 1;
                }
                else if (n.EndsWith("_3"))
                {
                    baseName = n.Substring(0, n.Length - 2);
                    variantIndex = 2;
                }
                Debug.Log($"[Hover] Sprite='{n}', baseName='{baseName}', variantIndex={variantIndex}");
            }
        }
        else
        {
            Debug.LogError($"[Hover] Still no SpriteRenderer on {name} - highlight disabled.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[Hover] ENTER {name} at {eventData.position} (pointerId={eventData.pointerId})");

        if (sr)
        {
            sr.color = hoverTint;
            sr.sortingOrder = baseSortingOrder + 5; // Bring forward visually
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Hover] EXIT {name} at {eventData.position} (pointerId={eventData.pointerId})");

        if (sr)
        {
            sr.color = baseColor;
            sr.sortingOrder = baseSortingOrder;
        }
    }

    // Click
    // Left: cycle variants of same base (base -> _2 -> _3)
    // Right: loop across all base tile types (town_center -> river -> school -> ...)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (sr == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left-click: existing upgrade/variant cycle
            for (int step = 1; step <= maxVariantCount; step++)
            {
                int candidate = (variantIndex + step) % maxVariantCount;
                string nextName = candidate == 0 ? baseName
                                : candidate == 1 ? $"{baseName}_2"
                                : $"{baseName}_3";

                var nextSprite = Resources.Load<Sprite>($"{resourcesFolder}/{nextName}");
                if (nextSprite != null)
                {
                    ApplyVisualAndData(nextName, nextSprite);
                    variantIndex = candidate;
                    Debug.Log($"[Hover] Click cycle (LEFT) on '{gameObject.name}': switched to '{nextName}' (variantIndex={variantIndex})");
                    return;
                }
            }

            Debug.Log($"[Hover] Click cycle (LEFT): no alternate variants found for '{baseName}' in '{resourcesFolder}'.");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right-click: cycle through ALL base tile types
            EnsureAllBaseNamesLoaded();
            if (s_allBaseNames == null || s_allBaseNames.Length == 0)
            {
                Debug.LogWarning("[Hover] Right-click: no base names discovered in resources.");
                return;
            }

            int idx = 0;
            for (int i = 0; i < s_allBaseNames.Length; i++)
                if (s_allBaseNames[i] == baseName) { idx = i; break; }

            int nextIdx = (idx + 1) % s_allBaseNames.Length;
            string nextBase = s_allBaseNames[nextIdx];

            // Try base sprite first, then _2, then _3 (in case only variants exist)
            string[] candidates = { nextBase, $"{nextBase}_2", $"{nextBase}_3" };
            foreach (var cand in candidates)
            {
                var spr = Resources.Load<Sprite>($"{resourcesFolder}/{cand}");
                if (spr != null)
                {
                    ApplyVisualAndData(cand, spr);
                    baseName = nextBase;            // update current base
                    variantIndex = cand.EndsWith("_3") ? 2 : cand.EndsWith("_2") ? 1 : 0;
                    Debug.Log($"[Hover] Click cycle (RIGHT) on '{gameObject.name}': switched to '{cand}' (base='{baseName}', variantIndex={variantIndex})");
                    return;
                }
            }

            Debug.Log($"[Hover] Right-click: couldn't find sprite for '{nextBase}' (tried base/_2/_3).");
        }
    }


    void EnsureAllBaseNamesLoaded()
    {
        if (s_allBaseNames != null) return;

        var allSprites = Resources.LoadAll<Sprite>(resourcesFolder);
        var set = new HashSet<string>();
        foreach (var sp in allSprites)
        {
            if (sp == null) continue;
            string n = sp.name;
            string b = n;
            if (n.EndsWith("_2") || n.EndsWith("_3"))
                b = n.Substring(0, n.Length - 2);
            set.Add(b);
        }

        var list = new List<string>(set);
        list.Sort(); // deterministic order
        s_allBaseNames = list.ToArray();
        Debug.Log($"[Hover] Loaded {s_allBaseNames.Length} base tile names from '{resourcesFolder}'.");
    }

    void ApplyVisualAndData(string nextName, Sprite nextSprite)
    {
        sr.sprite = nextSprite;

        // Keep MapTile data in sync with visual
        var mt = GetComponent<MapTile>();
        if (mt != null)
        {
            mt.tileName = nextName;

            // Try to sync data from MapTileData that matches the *base* name (without _2/_3)
            string dataKey = nextName.EndsWith("_2") || nextName.EndsWith("_3")
                ? nextName.Substring(0, nextName.Length - 2)
                : nextName;

            var data = Resources.Load<MapTileData>($"Tiles/{dataKey}");
            if (data != null)
            {
                mt.tileType = data.tileType;
                mt.fuelLoad = data.fuelLoad;
                mt.onFire   = data.onFire;
                mt.burnt    = data.burnt;
            }

            // update Map.Instance.tiles entry
            var map = Map.Instance;
            if (map != null)
            {
                if (map.tiles.ContainsKey(mt.cubeCoord))
                    map.tiles[mt.cubeCoord] = mt;
                else
                    map.tiles.Add(mt.cubeCoord, mt);

                gameObject.name = $"Tile_{mt.tileName}_{mt.cubeCoord.x}_{mt.cubeCoord.z}";
                Debug.Log($"[Hover] Synced Map.tiles at {mt.cubeCoord} -> {mt.tileName}");
            }
            else
            {
                Debug.LogWarning("[Hover] Map.Instance is null; cannot sync tiles dictionary.");
            }
        }
    }
}
