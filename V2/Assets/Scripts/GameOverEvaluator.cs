using UnityEngine;

public class GameOverEvaluator : MonoBehaviour
{
    // =========================
    // LINK THESE IN INSPECTOR
    // =========================

    [Header("Required (you already have this asset)")]
    [SerializeField] private TownResourceList townResources;

    [Header("End Condition Inputs (NOT created yet in your project)")]
    [SerializeField] private int totalCulturalSites = 3;
    [SerializeField] private int destroyedCulturalSites = 0;

    [SerializeField] private int totalBuildingTiles = 0;
    [SerializeField] private int damagedBuildingTiles = 0;
    [SerializeField] private int destroyedBuildingTiles = 0;

    [Tooltip("Longest chain of linked fires currently on the board.")]
    [SerializeField] private int longestLinkedFireChain = 0;

    [Header("Grade Inputs (NOT created yet in your project)")]
    [SerializeField] private int totalCulturalActionCards = 0;
    [SerializeField] private int uniqueCulturalActionCardsPlayed = 0;

    [SerializeField] private int numCulturalCardsPlayedIndigenousLeader = 0; // clamp 0..6
    [SerializeField] private int numPlayersUsedCulturalCard = 0;
    [SerializeField] private int numPlayers = 1;

    [SerializeField] private int uniqueCollabActionCardsUsed = 0;
    [SerializeField] private int totalUniqueCollabActionCardsUsed = 0;
    [SerializeField] private int numPlayersUsedCollabCard = 0;

    [SerializeField] private int uniquePreparednessActionCardsUsed = 0;
    [SerializeField] private int totalUniquePreparednessActionCards = 0;

    [SerializeField] private int uniqueActionTypesUsed = 0;
    [SerializeField] private int totalActionTypes = 0;

    [SerializeField] private int totalAPUsed = 0;
    [SerializeField] private int totalTurns = 1;

    // =========================
    // OUTPUT
    // =========================

    private void OnEnable()
    {
        EvaluateAndLog();
    }

