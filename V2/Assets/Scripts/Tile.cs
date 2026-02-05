using UnityEngine;

public class Tile : HexagonalGrid
{
    public TileData tileData;

    private bool HasUpgrade() => tileData.currentTileLevel < tileData.upgrades.Count;

    public void Upgrade() {
        if (!HasUpgrade()) return;

        ApplyUpgrade();
        tileData.currentTileLevel++;
    }

    private void ApplyUpgrade() {
        if (!HasUpgrade()) return;
        
        TileUpgrade up = tileData.upgrades[tileData.currentTileLevel];
        if (up.newSprite)
            hexData.spriteRenderer.sprite = up.newSprite;
        tileData.tileType = up.newTileType; // Maybe useful?
    }
}
