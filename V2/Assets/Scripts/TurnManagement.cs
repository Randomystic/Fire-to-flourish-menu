using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManagement : MonoBehaviour
{

    public Button endTurnButton;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;
    public TMP_InputField cardInputField;
    public ScrollRect cardScrollBar;
    public TextMeshProUGUI cardListText;

    public int minutes;
    public int seconds;

    public TextMeshProUGUI timerText;

    float time;
    bool running;

    public Button resetTimerButton;
    public Button startTimerButton;


    public int currentTurn = 1;
    public int currentPhase = 1; // 1: Preparation, 2: Firebush, 3: Reflection
    // Start is called before the first frame update

    public string currentCardInputs = "";


    void Start()
    {
        currentTurn = 1;
        turnText.text = "Current Turn: " + currentTurn.ToString();

        currentPhase = 1;
        phaseText.text = "Current Phase: " + "Preparation";
        
        time = minutes * 60 + seconds;
        running = false;
        UpdateDisplay();

    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;
        
        if (time <= 0) { 
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


    public void EndTurnButtonClicked()
    {
        Debug.Log("End Turn Button clicked!");
        currentTurn++;
        turnText.text = "Current Turn: " + currentTurn.ToString();

        int phaseIndex = ((currentTurn - 1)/ 3) % 2; 

        if (phaseIndex == 0)
        {
            currentPhase = 1;
            phaseText.text = "Current Phase: Preparation";
        }
        else if (phaseIndex == 1)
        {
            currentPhase = 2;
            phaseText.text = "Current Phase: Firebush";
        }
        

    }

    public void OnEndEditCardInput()
    {
        currentCardInputs = cardInputField.text;
        Debug.Log("Card Inputs: " + currentCardInputs);

        string[] cardsArray = currentCardInputs.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < cardsArray.Length; i++)
        {
            cardsArray[i] = cardsArray[i].Trim();

            if (int.TryParse(cardsArray[i], out int cardNumber))
            {
                cardsArray[i] = "Card " + cardNumber.ToString();
            }
            else
            {
                cardsArray[i] = "Invalid Input";
            }
        }
        currentCardInputs = string.Join("\n", cardsArray);
        cardListText.text = currentCardInputs;

        // cardListText.text = currentCardInputs;

        

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


}
