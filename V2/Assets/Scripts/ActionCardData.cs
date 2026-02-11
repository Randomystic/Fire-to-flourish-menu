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

    [Header("Effects")]
    [Tooltip("Always applied when the card is logged as played.")]
    [SerializeField] private List<ResourceEffect> baseEffects = new();

    [Tooltip("Applied only when a phase is supplied in input: (P) or (B).")]
    [SerializeField] private List<PhaseEffectBlock> phaseEffects = new();

    [Tooltip("Applied only when an outcome is supplied in input: (1..6) or coin (1|2).")]
    [SerializeField] private List<OutcomeEffectBlock> outcomeEffects = new();

    public string CardId => cardId;
    public string CardName => cardName;
    public int ApCost => apCost;
    public int MoneyCost => moneyCost;
    public string CardDescription => cardDescription;

    public IReadOnlyList<Keyword> Keywords => keywords;
    public IReadOnlyList<ResourceEffect> BaseEffects => baseEffects;
    public IReadOnlyList<PhaseEffectBlock> PhaseEffects => phaseEffects;
    public IReadOnlyList<OutcomeEffectBlock> OutcomeEffects => outcomeEffects;

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
        List<OutcomeEffectBlock> outcomeFx)
    {
        cardId = id;
        cardName = name;

        apCost = Mathf.Max(0, ap);
        moneyCost = Mathf.Max(0, money);

        cardDescription = desc;
        keywords = keys ?? new List<Keyword>();
        baseEffects = baseFx ?? new List<ResourceEffect>();
        phaseEffects = phaseFx ?? new List<PhaseEffectBlock>();
        outcomeEffects = outcomeFx ?? new List<OutcomeEffectBlock>();
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
