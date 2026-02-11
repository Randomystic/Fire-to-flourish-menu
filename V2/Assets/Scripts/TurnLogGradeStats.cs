using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TurnLogGradeStats : MonoBehaviour
{
    

    [Header("Links")]
    [SerializeField] private CardInputProcessor cardInputProcessor;

    [Tooltip("Must match CardInputProcessor.cardsFolderPath (Resources subfolder).")]

    [SerializeField] private string cardsFolderPath = "Data/Cards/Generated";

    [Header("Players (roles)")]
   
   
    [Tooltip("Civilian(A), Firefighter(B), Farmer(C), Indigenous Leader(D), Mayor(E), Teacher(F).")]
    [SerializeField] private int numPlayers = 6;



    // Computed values (ones GameOverEvaluator needs)
    public int TotalCulturalActionCards { get; private set; }
    public int uniqueCulturalActionCardsUsed { get; private set;}


    public int NumCulturalCardsPlayedIndigenousLeader { get; private set;} // clamp later
    public int NumPlayersUsedCulturalCard { get; private set; }

    public int UniqueCollabActionCardsUsed { get; private set; }
    public int TotalUniqueCollabActionCardsUsed { get; private set; }
    public int NumPlayersUsedCollabCard { get; private set; }


    public int UniquePreparednessActionCardsUsed { get; private set; }
    public int TotalUniquePreparednessActionCards { get; private set; }



    public int UniqueActionTypesUsed { get; private set; }
    
    public int TotalActionTypes { get; private set; } = 6; // Preparation, Bushfire, Action, Operation, Cultural, Outreach

    public int TotalAPUsed { get; private set; }
    public int TotalTurns { get; private set; } = 1;


    // Token grammar: first part is ID, rest are the optional parentheses
    private static readonly Regex IdRegex =
        new Regex(@"^(#?[A-Za-z]\d{2})", RegexOptions.Compiled);



    public int NumPlayers => numPlayers;


    public void Refresh()
    {


        if (cardInputProcessor == null)
        {
            Debug.LogError("[TurnLogGradeStats] Missing CardInputProcessor reference.");
            return;
        }



        // Load all cards once (needed for totals + keyword lookups + AP cost)
        CardActionData[] allCards = Resources.LoadAll<CardActionData>(cardsFolderPath);
        var cardsByIdSansHash = new Dictionary<string, CardActionData>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in allCards)
        {
            if (c == null) continue;

            string idSansHash = StripHash(c.CardId);

            if (!cardsByIdSansHash.ContainsKey(idSansHash))
                cardsByIdSansHash[idSansHash] = c;

        }

        // Totals across the full library (per your rules)
        TotalCulturalActionCards = CountCardsWithKeyword(allCards, Keyword.Cultural);
        TotalUniquePreparednessActionCards = CountCardsWithKeyword(allCards, Keyword.Preparation);
        
        TotalUniqueCollabActionCardsUsed = CountCardsWithKeyword(allCards, Keyword.Outreach);



        // Reset computed-from-log values


        uniqueCulturalActionCardsUsed = 0;
        NumCulturalCardsPlayedIndigenousLeader = 0;
        NumPlayersUsedCulturalCard = 0;

        UniqueCollabActionCardsUsed = 0;
        NumPlayersUsedCollabCard = 0;


        UniquePreparednessActionCardsUsed = 0;

        UniqueActionTypesUsed = 0;

        TotalAPUsed = 0;
        TotalTurns = 1;



        // Sets for “unique” calculations
        var uniqueCulturalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var uniquePrepIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var uniqueOutreachIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        var usedActionTypes = new HashSet<Keyword>(); // counts distinct keyword categories present in played cards

        // Distinct players who used a keyword (based on ID prefix A-F; X is generic and ignored)
        var playersUsedCultural = new HashSet<char>();

        var playersUsedOutreach = new HashSet<char>();



        IReadOnlyDictionary<int, List<string>> log = cardInputProcessor.GetTurnLog();
        TotalTurns = log.Count > 0 ? log.Count : 1;

        foreach (var kvp in log)
      
        {
          
          
            List<string> tokens = kvp.Value;
            if (tokens == null) continue;

            foreach (string rawToken in tokens)
            {

                string idSansHash = ExtractIdSansHash(rawToken);
             
                if (string.IsNullOrEmpty(idSansHash)) continue;

                if (!cardsByIdSansHash.TryGetValue(idSansHash, out CardActionData card))
                    continue;


                // AP used: sum AP cost per played card (duplicates count)
                TotalAPUsed += Mathf.Max(0, card.ApCost);

                // Track action type categories used (based on card keywords)
                var keys = card.Keywords;
                for (int i = 0; i < keys.Count; i++)
                    usedActionTypes.Add(keys[i]);



                // Cultural
                if (HasKeyword(card, Keyword.Cultural))
                {
                    uniqueCulturalIds.Add(idSansHash);


                    char role = RolePrefix(idSansHash);

                    if (role != '\0' && role != 'X')
                        playersUsedCultural.Add(role);


                    // Indigenous leader cultural cards (D-prefixed + cultural)
                    if (role == 'D')
                        NumCulturalCardsPlayedIndigenousLeader++;


                }



                // Preparation
                if (HasKeyword(card, Keyword.Preparation))
                    uniquePrepIds.Add(idSansHash);


                // Outreach (collab)
                if (HasKeyword(card, Keyword.Outreach))

                {
                    uniqueOutreachIds.Add(idSansHash);
                    char role = RolePrefix(idSansHash);


                    if (role != '\0' && role != 'X')
                        playersUsedOutreach.Add(role);
                }
            }


        }

        uniqueCulturalActionCardsUsed = uniqueCulturalIds.Count;
        UniquePreparednessActionCardsUsed = uniquePrepIds.Count;

        UniqueCollabActionCardsUsed = uniqueOutreachIds.Count;

        NumPlayersUsedCulturalCard = playersUsedCultural.Count;
        NumPlayersUsedCollabCard = playersUsedOutreach.Count;

        UniqueActionTypesUsed = usedActionTypes.Count;



    }


    // Helpers

    private static int CountCardsWithKeyword(CardActionData[] cards, Keyword k)
    {
        int count = 0;

        if (cards == null) return 0;

        for (int i = 0; i < cards.Length; i++)
        {

            var c = cards[i];

            if (c != null && HasKeyword(c, k))
                count++;
        }
        return count;
        
    }

    private static bool HasKeyword(CardActionData card, Keyword k)
    {
        var keys = card.Keywords;

        for (int i = 0; i < keys.Count; i++)
        {
           
            if (keys[i] == k) return true;
        }


        return false;
    }




    private static string ExtractIdSansHash(string token)
    {

        if (string.IsNullOrWhiteSpace(token)) return "";
        token = token.Trim();

        var m = IdRegex.Match(token);
        if (!m.Success) return "";

        return StripHash(m.Groups[1].Value);
    }

    private static string StripHash(string id)
    {

        if (string.IsNullOrEmpty(id)) return "";

        return id.StartsWith("#", StringComparison.Ordinal) ? id.Substring(1) : id;
    }


    private static char RolePrefix(string idSansHash)
    {
        if (string.IsNullOrEmpty(idSansHash)) return '\0';
        char c = char.ToUpperInvariant(idSansHash[0]);

        // A,B,C,D,E,F,X
        return c;
    }
}
