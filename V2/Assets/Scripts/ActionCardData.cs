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
    [Tooltip("Always applied when the card is logged as played.")]
    [SerializeField] private List<ResourceEffect> baseResourceEffects = new();

    [Tooltip("Applied only when a phase is supplied in input: (P) or (B).")]
    [SerializeField] private List<PhaseEffectBlock> phaseResourceEffects = new();

    [Tooltip("Applied only when an outcome is supplied in input: (1..6) or coin (1|2).")]
    [SerializeField] private List<OutcomeEffectBlock> outcomeResourceEffects = new();

    [Header("Tile Effects")]
    [Tooltip("Raw tile effects text from CSV (kept as-is for minimal approach).")]
    [SerializeField] private string tileEffectsRaw = "None";

    public string CardId => cardId;
    public string CardName => cardName;
    public int ApCost => apCost;
    public int MoneyCost => moneyCost;
    public string CardDescription => cardDescription;

    public IReadOnlyList<Keyword> Keywords => keywords;

    // New preferred names
    public IReadOnlyList<ResourceEffect> BaseResourceEffects => baseResourceEffects;
    public IReadOnlyList<PhaseEffectBlock> PhaseResourceEffects => phaseResourceEffects;
    public IReadOnlyList<OutcomeEffectBlock> OutcomeResourceEffects => outcomeResourceEffects;

    public string TileEffectsRaw => tileEffectsRaw;

    // Backwards-compatible names (so existing CardInputProcessor keeps working)
    public IReadOnlyList<ResourceEffect> BaseEffects => baseResourceEffects;
    public IReadOnlyList<PhaseEffectBlock> PhaseEffects => phaseResourceEffects;
    public IReadOnlyList<OutcomeEffectBlock> OutcomeEffects => outcomeResourceEffects;

#if UNITY_EDITOR
    // Minimal setter so an importer can populate assets.
    public void SetData(
        string id,
        string name,
        int ap,
        int money,
        string desc,
        List<Keyword> keys,
        List<ResourceEffect> baseFx,
        List<PhaseEffectBlock> phaseFx,
        List<OutcomeEffectBlock> outcomeFx,
        string tileFxRaw)
    {
        cardId = id;
        cardName = name;

        apCost = Mathf.Max(0, ap);
        moneyCost = Mathf.Max(0, money);

        cardDescription = desc;
        keywords = keys ?? new List<Keyword>();

        baseResourceEffects = baseFx ?? new List<ResourceEffect>();
        phaseResourceEffects = phaseFx ?? new List<PhaseEffectBlock>();
        outcomeResourceEffects = outcomeFx ?? new List<OutcomeEffectBlock>();

        tileEffectsRaw = string.IsNullOrWhiteSpace(tileFxRaw) ? "None" : tileFxRaw.Trim();
    }
#endif
}

[Serializable]
public struct ResourceEffect
{
    public ResourceType resource;
    public EffectValueMode mode;

    [Tooltip("Used when mode == Fixed")]
    public int amount;

    [Tooltip("Used when mode == UseInput. Typically +1 or -1.")]
    public int multiplier;

    public int Resolve(int inputX)
    {
        return mode == EffectValueMode.Fixed ? amount : inputX * multiplier;
    }

    public static ResourceEffect Fixed(ResourceType r, int amount)
        => new ResourceEffect { resource = r, mode = EffectValueMode.Fixed, amount = amount, multiplier = 0 };

    public static ResourceEffect UseInput(ResourceType r, int multiplier)
        => new ResourceEffect { resource = r, mode = EffectValueMode.UseInput, amount = 0, multiplier = multiplier };
}

[Serializable]
public struct PhaseEffectBlock
{
    public Phase phase;
    public List<ResourceEffect> effects;
}

[Serializable]
public struct OutcomeEffectBlock
{
    [Tooltip("Outcome number: 1..6 (or coin: 1..2)")]
    public int outcome;
    public List<ResourceEffect> effects;
}
