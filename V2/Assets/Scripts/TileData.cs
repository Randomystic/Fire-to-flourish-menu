using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public string tileName;
    public TileType tileType;
    public bool onFire;
    public bool burnt;
    public int fuelLoad;
    public int currentTileLevel = 0;

    public List<TileUpgrade> upgrades;
}

[Serializable]
public class TileUpgrade {
    public Sprite newSprite;
    public TileType newTileType; // May be useful?
    
    // TODO: Add other attribute upgrade perhaps?
}

public enum TileType
{
    FOREST,
    GRASSLAND,
    GROUNDLAND,
    FARMLAND,
    BUILDING,
    WATERBODY
}
