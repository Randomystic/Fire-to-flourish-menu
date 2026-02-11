using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CardInputProcessor : MonoBehaviour
{
    [Header("Resources paths (inside Assets/Resources/)")]
    [SerializeField] private string townResourcesPath = "TownResources"; // TownResources.asset
    [SerializeField] private string cardsFolderPath = "Cards"; // folder containing card assets
    
    private readonly Dictionary<int, List<string>> turnLog = new();

    public IReadOnlyDictionary<int, List<string>> GetTurnLog() => turnLog;


    private int currentTurn = 1;

    // Grammar: #ID optionally followed by up to 3 parentheses groups.
    // Examples:
    // #X01
    // #A10(P)
    // #A06(3)
    // #F10(B)(3)
    // #F08(P)(1)


    private static readonly Regex TokenRegex =
        new Regex(@"^(#?[A-Za-z]\d{2})(\([^)]+\))*$",
            RegexOptions.Compiled);

    // Validate + apply a whole input line for the current turn.
    // Returns true if applied; false if rejected.
    public bool SubmitTurn(string inputLine)
    {
        var errors = new List<string>();
        var parsedPlays = new List<CardPlay>();

        if (string.IsNullOrWhiteSpace(inputLine))
        {
            Debug.LogWarning("Input line is empty.");
            return false;
        }

        // Split by commas
        string[] rawTokens = inputLine.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (rawTokens.Length == 0)
        {
            Debug.LogWarning("No tokens found.");
            return false;
        }

        // Load TownResources
        TownResourceList town = Resources.Load<TownResourceList>(townResourcesPath);
        if (town == null)
        {
            Debug.LogError($"Could not load TownResourceList at Resources/{townResourcesPath}.asset");
            return false;
        }

        // Parse and validate all the tokens first
        for (int i = 0; i < rawTokens.Length; i++)
        {
            string token = rawTokens[i].Trim();
            if (token.Length == 0) continue;

            if (!TryParseToken(token, out CardPlay play, out string tokenError))
            {
                errors.Add($"Token {i + 1} '{token}': {tokenError}");
                continue;
            }

            // Load card asset by ID (we assume asset name equals ID without '#', as importer saved)

            // Example: #F10 -> Resources/Data/Cards/Generated/F10
            string cardResourcePath = $"{cardsFolderPath}/{play.CardIdSansHash}";
            CardActionData card = Resources.Load<CardActionData>(cardResourcePath);

            if (card == null)
            {
                errors.Add($"Token {i + 1} '{token}': Card asset not found at Resources/{cardResourcePath}.asset");
                continue;
            }

            // Validate that required selectors are present if the card expects them
            if (!ValidateSelectorsAgainstCard(card, play, out string selectorError))
            {
                errors.Add($"Token {i + 1} '{token}': {selectorError}");
                continue;
            }

            play.Card = card;
            parsedPlays.Add(play);
        }

        // If any errors, reject whole line
        if (errors.Count > 0)
        {
            Debug.LogError("Turn input rejected. Fix these errors:\n" + string.Join("\n", errors));
            return false;
        }

        // get old values for logging/debugging if needed before applying effects
        town.GetOldValuesBeforeTurn();

        // Apply all effects now that line is valid
        foreach (var play in parsedPlays)
            ApplyCardPlay(town, play);

        // Recalculate derived rating if you want it always current
        town.CalculateFireSafety();

        // Save turn log
        if (!turnLog.ContainsKey(currentTurn))
            turnLog[currentTurn] = new List<string>();

        foreach (var raw in rawTokens)
            turnLog[currentTurn].Add(raw.Trim());

        Debug.Log($"Turn {currentTurn} accepted. Applied {parsedPlays.Count} cards.");
        currentTurn++;

        Debug.Log("Current Town Resources after turn:\n" + town.GetResourceSummary());

        return true;
    }


    // Parsing
    private static bool TryParseToken(string token, out CardPlay play, out string error)
    {
        play = default;
        error = "";

        token = token.Replace(" ", "");

        // Basic shape check
        if (!TokenRegex.IsMatch(token))
        {
            error = "Bad syntax. Expected like #A10(P), #A06(3), #F10(B)(3), #F08(P)(1).";
            return false;
        }

        // Extract ID and parentheses groups
        // ID is first 4 chars like #F10 or F10
        string idPart = token.StartsWith("#") ? token.Substring(0, 4) : "#" + token.Substring(0, 3);
        string idSansHash = idPart.Replace("#", "");

        var groups = new List<string>();
        foreach (Match m in Regex.Matches(token, @"\(([^)]+)\)"))
            groups.Add(m.Groups[1].Value);


        // example: #ID(P)(1)(3)
        // Phase is a group equal to P or B (case-insensitive)
        // Outcome is a group that is an int 1..6 (if card uses outcomes)
        // X is a group that is an int (used for +X/-X effects)


        // If multiple ints exist, last int becomes X unless card expects outcome without X.
        Phase? phase = null;
        int? outcome = null;
        int? xValue = null;

        foreach (var g in groups)
        {
            if (string.Equals(g, "P", StringComparison.OrdinalIgnoreCase))
            {
                phase = Phase.Preparation;
                continue;
            }
            if (string.Equals(g, "B", StringComparison.OrdinalIgnoreCase))
            {
                phase = Phase.Bushfire;
                continue;
            }

            // Numeric group
            if (int.TryParse(g, out int n))
            {
                // Temporarily store. will resolve final assignment after we know card requirements.
                // - first numeric becomes outcome
                // - second numeric becomes X
                if (outcome == null) outcome = n;
                else xValue = n;

                continue;
            }

            error = $"Unknown parentheses group '{g}'. Only (P), (B), and numeric (1..6)/(X) are supported.";
            return false;
        }

        play = new CardPlay
        {
            RawToken = token,
            CardIdWithHash = idPart,
            CardIdSansHash = idSansHash,
            Phase = phase,
            Outcome = outcome,
            XValue = xValue
        };

        return true;
    }

    private static bool ValidateSelectorsAgainstCard(CardActionData card, CardPlay play, out string error)
    {
        error = "";

        // If card has phase effects, require phase
        bool cardUsesPhase = card.PhaseEffects != null && card.PhaseEffects.Count > 0;
        if (cardUsesPhase && play.Phase == null)
        {
            error = "Card requires a phase selector: add (P) or (B).";
            return false;
        }

        // If card has outcome effects, require outcome number
        bool cardUsesOutcome = card.OutcomeEffects != null && card.OutcomeEffects.Count > 0;
        if (cardUsesOutcome && play.Outcome == null)
        {
            error = "Card requires an outcome selector: add (1) or (2) (or 1..6).";
            return false;
        }

        // If any effect uses input X (base or selected phase/outcome), require XValue.
        // We do a minimal check by scanning the base effects, selected phase effects, and selected outcome effects
        
        bool needsX = CardNeedsX(card, play);
        if (needsX && play.GetResolvedX() == null)
        {
            error = "Card requires an X value: add a numeric (e.g. (3)).";
            return false;
        }

        // Validate outcome range if present (you said outcomes 1..6)
        if (play.Outcome is int o && (o < 1 || o > 6))
        {
            error = "Outcome must be between 1 and 6.";
            return false;
        }

        // Validate X range if present (dice roll usually 1..6, but you may want larger allowed)
        if (play.GetResolvedX() is int x && x < 0)
        {
            error = "X value cannot be negative.";
            return false;
        }


        return true;
    }



    private static bool CardNeedsX(CardActionData card, CardPlay play)
    {
        // Base effects
        if (ListNeedsX(card.BaseEffects))

            return true;


        // Phase effects
        if (play.Phase != null)
        {

            foreach (var pb in card.PhaseEffects)
            {

                if (pb.phase == play.Phase.Value && ListNeedsX(pb.effects))
                    return true;
            }
        }

        // Outcome effects
        if (play.Outcome != null)
        {
            foreach (var ob in card.OutcomeEffects)
            {
       
                if (ob.outcome == play.Outcome.Value && ListNeedsX(ob.effects))
                    return true;
            }
        }


        return false;


    }



    private static bool ListNeedsX(IReadOnlyList<ResourceEffect> list)
    {
        if (list == null) return false;
        for (int i = 0; i < list.Count; i++)
        {

            if (list[i].mode == EffectValueMode.UseInput)
                return true;
        }
        return false;
    }


    // Applying effects
    private static void ApplyCardPlay(TownResourceList town, CardPlay play)
    {
        // Apply base effects

        ApplyEffectList(town, play.Card.BaseEffects, play.GetResolvedX() ?? 0);

        // Apply phase effects (if specified)
        if (play.Phase != null)
        {

            foreach (var pb in play.Card.PhaseEffects)
            {

                if (pb.phase == play.Phase.Value)
                    ApplyEffectList(town, pb.effects, play.GetResolvedX() ?? 0);
            }

        }

        // Apply outcome effects (if specified)
        if (play.Outcome != null)
        {
          
            foreach (var ob in play.Card.OutcomeEffects)
            {
                if (ob.outcome == play.Outcome.Value)
                    ApplyEffectList(town, ob.effects, play.GetResolvedX() ?? 0);
            }

        }

    }

    private static void ApplyEffectList(TownResourceList town, IReadOnlyList<ResourceEffect> effects, int xValue)
    {

        if (effects == null) return;

        for (int i = 0; i < effects.Count; i++)
        {
            ResourceEffect e = effects[i];
            int delta = e.Resolve(xValue);

            switch (e.resource)
            {
                case ResourceType.Provision: town.AdjustProvisions(delta); break;
                case ResourceType.Education: town.AdjustEducation(delta); break;
                case ResourceType.Happiness: town.AdjustHappiness(delta); break;

                case ResourceType.FirefightingEquipment: town.AdjustFireFightingEquipment(delta); break;

                case ResourceType.WindSpeed: town.AdjustWindSpeed(delta); break;
                case ResourceType.Temperature: town.AdjustTemperatureSeason(delta); break;
                case ResourceType.FireSafetyRating: town.AdjustFireSafetyRating(delta); break;

                default:
                    Debug.LogWarning($"Unhandled resource type: {e.resource}");
                    break;

            }

        }

    }

    // // Optional: expose history
    // public string GetTurnLogAsText()
    // {
    //     // Example output:
    //     // 1: #X01, #F10, #C03
    //     // 2: #B01, #X04, #X04, #E04
    //     var lines = new List<string>();
    //     foreach (var kvp in turnLog)
    //     {
    //         lines.Add($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
    //     }
    //     return string.Join("\n", lines);
    // }

    // Data struct for a parsed token
    private struct CardPlay
    {
        public string RawToken;
        public string CardIdWithHash;   // "#F10"
        public string CardIdSansHash;   // "F10"

        public Phase? Phase;
        public int? Outcome;

        // If token has only one numeric group, we store it in Outcome first.
        // If it has two, second becomes XValue.
        public int? XValue;

        public CardActionData Card;

        public int? GetResolvedX()
        {
            // If XValue exists, it's X.
            if (XValue != null) return XValue;

            // If only one numeric group exists, it might be X for +X cards OR outcome for outcome cards.
            // We resolve as:
            // - If card has outcomeEffects, treat Outcome as outcome, otherwise treat Outcome as X.
            if (Outcome == null) return null;

            bool cardUsesOutcome = Card != null && Card.OutcomeEffects != null && Card.OutcomeEffects.Count > 0;

            return cardUsesOutcome ? null : Outcome;
            
        }
    }
}
