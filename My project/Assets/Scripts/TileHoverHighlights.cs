using UnityEngine;
using UnityEngine.EventSystems;

public class TileHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Highlight Settings")]
    public Color hoverTint = new Color(1f, 1f, 1f, 0.5f); // Bright red at 70% opacity

    private SpriteRenderer sr;
    private Color baseColor;
    private int baseSortingOrder;

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
}
