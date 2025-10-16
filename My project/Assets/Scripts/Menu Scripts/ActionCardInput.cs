using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ActionCardInput : MonoBehaviour
{
    public TMP_Dropdown dropdownPrefab;
    public Transform roleTitlesParent;
    public Button saveButton;
    public Vector2 dropdownSize = new Vector2(320, 34);
    public TMP_Text debugText; 

    public Dictionary<RoleType, string> selectedCardsDict = new();
    public static Dictionary<RoleType, string> lastSelections = new();

    public TMP_InputField inputPrefab;       // assign a TMP_InputField prefab
    public Button confirmButtonPrefab;       // assign a Button prefab
    public TMP_Text statusText;              // optional TMP text for feedback

    [Header("Tile UI Layout")]
    public Vector2 tileDropdownSize   = new Vector2(320, 34);
    public Vector3 tileDropdownScale  = new Vector3(0.25f, 0.25f, 1f);
    public float   tileDropdownOffsetX = -100f;   // anchored X relative to role title
    public float   tileDropdownOffsetY = -160f;   // anchored Y relative to role title
    public Vector2 tileConfirmOffset   = new Vector2(60f, -100f); // confirm button offset from dropdown anchor


    // keep a handle to each role's title transform (so we can spawn under it)
    readonly Dictionary<RoleType, Transform> roleTitles = new();

    // UI bundle per role that needs tile input
    class TileInputUI 
    { 
        public TMP_InputField x,y,z,name; 
        public Button confirm; 
        public bool confirmed; 

        public TMP_Dropdown tileDropdown;
        public List<Vector3Int> coordByIndex = new List<Vector3Int>();

    }


    readonly Dictionary<RoleType, TileInputUI> roleTileUI = new();

    readonly HashSet<RoleType> rolesNeedingTile = new();
    bool awaitingTileInputs;


    [SerializeField] bool constantFontSize = true;
    [SerializeField] float dropdownFontSize = 16f;

    [SerializeField] Vector3 dropdownScale = new Vector3(0.25f, 0.25f, 1f);
    [SerializeField] float dropdownOffsetX = 0f;
    [SerializeField] float dropdownOffsetY = -50f;

    [SerializeField] private TownResourceList townResources;

    readonly Dictionary<RoleType, TMP_Dropdown> roleDropdowns = new();
    readonly Dictionary<RoleType, List<string>> roleCardIDs  = new();
    
    public Map map;

    public static Dictionary<Vector3Int, string> updatedTiles = new();

    void Start()
    {
        if (!roleTitlesParent)
        {
            var go = GameObject.Find("Canvas/RoleTitles");
            if (go) roleTitlesParent = go.transform;
        }

        // Cache civilian card IDs once
        List<string> civilianIds = null;
        if (RoleCards.roleCardsDict.TryGetValue(RoleType.CIVILIAN, out var civIds))
            civilianIds = civIds;

        foreach (RoleType role in Enum.GetValues(typeof(RoleType)))
        {
            var title = roleTitlesParent ? roleTitlesParent.Find(role.ToString()) : null;
            if (!title)
            {
                var path = $"Canvas/RoleTitles/{role}";
                var f = GameObject.Find(path);
                if (!f) continue;
                title = f.transform;
            }

            roleTitles[role] = title;

            var roleDropdown = Instantiate(dropdownPrefab, title, false);
            roleDropdown.gameObject.SetActive(true);
            roleDropdown.gameObject.name = $"{role}_Dropdown";
            roleDropdown.ClearOptions();

            if (RoleCards.roleCardsDict.TryGetValue(role, out var ids))
            {
                roleCardIDs[role] = (ids != null) ? new List<string>(ids) : new List<string>();
                foreach (var id in ids) roleDropdown.options.Add(new TMP_Dropdown.OptionData(id.Replace("_", " ")));
            }
            else roleCardIDs[role] = new List<string>();

            if (civilianIds != null && role != RoleType.CIVILIAN)
            {
                foreach (var id in civilianIds)
                    roleDropdown.options.Add(new TMP_Dropdown.OptionData(id.Replace("_", " ")));

                roleCardIDs[role].AddRange(civilianIds);
            }

            ConfigureTMP(roleDropdown);
            var rectTransform = roleDropdown.GetComponent<RectTransform>();
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(dropdownOffsetX, dropdownOffsetY);
            rectTransform.localScale = dropdownScale;

            roleDropdowns[role] = roleDropdown;
            roleDropdown.onValueChanged.AddListener(_ => OnRoleCardChanged(role));
        }

        if (saveButton) saveButton.onClick.AddListener(Save);

        map = Map.Instance;
        if (map == null) Debug.LogError("No persistent Map found.");
        else
        {
            map.EnsureInitialized();
            Debug.Log($"ActionCardInput: Map tiles = {map.tiles.Count}");
        }
    }

    void OnRoleCardChanged(RoleType role)
    {
        if (!roleDropdowns.TryGetValue(role, out var dd)) return;
        if (!roleCardIDs.TryGetValue(role, out var ids)) return;

        var idx = dd.value;
        if (idx < 0 || idx >= ids.Count) return;

        string cardId = ids[idx];
        bool needsTile = CardHasFuelEffect(cardId);

        // If the new card does NOT need a tile, remove any existing tile UI immediately
        if (!needsTile)
        {
            if (roleTileUI.TryGetValue(role, out var ui))
            {
                if (ui.tileDropdown) Destroy(ui.tileDropdown.gameObject);
                if (ui.confirm) Destroy(ui.confirm.gameObject);
                if (ui.x) Destroy(ui.x.gameObject);
                if (ui.y) Destroy(ui.y.gameObject);
                if (ui.z) Destroy(ui.z.gameObject);
                if (ui.name) Destroy(ui.name.gameObject);
                roleTileUI.Remove(role);
            }
            rolesNeedingTile.Remove(role);
            return;
        }

        // Still needs a tile: either spawn UI or reset confirmation so user can confirm again
        if (!roleTileUI.TryGetValue(role, out var existing))
        {
            rolesNeedingTile.Add(role);
            SpawnTileInputsForRoles(); // reuses your existing spawner; it will create only missing UIs
        }
        else
        {
            existing.confirmed = false;
            if (existing.confirm) existing.confirm.interactable = true;
        }
    }

    void Save()
    {   
        ResourceDashboard.RecalculateFireSafety(Map.Instance, townResources);

        //Collect selections
        selectedCardsDict.Clear();
        foreach (var kv in roleDropdowns)
        {
            var role = kv.Key;
            var dropdown = kv.Value;
            if (dropdown && dropdown.options.Count > 0)
                selectedCardsDict[role] = roleCardIDs[role][dropdown.value];
        }
        lastSelections = new Dictionary<RoleType, string>(selectedCardsDict);

        //Refresh roles needing tile input
        rolesNeedingTile.Clear();
        foreach (var kv in lastSelections)
            if (CardHasFuelEffect(kv.Value))
                rolesNeedingTile.Add(kv.Key);

        // If we had previously spawned tile inputs, check if still relevant
        if (awaitingTileInputs)
        {
            // If no more roles need tiles (user changed card), skip validation
            if (rolesNeedingTile.Count == 0)
            {
                awaitingTileInputs = false;
                // remove any leftover UI from old fuel-load cards
                foreach (var kv in roleTileUI)
                {
                    if (kv.Value.x) Destroy(kv.Value.x.gameObject);
                    if (kv.Value.y) Destroy(kv.Value.y.gameObject);
                    if (kv.Value.z) Destroy(kv.Value.z.gameObject);
                    if (kv.Value.name) Destroy(kv.Value.name.gameObject);
                    if (kv.Value.confirm) Destroy(kv.Value.confirm.gameObject);

                    if (kv.Value.tileDropdown) Destroy(kv.Value.tileDropdown.gameObject);
                }
                roleTileUI.Clear();

                Debug.Log("Tile inputs cleared – proceeding to next scene.");
                SceneManager.LoadScene("TownActionsDisplay");
                return;
            }

            // Otherwise still require confirmations
            foreach (var r in rolesNeedingTile)
                if (!roleTileUI.TryGetValue(r, out var ui) || !ui.confirmed)
                {
                    Debug.Log("Tile inputs not confirmed yet.");
                    debugText.text = "Tile inputs not confirmed yet.";
                    return;
                }

            SceneManager.LoadScene("TownActionsDisplay");
            return;
        }

        // Need tile inputs for some roles
        if (rolesNeedingTile.Count > 0)
        {
            SpawnTileInputsForRoles();
            awaitingTileInputs = true;
            Debug.Log("Select a tile for each role that affects a tile, press Confirm, then press Save again to continue.");
            debugText.text = "Select a tile for each role that affects a tile, press Confirm, then press Save again to continue.";
            return;
        }


        SceneManager.LoadScene("TownActionsDisplay");
    }



    void ConfigureTMP(TMP_Dropdown roleDropdown)
    {
        void ConfigureDropdownLabel(TMP_Text txt)
        {
            if (!txt) return;
            txt.enableAutoSizing = !constantFontSize;

            if (constantFontSize) txt.fontSize = dropdownFontSize;
            txt.enableWordWrapping = false;
            txt.overflowMode = TextOverflowModes.Masking;
        }
        ConfigureDropdownLabel(roleDropdown.captionText);
        ConfigureDropdownLabel(roleDropdown.itemText);
    }

    bool CardHasFuelEffect(string cardId)
    {
        var card = Resources.Load<ActionCardData>($"Cards/{cardId}");
        if (!card) return false;
        foreach (var e in card.effects)
        {
            var key = Normalize(e.resourceName);
            if ((key == "fuelload" || key == "population") && e.value != 0)
                return true;
        }
        return false;
    }

    int GetFuelDeltaForCard(string cardId)
    {
        int sum = 0;
        var card = Resources.Load<ActionCardData>($"Cards/{cardId}");
        if (!card) return 0;
        foreach (var e in card.effects)
            if (Normalize(e.resourceName) == "fuelload") sum += e.value;
        return sum;
    }

    string Normalize(string s) => (s ?? "").Replace("_", "").ToLower();

    void SpawnTileInputsForRoles()
    {
        if (!map) map = FindObjectOfType<Map>();
        foreach (var role in rolesNeedingTile)
        {
            if (!roleTitles.TryGetValue(role, out var parent)) continue;
            if (roleTileUI.ContainsKey(role)) continue;

            // stack under this role title
            var ui = new TileInputUI();

            // simple vertical layout: offsets from title
            var y0 = -160f;
            var x0 = -100f;

           // tile dropdown
            ui.tileDropdown = Instantiate(dropdownPrefab, parent, false);
            ui.tileDropdown.gameObject.SetActive(true);
            ui.tileDropdown.gameObject.name = $"{role}_TileDropdown";
            ui.tileDropdown.ClearOptions();
            ConfigureTMP(ui.tileDropdown);

            var ddRect = ui.tileDropdown.GetComponent<RectTransform>();
            ddRect.anchorMin = ddRect.anchorMax = new Vector2(0.5f, 1f);
            ddRect.pivot = new Vector2(0.5f, 1f);
            ddRect.sizeDelta = tileDropdownSize;
            ddRect.anchoredPosition = new Vector2(tileDropdownOffsetX, tileDropdownOffsetY);
            ddRect.localScale = tileDropdownScale;

            // confirm button
            ui.confirm = Instantiate(confirmButtonPrefab, parent);
            var cbRect = ui.confirm.GetComponent<RectTransform>();
            cbRect.anchorMin = cbRect.anchorMax = new Vector2(0.5f, 1f);
            cbRect.pivot = new Vector2(0.5f, 1f);
            cbRect.anchoredPosition = new Vector2(
                tileDropdownOffsetX + tileConfirmOffset.x,
                tileDropdownOffsetY + tileConfirmOffset.y);

            ConfigureTMP(ui.tileDropdown);


            // deterministic order
            var keys = new List<Vector3Int>(map.tiles.Keys);
            keys.Sort((a,b) =>
            {
                int c = a.x.CompareTo(b.x); if (c != 0) return c;
                c = a.y.CompareTo(b.y);     if (c != 0) return c;
                return a.z.CompareTo(b.z);
            });

            foreach (var cube in keys)
            {
                var t = map.tiles[cube];
                string label = $"({cube.x}, {cube.y}, {cube.z}): {t.tileName}";
                ui.tileDropdown.options.Add(new TMP_Dropdown.OptionData(label));
                ui.coordByIndex.Add(cube);
            }

            var capturedRole = role; // capture for lambda
            ui.confirm.onClick.AddListener(() => ConfirmTileForRole(capturedRole));

            roleTileUI[role] = ui;
            Debug.Log($"Tile inputs spawned for {role} (fuel load card selected).");
        }
    }


    void ConfirmTileForRole(RoleType role)
    {
        foreach (var kv in map.tiles)
            Debug.Log($"TILE KEY: ({kv.Key.x},{kv.Key.y},{kv.Key.z}) -> {kv.Value.tileName}");

        if (!roleTileUI.TryGetValue(role, out var ui)) return;

        if (!map || map.tiles == null)
        {
            Debug.Log("Map not found");
            debugText.text = "Map not found";
            return;
        }

        //Pull cube from dropdown selection
        if (ui.tileDropdown == null || ui.tileDropdown.value < 0 || ui.tileDropdown.value >= ui.coordByIndex.Count)
        {
            Debug.Log("Invalid Syntax");
            debugText.text = "Please select a tile.";
            return;
        }
        var key = ui.coordByIndex[ui.tileDropdown.value];

        if (!map.tiles.TryGetValue(key, out var tile))
        {
            Debug.Log("Invalid Syntax (tile not found)");
            debugText.text = "Invalid Syntax (tile not found)";
            return;
        }

        // apply the fuel delta for THIS role's selected card
        if (!lastSelections.TryGetValue(role, out var cardId))
        {
            Debug.Log("No card for role");
            debugText.text = "No card for role";
            return;
        }

        int delta = GetFuelDeltaForCard(cardId);
        tile.fuelLoad = Mathf.Max(0, tile.fuelLoad + delta);

        if (ui.confirm) ui.confirm.interactable = false;
        ui.confirmed = true;

        Debug.Log($"Input Accepted -> {role} applied Fuel:{delta} to Tile {tile.tileName} at {key} (type {tile.tileType}).");
        debugText.text = $"{role} applied Fuel:{delta} to Tile {tile.tileName} at {key}".ToLower();
        updatedTiles[key] = debugText.text;

        Debug.Log($"Stored update info for TownActionsDisplay");
    }



}
