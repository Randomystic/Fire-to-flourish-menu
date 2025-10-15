using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class PreparationTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_InputField timerInput;

    public Button startButton;
    public Button resetButton;

    public float timerSeconds = 60f;

    float remainingTime;
    bool isTimerRunning = false;

    private static int index = 1;

    void Start()
    {
        remainingTime = timerSeconds;
        UpdateTimerText();

        if (timerInput)
        {
            timerInput.text = timerSeconds.ToString();
            timerInput.onEndEdit.AddListener(UpdateTimerFromInput);
        }


        if (startButton) startButton.onClick.AddListener(StartTimer);
        if (resetButton) resetButton.onClick.AddListener(ResetTimer);
    }

    void Update()
    {
        if (!isTimerRunning) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {

            remainingTime = 0f;
            isTimerRunning = false;
        }
        UpdateTimerText();
    }

    void StartTimer()
    {
        isTimerRunning = true;
    }

    void ResetTimer()
    {
        
        isTimerRunning = false;
        remainingTime = timerSeconds;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        timerText.text = Mathf.CeilToInt(remainingTime).ToString() + "s";
    }

    
    void UpdateTimerFromInput(string value)
    {
        if (float.TryParse(value, out float newSeconds) && newSeconds > 0)
        {
            timerSeconds = newSeconds;
            remainingTime = newSeconds;
            UpdateTimerText();
        }
        else
        {
            timerInput.text = timerSeconds.ToString();
        }
    }
    
    public void Save()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "DiscussionPhase")
        {
            if (index > 3)
                SceneManager.LoadScene("FireSimulation");
            else
            {
                index += 1;
                SceneManager.LoadScene("Scenario");
            }
        }
        else if (currentScene == "PreparationPhase")
        {
            SceneManager.LoadScene("ActionCardUI");
        }
    }
}