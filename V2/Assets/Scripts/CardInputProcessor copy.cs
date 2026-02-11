using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public partial class CardInputProcessor : MonoBehaviour
{
    [Header("Tile Map Linking")]
    [SerializeField] private Transform mapRoot; // optional; will auto-find "Map" if null

    // ---- Tile selection state ----
    private readonly Queue<TileRequest> pendingTileRequests = new();
    private TileRequest currentTileRequest;

    // Quick lookup: (x,y) -> Tile
    private readonly Dictionary<Vector2Int, Tile> tilesByXY = new();

    // Expose to UI
    public bool IsAwaitingTileSelection => currentTileRequest != null;
    public string CurrentTilePrompt => currentTileRequest == null ? "" : currentTileRequest.Prompt;

    // Call this after you accept card input (same place you currently parse/apply resource effects).
    // It will apply resource effects immediately, then begin tile prompting if needed.
    private void BuildAndStartTileRequests(List<CardPlay> acceptedPlaysThisTurn)
    {
        CacheTilesIfNeeded();

        pendingTileRequests.Clear();
        currentTileRequest = null;

        // Build list of tile-effect plays
        List<CardPlay> tilePlays = new();

        foreach (var play in acceptedPlaysThisTurn)
        {
            var tileFx = CollectTileEffects(play); // base + phase/outcome (or all if not specified)
            int tilesNeeded = CountTilesNeeded(tileFx, play.GetResolvedX() ?? 0);

            // Only queue if this card actually needs coordinate input
            if (tilesNeeded > 0)
            {
                tilePlays.Add(play);
                pendingTileRequests.Enqueue(new TileRequest(play, tileFx, tilesNeeded));
            }
        }

        // Log all tile cards
        if (tilePlays.Count > 0)
        {
            string msg = "[TILE FX] Cards requiring tile input:\n";
            for (int i = 0; i < tilePlays.Count; i++)
                msg += $"- {tilePlays[i].Card.CardId} {tilePlays[i].Card.CardName}\n";
            Debug.Log(msg);
        }

        StartNextTileRequest();
    }

    // UI should call this with the coordinate string for the CURRENT tile card:
    // "(2,3), (1,4)" etc
    public bool SubmitTileCoordinates(string coordsText)
    {
        if (currentTileRequest == null)
        {
            Debug.LogWarning("[TILE FX] No tile request is active.");
            return false;
        }

        if (!TryParseTileCoords(coordsText, out List<Vector2Int> coords, out string parseErr))
        {
            Debug.LogError($"[TILE FX] Bad tile input: {parseErr}");
            return false;
        }

        if (coords.Count != currentTileRequest.TilesNeeded)
        {
            Debug.LogError($"[TILE FX] Wrong number of tiles. Expected {currentTileRequest.TilesNeeded}, got {coords.Count}.");
            return false;
        }

        // Resolve Tile objects
        var tiles = new List<Tile>(coords.Count);
        var seen = new HashSet<Vector2Int>();
        for (int i = 0; i < coords.Count; i++)
        {
            if (!seen.Add(coords[i]))
            {
                Debug.LogError($"[TILE FX] Duplicate tile coordinate: {coords[i]}");
                return false;
            }

            if (!tilesByXY.TryGetValue(coords[i], out Tile t) || t == null)
            {
                Debug.LogError($"[TILE FX] No tile exists at {coords[i]}");
                return false;
            }
            tiles.Add(t);
        }

        // Validate tiles against effects (minimal checks)
        if (!ValidateTilesForRequest(currentTileRequest, tiles, out string validationErr))
        {
            Debug.LogError($"[TILE FX] Tile validation failed: {validationErr}");
            return false;
        }

        // Apply tile effects (minimal concrete changes + logs)
        ApplyTileEffectsToTiles(currentTileRequest, tiles);

        Debug.Log($"[TILE FX] Accepted tiles for {currentTileRequest.Play.Card.CardId}. Moving to next tile card...");
        StartNextTileRequest();
        return true;
    }

    // Use this in your EndTurnButton:
    // - if awaiting tile selection, block ending the turn
    public bool CanEndTurnNow()
    {
        if (IsAwaitingTileSelection)
        {
            Debug.LogWarning("[TURN] Cannot end turn: still awaiting tile selections.");
            Debug.Log(CurrentTilePrompt);
            return false;
        }
        return true;
    }

    // ---------------------------
    // Internals
    // ---------------------------

    private void StartNextTileRequest()
    {
        if (pendingTileRequests.Count == 0)
        {
            currentTileRequest = null;
            return;
        }

        currentTileRequest = pendingTileRequests.Dequeue();

        Debug.Log(
            "[TILE FX] Next tile card:\n" +
            $"- {currentTileRequest.Play.Card.CardId} {currentTileRequest.Play.Card.CardName}\n" +
            $"- {currentTileRequest.Play.Card.CardDescription}\n" +
            $"- Tiles required: {currentTileRequest.TilesNeeded}\n" +
            "Enter tiles as: (x1, y1), (x2, y2), ..."
        );

        Debug.Log(currentTileRequest.Prompt);
    }

    private void CacheTilesIfNeeded()
    {
        tilesByXY.Clear();

        if (mapRoot == null)
        {
            var go = GameObject.Find("Map");
            if (go != null) mapRoot = go.transform;
        }

        if (mapRoot == null)
        {
            Debug.LogError("[TILE FX] mapRoot not set and GameObject 'Map' not found.");
            return;
        }

        var tiles = mapRoot.GetComponentsInChildren<Tile>(true);
        foreach (var t in tiles)
        {
            var key = new Vector2Int(t.GridX, t.GridY); // requires GridX/GridY getters in HexagonalGrid
            tilesByXY[key] = t;
        }
    }

    private static bool TryParseTileCoords(string text, out List<Vector2Int> coords, out string error)
    {
        coords = new List<Vector2Int>();
        error = "";

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Empty input.";
            return false;
        }

        // Matches "(2, 3)" with optional spaces, supports negatives if needed.
        var matches = Regex.Matches(text, @"\(\s*(-?\d+)\s*,\s*(-?\d+)\s*\)");
        if (matches.Count == 0)
        {
            error = "No coordinates found. Expected format: (x1, y1), (x2, y2)";
            return false;
        }

        foreach (Match m in matches)
        {
            if (!int.TryParse(m.Groups[1].Value, out int x) || !int.TryParse(m.Groups[2].Value, out int y))
            {
                error = $"Invalid number in: {m.Value}";
                return false;
            }
            coords.Add(new Vector2Int(x, y));
        }

        return true;
    }

    // ---- Tile Effects collection ----
    // Default behavior for TILE effects:
    // - If phase not provided, include ALL phase tile blocks
    // - If outcome not provided, include ALL outcome tile blocks
    private List<TileEffect> CollectTileEffects(CardPlay play)
    {
        var list = new List<TileEffect>();

        if (play.Card.BaseTileEffects != null)
            list.AddRange(play.Card.BaseTileEffects);

        // Phase
        if (play.Card.PhaseTileEffects != null && play.Card.PhaseTileEffects.Count > 0)
        {
            if (play.Phase != null)
            {
                foreach (var pb in play.Card.PhaseTileEffects)
                    if (pb.phase == play.Phase.Value) list.AddRange(pb.effects);
            }
            else
            {
                foreach (var pb in play.Card.PhaseTileEffects)
                    list.AddRange(pb.effects);
            }
        }

        // Outcome
        if (play.Card.OutcomeTileEffects != null && play.Card.OutcomeTileEffects.Count > 0)
        {
            if (play.Outcome != null)
            {
                foreach (var ob in play.Card.OutcomeTileEffects)
                    if (ob.outcome == play.Outcome.Value) list.AddRange(ob.effects);
            }
            else
            {
                foreach (var ob in play.Card.OutcomeTileEffects)
                    list.AddRange(ob.effects);
            }
        }

        return list;
    }

    // values order: [v, s, t, d]
    // tiles needed uses t; if t == -1 then use input X
    private static int CountTilesNeeded(List<TileEffect> effects, int xValue)
    {
        int total = 0;
        if (effects == null) return 0;

        for (int i = 0; i < effects.Count; i++)
        {
            int t = GetValue(effects[i], 2, 0);
            if (t == 0) continue;

            if (t == -1) t = xValue; // variable tiles
            if (t > 0) total += t;
        }
        return total;
    }

    private static int GetValue(TileEffect fx, int idx, int fallback)
    {
        if (fx.values == null) return fallback;
        if (idx < 0 || idx >= fx.values.Count) return fallback;
        return fx.values[idx];
    }

    private static bool ValidateTilesForRequest(TileRequest req, List<Tile> tiles, out string error)
    {
        error = "";

        // We validate per-effect, consuming tiles in-order (first T tiles belong to first effect, etc.)
        int cursor = 0;
        int xValue = req.Play.GetResolvedX() ?? 0;

        for (int i = 0; i < req.Effects.Count; i++)
        {
            var fx = req.Effects[i];

            int t = GetValue(fx, 2, 0);
            if (t == 0) continue;
            if (t == -1) t = xValue;

            int v = GetValue(fx, 0, 0);
            int s = GetValue(fx, 1, 0);

            for (int k = 0; k < t; k++)
            {
                if (cursor >= tiles.Count)
                {
                    error = "Not enough tiles provided for effects list.";
                    return false;
                }

                Tile tile = tiles[cursor];
                var td = tile.tileData;

                // Minimal rules
                if (td == null)
                {
                    error = $"Tile at ({tile.GridX},{tile.GridY}) has no tileData.";
                    return false;
                }

                if (fx.name == "FireReduce")
                {
                    if (td.destroyed)
                    {
                        error = "Cannot change fire on destroyed tiles.";
                        return false;
                    }

                    if (td.onFire <= 0)
                    {
                        error = "FireReduce requires a tile that is currently on fire.";
                        return false;
                    }

                    if (s == 1 && td.onFire != 1)
                    {
                        error = "FireReduce(s=1) requires fire stage 1.";
                        return false;
                    }
                    if (s == 2 && td.onFire != 2)
                    {
                        error = "FireReduce(s=2) requires fire stage 2 (heavily on fire).";
                        return false;
                    }
                }
                else if (fx.name == "FuelDelta")
                {
                    if (td.destroyed)
                    {
                        error = "Cannot change fuel on destroyed tiles.";
                        return false;
                    }
                    if (td.tileType == TileType.WATERBODY)
                    {
                        error = "Cannot change fuel on WATERBODY tiles.";
                        return false;
                    }
                }
                else if (fx.name == "FireImmuneTile")
                {
                    if (td.destroyed) { error = "Cannot apply FireImmuneTile to destroyed tiles."; return false; }
                    if (td.onFire != 0) { error = "FireImmuneTile requires tile NOT currently on fire."; return false; }
                }
                else if (fx.name == "PreventSpreadTile")
                {
                    if (td.destroyed) { error = "Cannot apply PreventSpreadTile to destroyed tiles."; return false; }
                    if (td.onFire == 0) { error = "PreventSpreadTile requires tile currently on fire."; return false; }
                }
                else if (fx.name == "BuildingDevelop")
                {
                    int n = v; // you encoded n into v for BuildingDevelop(n)
                    if (td.tileType != TileType.BUILDING)
                    {
                        error = "BuildingDevelop requires BUILDING tiles.";
                        return false;
                    }

                    if (n == 1 && !td.destroyed)
                    {
                        error = "BuildingDevelop(1) Build requires destroyed==true.";
                        return false;
                    }
                    if (n == 2 && !td.damaged)
                    {
                        error = "BuildingDevelop(2) Repair requires damaged==true.";
                        return false;
                    }
                    if (n == 3 && td.destroyed)
                    {
                        error = "BuildingDevelop(3) Upgrade cannot target destroyed buildings.";
                        return false;
                    }
                }

                cursor++;
            }
        }

        return true;
    }

    private static void ApplyTileEffectsToTiles(TileRequest req, List<Tile> tiles)
    {
        int cursor = 0;
        int xValue = req.Play.GetResolvedX() ?? 0;

        for (int i = 0; i < req.Effects.Count; i++)
        {
            var fx = req.Effects[i];

            int t = GetValue(fx, 2, 0);
            if (t == 0) continue;
            if (t == -1) t = xValue;

            int v = GetValue(fx, 0, 0);
            int s = GetValue(fx, 1, 0);
            int d = GetValue(fx, 3, 0);

            // Consume T tiles for this effect
            for (int k = 0; k < t; k++)
            {
                var tile = tiles[cursor++];
                var td = tile.tileData;

                if (fx.name == "FuelDelta")
                {
                    td.fuelLoad = Mathf.Clamp(td.fuelLoad + v, 0, 4);
                    Debug.Log($"[TILE FX] FuelDelta({v}) applied to ({tile.GridX},{tile.GridY}) duration D={d}");
                }
                else if (fx.name == "FireReduce")
                {
                    // v expected negative to reduce
                    // s: 0=all,1,2
                    if (s == 0 || td.onFire == s)
                        td.onFire = Mathf.Clamp(td.onFire + v, 0, 2);

                    Debug.Log($"[TILE FX] FireReduce({v}, s={s}) applied to ({tile.GridX},{tile.GridY}) duration D={d}");
                }
                else if (fx.name == "FireImmuneTile")
                {
                    // Effect persistence system not implemented here; just log
                    Debug.Log($"[TILE FX] FireImmuneTile applied to ({tile.GridX},{tile.GridY}) duration D={d}");
                }
                else if (fx.name == "PreventSpreadTile")
                {
                    Debug.Log($"[TILE FX] PreventSpreadTile applied to ({tile.GridX},{tile.GridY}) duration D={d}");
                }
                else if (fx.name == "BuildingDevelop")
                {
                    int n = v; // (1)=build,(2)=repair,(3)=upgrade
                    if (n == 1) td.destroyed = false;
                    if (n == 2) td.damaged = false;
                    if (n == 3) tile.Upgrade(); // uses Tile.Upgrade() :contentReference[oaicite:2]{index=2}

                    Debug.Log($"[TILE FX] BuildingDevelop({n}) applied to ({tile.GridX},{tile.GridY}) duration D={d}");
                }
                else
                {
                    Debug.LogWarning($"[TILE FX] Unhandled tile effect '{fx.name}' on ({tile.GridX},{tile.GridY}).");
                }
            }
        }
    }

    // ---------------------------
    // Small helper class
    // ---------------------------
    private sealed class TileRequest
    {
        public readonly CardPlay Play;
        public readonly List<TileEffect> Effects;
        public readonly int TilesNeeded;
        public string Prompt =>
            $"[TILE FX] {Play.Card.CardId} requires {TilesNeeded} tile(s). Input: (x,y), (x,y)...";

        public TileRequest(CardPlay play, List<TileEffect> effects, int tilesNeeded)
        {
            Play = play;
            Effects = effects ?? new List<TileEffect>();
            TilesNeeded = Mathf.Max(0, tilesNeeded);
        }
    }
}
