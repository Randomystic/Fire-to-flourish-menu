#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CardCsvImporter
{
    // Change this if you want a different output location
    private const string OutputFolder = "Assets/Resources/Cards";

    [MenuItem("Tools/Cards/Import Cards From CSV")]
    public static void ImportCardsFromCsv()
    {
        // Pick file
        string csvPath = EditorUtility.OpenFilePanel("Select cards CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(csvPath))
            return;

        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV not found: {csvPath}");
            return;
        }

        Directory.CreateDirectory(OutputFolder);

        string csvText = File.ReadAllText(csvPath);
        List<string[]> rows = ParseCsv(csvText);
        if (rows.Count < 2)
        {
            Debug.LogError("CSV contains no data rows.");
            return;
        }

        // Header
        string[] header = rows[0];
        int colId = FindCol(header, "ID");
        int colName = FindCol(header, "Name");
        int colDesc = FindCol(header, "Description");
        int colKeywords = FindCol(header, "Keywords");
        int colEffects = FindCol(header, "Effects");

        if (colId < 0 || colName < 0)
        {
            Debug.LogError("CSV must contain columns: ID, Name (and preferably Description, Keywords, Effects).");
            return;
        }

        int created = 0;
        int updated = 0;

        for (int i = 1; i < rows.Count; i++)
        {
            string[] r = rows[i];
            string id = Get(r, colId).Trim();
            if (string.IsNullOrWhiteSpace(id))
                continue;

            string name = Get(r, colName).Trim();
            string desc = Get(r, colDesc);
            string keywordsText = Get(r, colKeywords);
            string effectsText = Get(r, colEffects);

            List<Keyword> keywords = ParseKeywords(keywordsText);

            // Parse effects into base/phase/outcome blocks
            List<ResourceEffect> baseEffects;
            List<PhaseEffectBlock> phaseEffects;
            List<OutcomeEffectBlock> outcomeEffects;
            ParseEffects(effectsText, out baseEffects, out phaseEffects, out outcomeEffects);

            // Stable filename based on ID only (so renaming card name doesn't create a new asset)
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
            else
            {
                updated++;
            }

            // Requires CardActionData.SetData(...) with base/phase/outcome fields (the version we discussed)
            asset.SetData(
                id,
                name,
                desc,
                keywords,
                baseEffects,
                phaseEffects,
                outcomeEffects
            );

            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Import complete. Created: {created}, Updated: {updated}. Output: {OutputFolder}");
    }

    // -----------------------------
    // Keyword parsing
    // -----------------------------
    private static List<Keyword> ParseKeywords(string text)
    {
        var list = new List<Keyword>();
        if (string.IsNullOrWhiteSpace(text))
            return list;

        string[] parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string p in parts)
        {
            string token = p.Trim();
            if (token.Length == 0) continue;

            if (Enum.TryParse(token, true, out Keyword k))
                list.Add(k);
            else
                Debug.LogWarning($"Unknown Keyword '{token}'");
        }
        return list;
    }

    // -----------------------------
    // Effects parsing
    // Supports:
    // - None
    // - Resource:+2;Resource:-1
    // - Resource:+X / Resource:-X
    // - (P) ... | (B) ...
    // - (1) ... | (2) ...
    // -----------------------------
    private static void ParseEffects(
        string effectsText,
        out List<ResourceEffect> baseEffects,
        out List<PhaseEffectBlock> phaseEffects,
        out List<OutcomeEffectBlock> outcomeEffects)
    {
        baseEffects = new List<ResourceEffect>();
        phaseEffects = new List<PhaseEffectBlock>();
        outcomeEffects = new List<OutcomeEffectBlock>();

        if (string.IsNullOrWhiteSpace(effectsText))
            return;

        string trimmed = effectsText.Trim();

        // Treat explicit "None" as no effects
        if (string.Equals(trimmed, "None", StringComparison.OrdinalIgnoreCase))
            return;

        bool looksLikePhase = trimmed.Contains("(P)") || trimmed.Contains("(B)");
        bool looksLikeOutcome = Regex.IsMatch(trimmed, @"\(\d+\)");

        if (looksLikePhase)
        {
            // Example: (P) Happiness:+2 | (B) Happiness:+4
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((P|B)\)\s*(.*)$");
                if (!m.Success) continue;

                Phase phase = m.Groups[1].Value == "P" ? Phase.Preparation : Phase.Bushfire;
                string payload = m.Groups[2].Value.Trim();

                var fxList = ParseEffectList(payload);
                phaseEffects.Add(new PhaseEffectBlock { phase = phase, effects = fxList });
            }
            return;
        }

        if (looksLikeOutcome)
        {
            // Example: (1) Education:+4 | (2) Happiness:-4
            foreach (string branch in trimmed.Split('|'))
            {
                string b = branch.Trim();
                var m = Regex.Match(b, @"^\((\d+)\)\s*(.*)$");
                if (!m.Success) continue;

                int outcome = int.Parse(m.Groups[1].Value);
                string payload = m.Groups[2].Value.Trim();

                var fxList = ParseEffectList(payload);
                outcomeEffects.Add(new OutcomeEffectBlock { outcome = outcome, effects = fxList });
            }
            return;
        }

        // Otherwise: base effects list
        baseEffects = ParseEffectList(trimmed);
    }

    private static List<ResourceEffect> ParseEffectList(string payload)
    {
        var list = new List<ResourceEffect>();

        if (string.IsNullOrWhiteSpace(payload))
            return list;

        if (string.Equals(payload.Trim(), "None", StringComparison.OrdinalIgnoreCase))
            return list;

        string[] parts = payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string p in parts)
        {
            string token = p.Trim();
            if (token.Length == 0) continue;

            // Format: Resource:+2 / Resource:-X / Resource:0
            string[] kv = token.Split(':');
            if (kv.Length != 2)
            {
                Debug.LogWarning($"Bad effect token '{token}' (expected Resource:Value)");
                continue;
            }

            string resName = kv[0].Trim();
            string valueText = kv[1].Trim();

            if (!Enum.TryParse(resName, true, out ResourceType resource))
            {
                Debug.LogWarning($"Unknown ResourceType '{resName}' in '{token}'");
                continue;
            }

            // Variable X
            if (valueText.Contains("X", StringComparison.OrdinalIgnoreCase))
            {
                // Accept: +X, -X, X
                int multiplier = 1;
                if (valueText.StartsWith("-", StringComparison.Ordinal)) multiplier = -1;

                list.Add(ResourceEffect.UseInput(resource, multiplier));
                continue;
            }

            // Fixed int (accept +3, -2, 0)
            if (!int.TryParse(valueText, out int amount))
            {
                Debug.LogWarning($"Bad numeric value '{valueText}' in '{token}'");
                continue;
            }

            list.Add(ResourceEffect.Fixed(resource, amount));
        }

        return list;
    }

    // -----------------------------
    // CSV parser (supports quoted commas + quoted newlines)
    // -----------------------------
    private static List<string[]> ParseCsv(string text)
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var field = "";
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                // Escaped quote
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
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

                if (currentRow.Count > 0)
                    rows.Add(currentRow.ToArray());

                currentRow.Clear();
            }
            else
            {
                field += c;
            }
        }

        if (field.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(field);
            rows.Add(currentRow.ToArray());
        }

        // Remove completely empty trailing rows
        rows.RemoveAll(r => r.Length == 1 && string.IsNullOrWhiteSpace(r[0]));
        return rows;
    }

    private static int FindCol(string[] header, string name)
    {
        for (int i = 0; i < header.Length; i++)
        {
            if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string Get(string[] row, int col)
    {
        if (col < 0 || row == null || col >= row.Length) return "";
        return row[col] ?? "";
    }

    private static string MakeSafeFileName(string name)
    {
        // Keep it stable per card id, remove characters that can break filenames.
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        // Replace spaces and '#'
        name = name.Replace(' ', '_').Replace("#", "");
        return name;
    }
}
#endif
