using System;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class HexagonalGrid : MonoBehaviour
{
    [Serializable] public class HexData
    {
        [Header("Hex Size (Auto)")]
        public bool autoSizeFromSprite = true;
        public float hexRadius = 0.5f;

        [Header("Grid Coordinates")]
        public int x;
        public int y;

        public bool snapInEditor = true;

        public SpriteRenderer spriteRenderer;
        public Color originalColor;
        public bool isHoverable = true;
        
        public float HexWidth => 2f * hexRadius;
        public float HexHeight => Mathf.Sqrt(3f) * hexRadius;
    }

    [SerializeField] private HexData hexData;

    private void Awake() {
        hexData.spriteRenderer = GetComponent<SpriteRenderer>();
        hexData.originalColor = hexData.spriteRenderer.color;
    }

    private void OnEnable()
    {
        UpdateHexSize();
    }

    private void Update()
    {
        if (!hexData.snapInEditor || Application.isPlaying) return;

        UpdateHexSize();
        transform.position = HexToWorld(hexData.x, hexData.y);
    }

    private void UpdateHexSize()
    {
        if (!hexData.autoSizeFromSprite || !hexData.spriteRenderer) return;
        hexData.hexRadius = hexData.spriteRenderer.sprite.bounds.size.x / 2f;
    }

    private Vector3 HexToWorld(int x, int y)
    {
        float worldX = hexData.HexWidth * 0.75f * x;
        float worldY = hexData.HexHeight * (y + (x % 2 == 0 ? 0.5f : 0f));
        return new Vector3(worldX, worldY, transform.position.z);
    }

    public void Dim(float alpha = 0.5f) {
        if (!hexData.spriteRenderer)
            return;
        
        Color og = hexData.originalColor;
        hexData.spriteRenderer.color = new Color(og.r, og.g, og.b, alpha); // TODO: Maybe instead of changing alpha I can mathematically make it dim so that the background doesn't show through
    }
    
    public void UnDim() {
        if (!hexData.spriteRenderer)
            return;
        
        Color og = hexData.originalColor;
        hexData.spriteRenderer.color = og;
    }
}

public enum TileType
{
    FOREST,
    GRASSLAND,
    FARMLAND,
    BUILDING,
    WATERBODY
}
