using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Provision,
    Education,
    Happiness,
    FireSafetyRating,
    WindSpeed,
    Temperature,
    FirefightingEquipment
}

public enum Keyword
{
    Preparation,
    Bushfire,
    Action,
    Operation,
    Cultural,
    Outreach
}

public enum Phase
{
    Preparation, // (P)
    Bushfire     // (B)
}

public enum EffectValueMode
{
    Fixed,   // use Amount
    UseInput // use input X (from GM) * Multiplier
}

public enum TileEffectType
{
    None,
    FuelDelta,
    FireReduce,
    IgniteMod,
    SpreadMod,
    FireImmuneTile,
    PreventSpreadTile,
    BuildingDevelop
}

// What "T=" means for mods.
// Numeric tiles count still comes from TileEffect.Args[2] (t).
public enum TileTarget
{
    Tile,       // single chosen tile
    Global,     // whole map/global rule
    Farmland,
    Building,
    Forest,
    Grassland,
    Groundland,
    Waterbody
}

[CreateAssetMenu(fileName = "NewActionCard", menuName = "Cards/ActionCard")]
public class CardActionData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string cardId;     // e.g. "#F10"
    [SerializeField] private string cardName;

    [Header("Costs")]
    [SerializeField] private int apCost;
    [SerializeField] private int moneyCost;

    [Header("Text")]
    [TextArea, SerializeField] private string cardDescription;

    [Header("Tags")]
    [SerializeField] private List<Keyword> keywords = new();

    [Header("Resource Effects")]
    [SerializeField] private List<ResourceEffect> baseResourceEffects = new();
    [SerializeField] private List<PhaseResourceEffects> phaseResourceEffects = new();
    [SerializeField] private List<OutcomeResourceEffects> outcomeResourceEffects = new();

    [Header("Tile Effects")]
    [SerializeField] private List<TileEffect> baseTileEffects = new();
    [SerializeField] private List<PhaseTileEffects> phaseTileEffects = new();
    [SerializeField] private List<OutcomeTileEffects> outcomeTileEffects = new();

    public string CardId => cardId;
    public string CardName => cardName;
    public int ApCost => apCost;
    public int MoneyCost => moneyCost;
    public string CardDescription => cardDescription;

    public IReadOnlyList<Keyword> Keywords => keywords;

    public IReadOnlyList<ResourceEffect> BaseResourceEffects => baseResourceEffects;
    public IReadOnlyList<PhaseResourceEffects> PhaseResourceEffects => phaseResourceEffects;
    public IReadOnlyList<OutcomeResourceEffects> OutcomeResourceEffects => outcomeResourceEffects;

    public IReadOnlyList<TileEffect> BaseTileEffects => baseTileEffects;
    public IReadOnlyList<PhaseTileEffects> PhaseTileEffects => phaseTileEffects;
    public IReadOnlyList<OutcomeTileEffects> OutcomeTileEffects => outcomeTileEffects;

#if UNITY_EDITOR
    public void SetData(
        string id,
        string name,
        int ap,
        int money,
        string desc,
        List<Keyword> keys,
        List<ResourceEffect> baseResFx,
        List<PhaseResourceEffects> phaseResFx,
        List<OutcomeResourceEffects> outcomeResFx,
        List<TileEffect> baseTileFx,
        List<PhaseTileEffects> phaseTileFx,
        List<OutcomeTileEffects> outcomeTileFx)
    {
        cardId = id;
        cardName = name;

        apCost = Mathf.Max(0, ap);
        moneyCost = Mathf.Max(0, money);

        cardDescription = desc;
        keywords = keys ?? new List<Keyword>();

        baseResourceEffects = baseResFx ?? new List<ResourceEffect>();
        phaseResourceEffects = phaseResFx ?? new List<PhaseResourceEffects>();
        outcomeResourceEffects = outcomeResFx ?? new List<OutcomeResourceEffects>();

        baseTileEffects = baseTileFx ?? new List<TileEffect>();
        phaseTileEffects = phaseTileFx ?? new List<PhaseTileEffects>();
        outcomeTileEffects = outcomeTileFx ?? new List<OutcomeTileEffects>();
    }
#endif
}

[Serializable]
public struct ResourceEffect
{
    public ResourceType resource;
    public EffectValueMode mode;
    public int amount;      // Fixed
    public int multiplier;  // UseInput

    public int Resolve(int inputX) => mode == EffectValueMode.Fixed ? amount : inputX * multiplier;

    public static ResourceEffect Fixed(ResourceType r, int amount)
        => new ResourceEffect { resource = r, mode = EffectValueMode.Fixed, amount = amount, multiplier = 0 };

    public static ResourceEffect UseInput(ResourceType r, int multiplier)
        => new ResourceEffect { resource = r, mode = EffectValueMode.UseInput, amount = 0, multiplier = multiplier };
}

[Serializable]
public struct PhaseResourceEffects
{
    public Phase phase;
    public List<ResourceEffect> effects;
}

[Serializable]
public struct OutcomeResourceEffects
{
    public int outcome; // 1..6 (or 1..2 for coin)
    public List<ResourceEffect> effects;
}

/// <summary>
/// Minimal ordered args:
/// Args[0] = v (value delta)       e.g. -2, -1, +1
/// Args[1] = s (stage filter)      FireReduce only: 0 all, 1, 2; otherwise 0
/// Args[2] = t (tiles count)       int; use -1 for X
/// Args[3] = d (duration)          0 this phase, 1 next, 2 two turns, 3 three turns...
/// Extra target used by Ignite/Spread mods (Tile/Global/Farmland/etc)
/// </summary>
[Serializable]
public struct TileEffect
{
    public TileEffectType type;
    public TileTarget target;   // only meaningful for IgniteMod/SpreadMod; default Tile
    public List<int> args;      // must be length 4

    public int V => args != null && args.Count > 0 ? args[0] : 0;
    public int S => args != null && args.Count > 1 ? args[1] : 0;
    public int T => args != null && args.Count > 2 ? args[2] : 0;
    public int D => args != null && args.Count > 3 ? args[3] : 0;
}

[Serializable]
public struct PhaseTileEffects
{
    public Phase phase;
    public List<TileEffect> effects;
}

[Serializable]
public struct OutcomeTileEffects
{
    public int outcome;
    public List<TileEffect> effects;
}
