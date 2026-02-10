using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MouseManager : MonoBehaviour
{
    private Tile lastHovered;
    private Tile lastClicked;

    public Button mapButton;

    void Update()
    {
        // TODO: Add mouse clicking input events
        UpdateHovering();
        UpdateInput();
    }

    private void UpdateHovering() {
        if (lastClicked) // Don't apply hovering if we're clicking
            return;
        
        // Check if mouse hits something by just hovering over it
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // If we hit something, check if it is a tile, if so then dim and undim last tile if there was one
        if (hit.collider)
        {
            Tile tile = hit.collider.GetComponent<Tile>();

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

    private void UpdateInput() {
        bool isDown = Input.GetMouseButtonDown(0); // TODO: Use Unity's new input manager
        bool isUp = Input.GetMouseButtonUp(0); // TODO: Use Unity's new input manager
        
        if (isDown && lastHovered && !lastClicked) {
            lastClicked = lastHovered;
            lastClicked.Dim(0.25f);
        }
        
        if (isUp && lastClicked) {
            lastClicked.UnDim();
            lastClicked.Upgrade();
            lastClicked = null;
        }
    }
    
    public void OnMapButtonClicked()
    {
       SceneManager.LoadScene("GameDashboard");
    }

}
