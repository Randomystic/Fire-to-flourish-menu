using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreperationPhase : MonoBehaviour
{
    public TMP_Dropdown dropdownPrefab;
    public Transform dropdownParent;
    public Button saveButton;

    public Dictionary<RoleType, string> selectedCardsDict = new();

    readonly Dictionary<RoleType, TMP_Dropdown> roleDropdowns = new();
    readonly Dictionary<RoleType, List<string>> roleCardIds = new();

    void Start()
    {
        foreach (RoleType role in Enum.GetValues(typeof(RoleType)))
        {
            var dd = Instantiate(dropdownPrefab, dropdownParent);
            dd.gameObject.name = role.ToString();
            dd.ClearOptions();

            if (RoleCards.roleCardsDict.TryGetValue(role, out var ids))
            {
                roleCardIds[role] = ids;
                var labels = new List<string>(ids.Count);
                foreach (var id in ids) labels.Add(id.Replace("_", " "));
                dd.AddOptions(labels);
            }
            else
            {
                roleCardIds[role] = new List<string>();
            }

            ConfigureTMP(dd);
            roleDropdowns[role] = dd;
        }

        if (saveButton) saveButton.onClick.AddListener(Save);
    }

    void Save()
    {
        selectedCardsDict.Clear();
        foreach (var kv in roleDropdowns)
        {
            var role = kv.Key;
            var dd = kv.Value;
            if (dd.options.Count == 0) continue;

            int idx = dd.value;
            string pickedId = roleCardIds[role][idx];
            selectedCardsDict[role] = pickedId;
        }
    }

    static void ConfigureTMP(TMP_Dropdown dd)
    {
        void Cfg(TMP_Text t)
        {
            if (!t) return;
            t.enableAutoSizing = true;
            t.fontSizeMin = 8;
            t.fontSizeMax = 28;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
        }

        Cfg(dd.captionText);
        Cfg(dd.itemText);
    }
}
