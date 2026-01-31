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


    public int currentTurn = 1;
    public int currentPhase = 1; // 1: Preparation, 2: Firebush, 3: Reflection
    // Start is called before the first frame update
    void Start()
    {
        currentTurn = 1;
        turnText.text = "Current Turn: " + currentTurn.ToString();

        currentPhase = 1;
        phaseText.text = "Current Phase: " + "Preparation";
        
    }

    // Update is called once per frame
    void Update()
    {
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
}
