using UnityEngine;
using UnityEngine.EventSystems;

public class TileHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler 
{
    [Header("Highlight Settings")]
    public Color hoverTint = new Color(1f, 1f, 1f, 0.5f);

    private SpriteRenderer sr;
    private Color baseColor;
    private int baseSortingOrder;

    [Header("Click-to-Cycle Settings")]
    public string resourcesFolder = "Tiles/Images"; 
    public int maxVariantCount = 3;  
    private string baseName; 
    private int variantIndex = 0;  

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

    // Click cycles through base → _2 → _3 (if they exist in Resources)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (sr == null || eventData.button != PointerEventData.InputButton.Left) return;

        // Try up to maxVariantCount-1 other variants, wrapping around
        for (int step = 1; step <= maxVariantCount; step++)
        {
            int candidate = (variantIndex + step) % maxVariantCount; // 0,1,2
            string nextName = candidate == 0 ? baseName
                            : candidate == 1 ? $"{baseName}_2"
                            : $"{baseName}_3";

            var nextSprite = Resources.Load<Sprite>($"{resourcesFolder}/{nextName}");
            if (nextSprite != null)
            {
                sr.sprite = nextSprite;
                variantIndex = candidate;

                // Keep MapTile data in sync with visual
                var mt = GetComponent<MapTile>();
                if (mt != null)
                {
                    mt.tileName = nextName;

                    // (Optional but useful) sync other fields from MapTileData if present
                    var data = Resources.Load<MapTileData>($"Tiles/{nextName}");
                    if (data != null)
                    {
                        mt.tileType = data.tileType;
                        mt.fuelLoad = data.fuelLoad;
                        mt.onFire   = data.onFire;
                        mt.burnt    = data.burnt;
                    }

                    // Update the Map.Instance.tiles dictionary entry <<<
                    var map = Map.Instance;
                    if (map != null)
                    {
                        // Ensure the dictionary points to THIS MapTile for its cube coord
                        if (map.tiles.ContainsKey(mt.cubeCoord))
                            map.tiles[mt.cubeCoord] = mt;
                        else
                            map.tiles.Add(mt.cubeCoord, mt);

                        // Keep GameObject name consistent (optional, helps debugging)
                        gameObject.name = $"Tile_{mt.tileName}_{mt.cubeCoord.x}_{mt.cubeCoord.z}";

                        Debug.Log($"[Hover] Updated Map.tiles at {mt.cubeCoord} -> {mt.tileName}");
                    }
                    else
                    {
                        Debug.LogWarning("[Hover] Map.Instance is null; cannot sync tiles dictionary.");
                    }
                }

                Debug.Log($"[Hover] Click cycle on '{gameObject.name}': switched to '{nextName}' (variantIndex={variantIndex})");
                return;
            }
        }

        Debug.Log($"[Hover] Click cycle: no alternate variants found for '{baseName}' in '{resourcesFolder}'.");
    }
}
