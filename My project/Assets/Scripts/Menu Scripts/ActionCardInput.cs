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

    public Dictionary<RoleType, string> selectedCardsDict = new();
    public static Dictionary<RoleType, string> lastSelections = new();


    [SerializeField] bool constantFontSize = true;
    [SerializeField] float dropdownFontSize = 16f;

    [SerializeField] Vector3 dropdownScale = new Vector3(0.25f, 0.25f, 1f);
    [SerializeField] float dropdownOffsetX = 0f;
    [SerializeField] float dropdownOffsetY = -50f;

    readonly Dictionary<RoleType, TMP_Dropdown> roleDropdowns = new();
    readonly Dictionary<RoleType, List<string>> roleCardIDs  = new();

    void Start()
    {
        if (!roleTitlesParent)
        {
            var go = GameObject.Find("Canvas/RoleTitles");
            if (go) roleTitlesParent = go.transform;
        }

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

            var roleDropdown = Instantiate(dropdownPrefab, title, false);

            roleDropdown.gameObject.SetActive(true);
            roleDropdown.gameObject.name = $"{role}_Dropdown";
            roleDropdown.ClearOptions();

            if (RoleCards.roleCardsDict.TryGetValue(role, out var ids))
            {
                roleCardIDs[role] = ids;
                var labels = new List<string>(ids.Count);

                foreach (var id in ids) labels.Add(id.Replace("_", " "));
                roleDropdown.AddOptions(labels);
            }
            else roleCardIDs[role] = new List<string>();

            ConfigureTMP(roleDropdown);
            var rectTransform = roleDropdown.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot     = new Vector2(0.5f, 1f);

            rectTransform.anchoredPosition = new Vector2(dropdownOffsetX, dropdownOffsetY);
            rectTransform.localScale = dropdownScale;

            roleDropdowns[role] = roleDropdown;
        }

        if (saveButton) saveButton.onClick.AddListener(Save);
    }

    void Save()
    {
        selectedCardsDict.Clear();
        foreach (var kv in roleDropdowns)
        {
            var role = kv.Key;
            var dropdown = kv.Value;
            if (dropdown && dropdown.options.Count > 0)
                selectedCardsDict[role] = roleCardIDs[role][dropdown.value];
        }

        lastSelections = new Dictionary<RoleType, string>(selectedCardsDict);
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
}
