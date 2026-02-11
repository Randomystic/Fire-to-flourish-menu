#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CardCsvImporter
{
    private const string OutputFolder = "Assets/Resources/Cards";

    [MenuItem("Tools/Cards/Import Cards From CSV")]
    public static void ImportCardsFromCsv()
    {
        string csvPath = EditorUtility.OpenFilePanel("Select cards CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(csvPath)) return;

        Directory.CreateDirectory(OutputFolder);

        string csvText = File.ReadAllText(csvPath);
        List<string[]> rows = ParseCsv(csvText);
        if (rows.Count < 2)
        {
            Debug.LogError("CSV contains no data rows.");
            return;
        }

        string[] header = rows[0];

        int colId = FindCol(header, "ID");
        int colName = FindCol(header, "Name");
        int colCost = FindCol(header, "Cost");
        int colDesc = FindCol(header, "Description");
        int colKeywords = FindCol(header, "Keywords");

        int colResFx = FindCol(header, "ResourceEffects");
        if (colResFx < 0) colResFx = FindCol(header, "Effects");

        int colTileFx = FindCol(header, "TileEffects");

        if (colId < 0 || colName < 0)
        {
            Debug.LogError("CSV must contain columns: ID, Name (and preferably Cost, Description, Keywords, Effects, TileEffects).");
            return;
        }

        int created = 0, updated = 0;

        for (int i = 1; i < rows.Count; i++)
        {
            string[] r = rows[i];
            string id = Get(r, colId).Trim();
            if (string.IsNullOrWhiteSpace(id)) continue;

            string name = Get(r, colName).Trim();
            string costText = Get(r, colCost);
            string desc = Get(r, colDesc);
            string keywordsText = Get(r, colKeywords);
            string resFxText = Get(r, colResFx);
            string tileFxText = Get(r, colTileFx);

            ParseCost(costText, out int apCost, out int moneyCost);
            List<Keyword> keywords = ParseKeywords(keywordsText);

            // Resource effects
            ParseResourceEffects(resFxText, out var baseRes, out var phaseRes, out var outcomeRes);

            // Tile effects
            ParseTileEffects(tileFxText, out var baseTile, out var phaseTile, out var outcomeTile);

            string fileName = MakeSafeFileName(id) + ".asset";
            string assetPath = $"{OutputFolder}/{fileName}";

            CardActionData asset = AssetDatabase.LoadAssetAtPath<CardActionData>(assetPath);
            bool isNew = asset == null;

            if (isNew)
            {
                asset = ScriptableObject.CreateInstance<CardActionData>();
                AssetDatabase.CreateAsset(asset, assetPath);
                created++;
            }
            else updated++;

            asset.SetData(
                id, name, apCost, moneyCost, desc, keywords,
                baseRes, phaseRes, outcomeRes,
                baseTile, phaseTile, outcomeTile
            );

            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Import complete. Created: {created}, Updated: {updated}. Output: {OutputFolder}");
    }

    // ---------------- COST ----------------
    private static void ParseCost(string costText, out int ap, out int money)
    {
        ap = 0; money = 0;
        if (string.IsNullOrWhiteSpace(costText)) return;

        var apMatch = Regex.Match(costText, @"(\d+)\s*AP", RegexOptions.IgnoreCase);
        if (apMatch.Success && int.TryParse(apMatch.Groups[1].Value, out int apVal)) ap = Mathf.Max(0, apVal);

        var moneyMatch = Regex.Match(costText, @"(\d+)\s*Money", RegexOptions.IgnoreCase);
        if (moneyMatch.Success && int.TryParse(moneyMatch.Groups[1].Value, out int moneyVal)) money = Mathf.Max(0, moneyVal);
    }

    // ---------------- KEYWORDS ----------------
    private static List<Keyword> ParseKeywords(string text)
    {
        var list = new List<Keyword>();
        if (string.IsNullOrWhiteSpace(text)) return list;

        foreach (string p in text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string token = p.Trim();
            if (token.Length == 0) continue;

            if (Enum.TryParse(token, true, out Keyword k)) list.Add(k);
            else Debug.LogWarning($"Unknown Keyword '{token}'");
        }
        return list;
    }

    // ---------------- RESOURCE EFFECTS ----------------
    private static void ParseResourceEffects(
        string effectsText,
        out List<ResourceEffect> baseEffects,
        out List<PhaseResourceEffects> phaseEffects,
        out List<OutcomeResourceEffects> outcomeEffects)
    {
        baseEffects = new List<ResourceEffect>();
        phaseEffects = new List<PhaseResourceEffects>();
        outcomeEffects = new List<OutcomeResourceEffects>();

        if (string.IsNullOrWhiteSpace(effectsText)) return;
        string trimmed = effectsText.Trim();
        if (trimmed.Equals("None", StringComparison.OrdinalIgnoreCase)) return;

        bool looksLikePhase = trimmed.IndexOf("(P)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              trimmed.IndexOf("(B)", StringComparison.OrdinalIgnoreCase) >= 0;

        bool looksLikeOutcome = Regex.IsMatch(trimmed, @"\(\d+\)");

        if (looksLikePhase)
        {
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((P|B)\)\s*(.*)$", RegexOptions.IgnoreCase);
                if (!m.Success) continue;

                Phase phase = m.Groups[1].Value.Equals("P", StringComparison.OrdinalIgnoreCase)
                    ? Phase.Preparation : Phase.Bushfire;

                var fx = ParseResourceEffectList(m.Groups[2].Value.Trim());
                phaseEffects.Add(new PhaseResourceEffects { phase = phase, effects = fx });
            }
            return;
        }

        // If outcome formatting is used for resources (rare now), parse it
        if (looksLikeOutcome && trimmed.StartsWith("("))
        {
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((\d+)\)\s*(.*)$");
                if (!m.Success) continue;

                int outcome = int.Parse(m.Groups[1].Value);
                var fx = ParseResourceEffectList(m.Groups[2].Value.Trim());
                outcomeEffects.Add(new OutcomeResourceEffects { outcome = outcome, effects = fx });
            }
            return;
        }

        baseEffects = ParseResourceEffectList(trimmed);
    }

    private static List<ResourceEffect> ParseResourceEffectList(string payload)
    {
        var list = new List<ResourceEffect>();
        if (string.IsNullOrWhiteSpace(payload)) return list;
        if (payload.Equals("None", StringComparison.OrdinalIgnoreCase)) return list;

        foreach (string part in payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string token = part.Trim();
            if (token.Length == 0) continue;

            string[] kv = token.Split(':');
            if (kv.Length != 2) { Debug.LogWarning($"Bad resource effect '{token}'"); continue; }

            if (!Enum.TryParse(kv[0].Trim(), true, out ResourceType resource))
            { Debug.LogWarning($"Unknown ResourceType '{kv[0]}'"); continue; }

            string valueText = kv[1].Trim();

            if (valueText.IndexOf("X", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int mul = valueText.StartsWith("-", StringComparison.Ordinal) ? -1 : 1;
                list.Add(ResourceEffect.UseInput(resource, mul));
                continue;
            }

            if (!int.TryParse(valueText, out int amount))
            { Debug.LogWarning($"Bad numeric value '{valueText}'"); continue; }

            list.Add(ResourceEffect.Fixed(resource, amount));
        }

        return list;
    }

    // ---------------- TILE EFFECTS ----------------
    private static void ParseTileEffects(
        string tileText,
        out List<TileEffect> baseEffects,
        out List<PhaseTileEffects> phaseEffects,
        out List<OutcomeTileEffects> outcomeEffects)
    {
        baseEffects = new List<TileEffect>();
        phaseEffects = new List<PhaseTileEffects>();
        outcomeEffects = new List<OutcomeTileEffects>();

        if (string.IsNullOrWhiteSpace(tileText)) return;

        string trimmed = tileText.Trim();
        if (trimmed.Equals("None", StringComparison.OrdinalIgnoreCase)) return;

        bool looksLikePhase = trimmed.IndexOf("(P)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              trimmed.IndexOf("(B)", StringComparison.OrdinalIgnoreCase) >= 0;

        bool looksLikeOutcome = Regex.IsMatch(trimmed, @"^\(\d+\)");

        if (looksLikePhase)
        {
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((P|B)\)\s*(.*)$", RegexOptions.IgnoreCase);
                if (!m.Success) continue;

                Phase phase = m.Groups[1].Value.Equals("P", StringComparison.OrdinalIgnoreCase)
                    ? Phase.Preparation : Phase.Bushfire;

                var fx = ParseTileEffectList(m.Groups[2].Value.Trim());
                phaseEffects.Add(new PhaseTileEffects { phase = phase, effects = fx });
            }
            return;
        }

        if (looksLikeOutcome)
        {
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((\d+)\)\s*(.*)$");
                if (!m.Success) continue;

                int outcome = int.Parse(m.Groups[1].Value);
                var fx = ParseTileEffectList(m.Groups[2].Value.Trim());
                outcomeEffects.Add(new OutcomeTileEffects { outcome = outcome, effects = fx });
            }
            return;
        }

        baseEffects = ParseTileEffectList(trimmed);
    }

    // Parses 1+ tile effects separated by ';'
    // Example:
    // FireReduce(-2,0) (T=1) (D=0); FuelDelta(-4) (T=2) (D=0)
    private static List<TileEffect> ParseTileEffectList(string payload)
    {
        var list = new List<TileEffect>();
        if (string.IsNullOrWhiteSpace(payload)) return list;
        if (payload.Equals("None", StringComparison.OrdinalIgnoreCase)) return list;

        foreach (string part in payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string token = part.Trim();
            if (token.Length == 0) continue;

            if (TryParseSingleTileEffect(token, out TileEffect fx))
                list.Add(fx);
            else
                Debug.LogWarning($"Bad TileEffect token '{token}'");
        }

        return list;
    }

    private static bool TryParseSingleTileEffect(string token, out TileEffect fx)
    {
        fx = new TileEffect
        {
            type = TileEffectType.None,
            target = TileTarget.Tile,
            args = new List<int> { 0, 0, 0, 0 } // v,s,t,d
        };

        // Extract effect name and optional (a,b)
        // Matches: Name(...) or Name
        var m = Regex.Match(token, @"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*(\(([^)]*)\))?\s*(.*)$");
        if (!m.Success) return false;

        string name = m.Groups[1].Value.Trim();
        string argText = m.Groups[3].Value.Trim(); // inside (...)
        string suffix = m.Groups[4].Value;         // remaining " (T=...) (D=...)"

        if (!Enum.TryParse(name, true, out TileEffectType type))
            return false;

        fx.type = type;

        // Parse the main args for certain effect types:
        // FuelDelta(v) -> v
        // FireReduce(v,s) -> v,s
        // IgniteMod(p) / SpreadMod(p) -> v=p
        // BuildingDevelop(n) -> v=n
        if (!string.IsNullOrEmpty(argText))
        {
            string[] parts = argText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (type == TileEffectType.FireReduce)
            {
                // Expect v,s
                if (parts.Length >= 1) fx.args[0] = ParseInt(parts[0].Trim(), 0);
                if (parts.Length >= 2) fx.args[1] = ParseInt(parts[1].Trim(), 0);
            }
            else
            {
                // Single-int forms
                if (parts.Length >= 1) fx.args[0] = ParseInt(parts[0].Trim(), 0);
            }
        }

        // Parse suffix groups: (T=...), (D=...)
        // T can be int, X, or a type token (Global/Farmland/Tile/etc)
        foreach (Match sm in Regex.Matches(suffix, @"\(([^=]+)=([^)]+)\)"))
        {
            string key = sm.Groups[1].Value.Trim();
            string val = sm.Groups[2].Value.Trim();

            if (key.Equals("D", StringComparison.OrdinalIgnoreCase))
            {
                fx.args[3] = ParseInt(val, 0);
            }
            else if (key.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                // If T is numeric or X -> tiles count
                if (val.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    fx.args[2] = -1; // special meaning: use input X
                }
                else if (int.TryParse(val, out int tiles))
                {
                    fx.args[2] = tiles;
                }
                else
                {
                    // Otherwise treat T as TileTarget enum for mods
                    if (Enum.TryParse(val, true, out TileTarget target))
                        fx.target = target;
                    else
                        fx.target = TileTarget.Tile;
                }
            }
        }

        return fx.type != TileEffectType.None;
    }

    private static int ParseInt(string s, int fallback)
    {
        if (int.TryParse(s, out int v)) return v;
        return fallback;
    }

    // ---------------- CSV PARSING HELPERS ----------------
    private static List<string[]> ParseCsv(string text)
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        string field = "";
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field += '"';
                    i++;
                }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                currentRow.Add(field);
                field = "";
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;

                currentRow.Add(field);
                field = "";
                if (currentRow.Count > 0) rows.Add(currentRow.ToArray());
                currentRow.Clear();
            }
            else field += c;
        }

        if (field.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(field);
            rows.Add(currentRow.ToArray());
        }

        rows.RemoveAll(r => r.Length == 1 && string.IsNullOrWhiteSpace(r[0]));
        return rows;
    }

    private static int FindCol(string[] header, string name)
    {
        for (int i = 0; i < header.Length; i++)
            if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static string Get(string[] row, int col)
    {
        if (col < 0 || row == null || col >= row.Length) return "";
        return row[col] ?? "";
    }

    private static string MakeSafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name.Replace(' ', '_').Replace("#", "");
    }
}
#endif
