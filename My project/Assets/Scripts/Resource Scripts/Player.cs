using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoleType
{
    RFS_MEMBER,
    LGA_MAYOR,
    PRIMARY_SCHOOL_TEACHER,
    CATTLE_FARMER,
    INDIGENOUS_FARMER,
    CIVILIAN
}

public class Player : MonoBehaviour
{
    public string playerName;
    public RoleType role;
    public PlayerResourceList resources;
    
    public static Dictionary<RoleType, Player> allPlayers = new();

    
    void Awake()
    {
        if (!allPlayers.ContainsKey(role))
            allPlayers.Add(role, this);
    }


   public void UseActionCard(ActionCardData card)
    {
        Debug.Log($"{playerName} is using card: {card.cardName}");
        foreach (var effect in card.effects)
        {
            ApplyEffect(effect);
        }
    }

    void ApplyEffect(ResourceEffect effect)
    {
        switch (effect.resourceName.ToLower())
        {
            case "money": resources.AdjustMoney(effect.value); break;
            case "morale": resources.AdjustMorale(effect.value); break;
            case "respect": resources.AdjustRespect(effect.value); break;
        }
    }


    public RoleType GetRole()
    {
        return role;
    }

}