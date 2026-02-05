using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private HexagonalGrid lastHovered;

    void Update()
    {
        // TODO: Add mouse clicking input events
        UpdateHovering();
    }

    private void UpdateHovering()
    {
        // Check if mouse hits something by just hovering over it
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // If we hit something, check if it is a tile, if so then dim and undim last tile if there was one
        if (hit.collider)
        {
            HexagonalGrid tile = hit.collider.GetComponent<HexagonalGrid>();

            if (tile)
            {
                if (lastHovered != tile)
                {
                    if (lastHovered)
                        lastHovered.UnDim();

                    lastHovered = tile;
                    lastHovered.Dim();
                }
                return;
            }
        }

        if (lastHovered)
        {
            lastHovered.UnDim();
            lastHovered = null;
        }
    }
    
}
