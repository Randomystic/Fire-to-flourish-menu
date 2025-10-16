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

    // keep a handle to each role's title transform (so we can spawn under it)
    readonly Dictionary<RoleType, Transform> roleTitles = new();

    // UI bundle per role that needs tile input
    class TileInputUI { public TMP_InputField x,y,z,name; public Button confirm; public bool confirmed; }
    readonly Dictionary<RoleType, TileInputUI> roleTileUI = new();

    readonly HashSet<RoleType> rolesNeedingTile = new();
    bool awaitingTileInputs;


    [SerializeField] bool constantFontSize = true;
    [SerializeField] float dropdownFontSize = 16f;

    [SerializeField] Vector3 dropdownScale = new Vector3(0.25f, 0.25f, 1f);
    [SerializeField] float dropdownOffsetX = 0f;
    [SerializeField] float dropdownOffsetY = -50f;

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
                roleCardIDs[role] = ids;
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

    void Save()
    {
        // collect selections
        selectedCardsDict.Clear();
        foreach (var kv in roleDropdowns)
        {
            var role = kv.Key;
            var dropdown = kv.Value;
            if (dropdown && dropdown.options.Count > 0)
                selectedCardsDict[role] = roleCardIDs[role][dropdown.value];
        }
        lastSelections = new Dictionary<RoleType, string>(selectedCardsDict);

        // if we already spawned inputs, only proceed when all confirmed
        if (awaitingTileInputs)
        {
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

        // first click: detect which roles need tile input (fuel_load)
        rolesNeedingTile.Clear();
        foreach (var kv in lastSelections)
            if (CardHasFuelEffect(kv.Value))
                rolesNeedingTile.Add(kv.Key);

        if (rolesNeedingTile.Count > 0)
        {
            SpawnTileInputsForRoles();
            awaitingTileInputs = true;
            Debug.Log("Enter tile coords + name under roles with fuel load effects, then press Confirm. Press Save again to continue.");
            debugText.text = "Enter tile coords + name under roles with fuel load effects, then press Confirm. Press Save again to continue.";
            return;
        }

        // no tile input needed, or all confirmed
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
            if (Normalize(e.resourceName) == "fuelload" && e.value != 0)
                return true;
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
            ui.x    = Instantiate(inputPrefab, parent);
            ui.y    = Instantiate(inputPrefab, parent);
            ui.z    = Instantiate(inputPrefab, parent);
            ui.name = Instantiate(inputPrefab, parent);
            ui.confirm = Instantiate(confirmButtonPrefab, parent);

            ui.x.placeholder.GetComponent<TMP_Text>().text = "x (int)";
            ui.y.placeholder.GetComponent<TMP_Text>().text = "y (int)";
            ui.z.placeholder.GetComponent<TMP_Text>().text = "z (int)";
            ui.name.placeholder.GetComponent<TMP_Text>().text = "tile name";
            ui.x.contentType = TMP_InputField.ContentType.IntegerNumber;
            ui.y.contentType = TMP_InputField.ContentType.IntegerNumber;
            ui.z.contentType = TMP_InputField.ContentType.IntegerNumber;

            // simple vertical layout: offsets from title
            var y0 = -160f;
            var x0 = -100f;
            ui.x.GetComponent<RectTransform>().anchoredPosition    = new Vector2(x0, y0);
            ui.y.GetComponent<RectTransform>().anchoredPosition    = new Vector2(x0 + 120, y0);
            ui.z.GetComponent<RectTransform>().anchoredPosition    = new Vector2(x0, y0 - 40);
            ui.name.GetComponent<RectTransform>().anchoredPosition = new Vector2(x0 + 120, y0 - 40);
            ui.confirm.GetComponent<RectTransform>().anchoredPosition = new Vector2(x0 + 60, y0 - 100);

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
        if (!int.TryParse(ui.x.text, out int x) ||
             !int.TryParse(ui.y.text, out int y) ||
            !int.TryParse(ui.z.text, out int z) ||

            string.IsNullOrWhiteSpace(ui.name.text))
        { 
            Debug.Log("Invalid Syntax"); 
            debugText.text = "Invalid Syntax";
            return; 
        }

        if (x + y + z != 0) { 
            Debug.Log("Invalid Syntax (x+y+z must equal 0)"); 
            debugText.text = "Invalid Syntax (x+y+z must equal 0)";
            return; 
            }

        if (!map || map.tiles == null) 
        { 
            Debug.Log("Map not found"); 
            debugText.text = "Map not found";
            return; 
            }

        var key = new Vector3Int(x, y, z);
        if (!map.tiles.TryGetValue(key, out var tile)) { 
            Debug.Log("Invalid Syntax (tile not found)"); 
            debugText.text = "Invalid Syntax (tile not found)";
            return; 
            }

        // optional name check (case/underscore insensitive)
        var inputName = Normalize(ui.name.text);
        var tileName  = Normalize(tile.tileName);
        if (inputName.Length > 0 && inputName != tileName)
        { 
            Debug.Log("Invalid Syntax (tile name mismatch)"); 
            debugText.text = "Invalid Syntax (tile name mismatch)";
            return; 
            }

        // apply the fuel delta for THIS role's selected card
        if (!lastSelections.TryGetValue(role, out var cardId)) { 
            Debug.Log("No card for role"); 
            debugText.text = "No card for role";
            return; 
            }


        int delta = GetFuelDeltaForCard(cardId);
        tile.fuelLoad = Mathf.Max(0, tile.fuelLoad + delta);

        ui.confirm.interactable = false;
        ui.confirmed = true;

        Debug.Log($"Input Accepted → {role} applied Fuel:{delta} to Tile {tile.tileName} at {key} (type {tile.tileType}).");
        debugText.text = $"{role} applied Fuel:{delta} to Tile {tile.tileName} at {key}".ToLower();
        updatedTiles[key] = debugText.text;

        Debug.Log($"Stored update info for TownActionsDisplay");
    }


}
