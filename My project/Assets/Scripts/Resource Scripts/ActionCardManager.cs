using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCardManager : MonoBehaviour
{
    public List<ActionCard> actionCardList = new List<ActionCard>();

    public void AddActionCard(ActionCard card)
    {
        actionCardList.Add(card);
    }

    public void ApplyAllActionCards()
    {
        foreach (var card in actionCardList)
        {
            Debug.Log("Applying card: " + card.cardName);
            // Apply effects to resources here
        }
    }

    public List<ActionCard> GetActionCardList()
    {
        return actionCardList;
    }
}
