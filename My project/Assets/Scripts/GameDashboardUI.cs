using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class GameDashboardUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown sceneDropdown;
    public Button confirmButton;
    public TMP_Text townStatsText;

    [Header("Scene List (Exact Names)")]
    public string[] sceneNames;

    void Start()
    {
        if (sceneDropdown)
        {
            sceneDropdown.ClearOptions();
            var sceneNames = GetSceneNames();
            sceneDropdown.AddOptions(new System.Collections.Generic.List<string>(sceneNames));
        }

        if (confirmButton) confirmButton.onClick.AddListener(SwitchScene);

        UpdateTownStats();
    }

    void UpdateTownStats()
    {
        var rm = ResourceManager.GetInstance();
        var town = (rm != null) ? rm.townResources : null;
        if (town == null)
        {
            townStatsText.text = "No town resources found.";
            return;
        }

        townStatsText.text =
            $"Provisions: {town.provisions}\n" +
            $"Education: {town.education}\n" +
            // $"Population: {town.population}\n" +
            $"Happiness: {town.happiness}\n" +
            $"Firefighting Equipment: {town.firefightingEquipment}\n" +
            $"Fire Safety Rating: {town.fireSafetyRating}\n" +
            $"Wind Speed: {town.windSpeed}\n" +
            $"Temperature Season: {town.temperatureSeason}";
    }

    List<string> GetSceneNames()
    {
        List<string> names = new List<string>();
        string path = Application.dataPath + "/Scenes";
        if (Directory.Exists(path))
        {
            foreach (var file in Directory.GetFiles(path, "*.unity"))
                names.Add(Path.GetFileNameWithoutExtension(file));
        }
        return names;
    }

    void SwitchScene()
    {
        if (!sceneDropdown) return;
        var sceneName = sceneDropdown.options[sceneDropdown.value].text;
        SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        // Allow quick return to dashboard with Backspace
        if (Input.GetKeyDown(KeyCode.Backspace))
            SceneManager.LoadScene("GameDashboard"); // Replace with your exact dashboard scene name
    }

    public void Save()
    {
        // // Ensure only one Map survives
        // var existingMap = FindObjectOfType<Map>();
        // if (existingMap == null)
        // {
        //     Debug.Log("No Map found in current scene — safe to load.");
        //     SceneManager.LoadScene("Map");
        // }
        // else
        // {
        //     Debug.Log("Persistent Map already exists, re-enabling UI instead of reloading.");
        //     existingMap.SetMapVisibility(true);
        //     SceneManager.LoadScene("Map");
        // }
        SceneManager.LoadScene("Map");
    }

}
