using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnManagement : MonoBehaviour
{
    [Header("UI")]
    public Button endTurnButton;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;
    public TMP_InputField cardInputField;
    public ScrollRect cardScrollBar;
    public TextMeshProUGUI cardListText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI townSummaryText;
    public Button mapButton;

    [Header("Timer")]
    public int minutes;
    public int seconds;

    float time;
    bool running;

    public Button resetTimerButton;
    public Button startTimerButton;

    [Header("Turn State")]
    public int currentTurn = 1;
    public int currentPhase = 1; // 1: Preparation, 2: Firebush, 3: Reflection

    [Header("Card Processing")]
    [SerializeField] private CardInputProcessor cardInputProcessor; // drag in Inspector
    [SerializeField] private string townResourcesPath = "TownResources"; // TownResources.asset

    public string currentCardInputs = "";

    void Start()
    {
        currentTurn = 1;
        turnText.text = "Current Turn: " + currentTurn.ToString();

        currentPhase = 1;
        phaseText.text = "Current Phase: Preparation";

        time = minutes * 60 + seconds;
        running = false;
        UpdateDisplay();
        DisplayTownSummary();

        // Optional safety: auto-find if not assigned
        if (cardInputProcessor == null)
            cardInputProcessor = FindObjectOfType<CardInputProcessor>();
    }

    void Update()
    {
        
        if (!running) return;

        if (time <= 0)
        {
            time = 0;
            running = false;
            UpdateDisplay();
            return;
        }

        time -= Time.deltaTime;
        if (time < 0) time = 0;
        UpdateDisplay();

        if (time <= 0) running = false;

    }

    void UpdateDisplay()
    {
        int m = Mathf.FloorToInt(time / 60f);
        int s = Mathf.FloorToInt(time % 60f);
        timerText.text = m.ToString("00") + ":" + s.ToString("00");
    }

    // Call this from End Turn button (recommended)
    public void EndTurnButtonClicked()
    {
        if (cardInputProcessor == null)
        {
            Debug.LogError("No CardInputProcessor assigned/found. Cannot validate/apply card input.");
            return;
        }

        currentCardInputs = cardInputField.text.Trim();

        // Validate + Apply. If invalid, DO NOT end the turn.
        bool success = cardInputProcessor.SubmitTurn(currentCardInputs);
        if (!success)
        {
            Debug.LogWarning("Turn not advanced because card input has errors.");
            return;
        }

        // If we get here: all effects applied successfully. Now advance turn.
        AdvanceTurnUI();

        // Clear input for next turn
        cardInputField.text = "";
        cardListText.text = "";

        DisplayTownSummary();
    }

    private void AdvanceTurnUI()
    {
        currentTurn++;
        turnText.text = "Current Turn: " + currentTurn.ToString();

        int phaseIndex = ((currentTurn - 1) / 3) % 2;

        if (phaseIndex == 0)
        {
            currentPhase = 1;
            phaseText.text = "Current Phase: Preparation";
        }
        else
        {
            currentPhase = 2;
            phaseText.text = "Current Phase: Firebush";
        }
    }

    // Optional: call on input field end edit to show the list (not validation)
    public void OnEndEditCardInput()
    {
        currentCardInputs = cardInputField.text;

        // Just display tokens nicely; real validation happens on EndTurn
        string[] tokens = currentCardInputs.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
            tokens[i] = tokens[i].Trim();

        cardListText.text = string.Join("\n", tokens);
    }

    public void OnResetTimerButtonClicked()
    {
        running = false;
        time = minutes * 60 + seconds;
        UpdateDisplay();
    }

    public void OnStartTimerButtonClicked()
    {
        if (time <= 0) time = minutes * 60 + seconds;
        running = true;
        UpdateDisplay();
    }

    public void DisplayTownSummary()
    {
        TownResourceList town = Resources.Load<TownResourceList>(townResourcesPath);

        townSummaryText.text = town.GetResourceSummary();
        // Debug.Log("Current Town Resources:\n" + town.GetResourceSummary());
    }

    public void OnMapButtonClicked()
    {
       SceneManager.LoadScene("MainMap");
    }

    public void OnNarrativeButtonClicked()
    {
       SceneManager.LoadScene("Narrative");
    }

    public void OnGameSummaryButtonClicked()
    {
       SceneManager.LoadScene("GameSummary");
    }

    
}
