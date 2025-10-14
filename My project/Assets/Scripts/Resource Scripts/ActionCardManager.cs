using System.Collections.Generic;
using UnityEngine;

public class ActionCardManager : MonoBehaviour
{
    public List<ActionCard> actionCardList = new List<ActionCard>();

    public void AddActionCard(ActionCard card) => actionCardList.Add(card);

    public void ApplyAllActionCards()
    {
        foreach (var card in actionCardList)
        {
            Debug.Log("Applying card: " + card.getName());   // ← was card.cardName
            card.ApplyAllEffects();
        }
    }

    public List<ActionCard> GetActionCardList() => actionCardList;
}