    public void EvaluateAndLog()
    {
        if (townResources == null)
        {
            Debug.LogError("[GAME OVER] TownResourceList reference is missing.");
            return;
        }

        // Keep derived rating up to date
        float fireRiskRating = townResources.CalculateFireSafety();

        // -------------------------
        // 1) GAME END CONDITIONS
        // -------------------------
        bool irrepairableCulturalDamage =
            totalCulturalSites > 0 && destroyedCulturalSites >= Mathf.CeilToInt(totalCulturalSites * (2f / 3f)); // 2/3 destroyed :contentReference[oaicite:3]{index=3}

        float buildingDamageRatio =
            totalBuildingTiles > 0 ? (damagedBuildingTiles + destroyedBuildingTiles) / (float)totalBuildingTiles : 0f;

        bool infrastructureCollapse =
            totalBuildingTiles > 0 && buildingDamageRatio >= 0.40f; // >=40% :contentReference[oaicite:4]{index=4}

        bool uncontrolledFire =
            longestLinkedFireChain >= 6; // chain of 6+ :contentReference[oaicite:5]{index=5}

        Debug.Log(
            "[GAME OVER] End Conditions\n" +
            $"- Irrepairable Cultural Damage: {irrepairableCulturalDamage} (Destroyed {destroyedCulturalSites}/{totalCulturalSites})\n" +
            $"- Infrastructure Collapse: {infrastructureCollapse} (Buildings damaged+destroyed {damagedBuildingTiles + destroyedBuildingTiles}/{totalBuildingTiles} = {(buildingDamageRatio * 100f):0.#}%)\n" +
            $"- Uncontrolled Fire: {uncontrolledFire} (Longest linked chain = {longestLinkedFireChain})"
        );

        // -------------------------
        // 2) GAME END GRADES
        // -------------------------

        // CulturalSiteIntegrity = 100 * (Undamaged / Total)
        int undamagedCulturalSites = Mathf.Max(0, totalCulturalSites - destroyedCulturalSites);
        float culturalSiteIntegrity = Percent(undamagedCulturalSites, totalCulturalSites);

        // A) Cultural safety, integrity and inclusion :contentReference[oaicite:6]{index=6}
        float culturalCardCoverage01 = Clamp01((totalCulturalActionCards > 0)
            ? (uniqueCulturalActionCardsPlayed / (float)totalCulturalActionCards) / 0.75f
            : 0f);
        float gradeCulturalSafety =
            0.6f * (culturalSiteIntegrity / 100f) +
            0.4f * culturalCardCoverage01;
        gradeCulturalSafety *= 100f;

        // B) Indigenous knowledge, experience and practice :contentReference[oaicite:7]{index=7}
        float indigenousLeader01 = Mathf.Clamp(numCulturalCardsPlayedIndigenousLeader, 0, 6) / 6f;
        float playersUsedCultural01 = Ratio(numPlayersUsedCulturalCard, numPlayers);
        float gradeIndigenousKnowledge =
            0.5f * indigenousLeader01 +
            0.3f * (culturalSiteIntegrity / 100f) +
            0.2f * playersUsedCultural01;
        gradeIndigenousKnowledge *= 100f;

        // CollaborativeScore definition :contentReference[oaicite:8]{index=8}
        float collabScore01 =
            0.5f * Ratio(numPlayersUsedCollabCard, numPlayers) +
            0.5f * Ratio(uniqueCollabActionCardsUsed, totalUniqueCollabActionCardsUsed);
        float collaborativeScore = collabScore01 * 100f;

        // C) Building and maintaining networks :contentReference[oaicite:9]{index=9} :contentReference[oaicite:10]{index=10}
        float gradeNetworks =
            (0.3f * Percent01(townResources.happiness, 50)) +
            (0.4f * (collaborativeScore / 100f)) +
            (0.3f * Percent01(townResources.education, 50));
        gradeNetworks *= 100f;

        // PreparednessScore :contentReference[oaicite:11]{index=11}
        float preparedness01 = Clamp01((totalUniquePreparednessActionCards > 0)
            ? (uniquePreparednessActionCardsUsed / (float)totalUniquePreparednessActionCards) / 0.75f
            : 0f);
        float preparednessScore = preparedness01 * 100f;

        // D) Community disaster resilience knowledge :contentReference[oaicite:12]{index=12}
        float gradeDisasterResilience =
            (0.3f * Percent01(townResources.education, 50)) +
            (0.3f * (fireRiskRating / 100f)) +
            (0.2f * Percent01(townResources.firefightingEquipment, 10)) +
            (0.2f * (preparednessScore / 100f));
        gradeDisasterResilience *= 100f;

        // AvgActivityScore :contentReference[oaicite:13]{index=13}
        // clamp( ((TotalAPUsed / TotalTurns * NumPlayers) - 0.40) / 0.40, 0, 1 )
        float avgActivity01 = Clamp01(((Ratio(totalAPUsed, totalTurns) * numPlayers) - 0.40f) / 0.40f);

        // E) Community led action :contentReference[oaicite:14]{index=14}
        float actionTypes01 = Ratio(uniqueActionTypesUsed, totalActionTypes);
        float gradeCommunityLedAction =
            (0.4f * avgActivity01) +
            (0.3f * Percent01(townResources.provisions, 50)) +
            (0.2f * actionTypes01) +
            (0.1f * Percent01(townResources.happiness, 50));
        gradeCommunityLedAction *= 100f;

        // F) Social innovation :contentReference[oaicite:15]{index=15}
        float gradeSocialInnovation =
            (0.4f * Percent01(townResources.education, 50)) +
            (0.4f * actionTypes01) +
            (0.2f * Percent01(townResources.provisions, 50));
        gradeSocialInnovation *= 100f;

        Debug.Log(
            "[GAME OVER] Grade Scores\n" +
            $"- Cultural safety, integrity and inclusion: {gradeCulturalSafety:0.#}\n" +
            $"- Indigenous knowledge, experience and practice: {gradeIndigenousKnowledge:0.#}\n" +
            $"- Building and maintaining networks: {gradeNetworks:0.#}\n" +
            $"- Community disaster resilience knowledge: {gradeDisasterResilience:0.#}\n" +
            $"- Community led action: {gradeCommunityLedAction:0.#}\n" +
            $"- Social innovation: {gradeSocialInnovation:0.#}"
        );

        // -------------------------
        // 3) FINAL SCORE + LETTER
        // -------------------------
        // Minimal aggregation (NOT specified in PDF): simple average of the 6 grade scores.
        float finalScore =
            (gradeCulturalSafety + gradeIndigenousKnowledge + gradeNetworks +
             gradeDisasterResilience + gradeCommunityLedAction + gradeSocialInnovation) / 6f;

        finalScore = Mathf.Clamp(finalScore, 0f, 100f);
        string letter = ToLetterGrade(finalScore); // thresholds :contentReference[oaicite:16]{index=16}

        Debug.Log($"[GAME OVER] Final\n- Score: {finalScore:0.#}/100\n- Grade: {letter}");
    }

    // =========================
    // Helpers (minimal)
    // =========================

    private static float Ratio(int num, int den)
    {
        if (den <= 0) return 0f;
        return Mathf.Clamp01(num / (float)den);
    }

    private static float Clamp01(float v) => Mathf.Clamp01(v);

    private static float Percent(int num, int den) => Ratio(num, den) * 100f;

    private static float Percent01(int value, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01(value / (float)max);
    }

    private static string ToLetterGrade(float score)
    {
        // 90-100 S, 75-89 A, 65-74 B, 55-64 C, 40-54 D, 25-39 E, 0-24 F :contentReference[oaicite:17]{index=17}
        if (score >= 90f) return "S";
        if (score >= 75f) return "A";
        if (score >= 65f) return "B";
        if (score >= 55f) return "C";
        if (score >= 40f) return "D";
        if (score >= 25f) return "E";
        return "F";
    }
}
