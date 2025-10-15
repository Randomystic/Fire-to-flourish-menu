using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionCard : Card
{
    public List<Effect> effects = new List<Effect>();

    public void ApplyAllEffects()
    {
        foreach (var effect in effects)
            effect.ApplyEffect();
    }

    // public ActionCard Clone()
    // {
    //     var copy = new ActionCard
    //     {
    //         id = id,
    //         name = name,
    //         description = description,
    //         user = null,
    //         target = null,
    //         effects = new List<Effect>(effects.Count)
    //     };
        
    //     return copy;
    // }

}

public enum EffectType { RESOURCE, TILE }

[System.Serializable]
public class Effect
{
    public EffectType effectType;

    public void ApplyEffect()
    {
        // TO DO: implement effect
    }
}

[System.Serializable]

public class Card
{
    public string name;     //name: string
    public Player user;     //user: Player
    public Player target;   //target?: Player (null = optional)

    public string getName() => name;
    public string setName(string newName) { name = newName; return name; }

    public Player getUser() => user;
    public void setUser(Player newUser) { user = newUser; }
}


public static class RoleCards
{
    public static Dictionary<RoleType, List<string>> roleCardsDict = new Dictionary<RoleType, List<string>>()
    {
        { RoleType.CIVILIAN, new List<string>
            {
                "community_cleanup",
                "local_story_night",
                "prepare_evacuation_plans",
                "social_media_campaign",
                "support_a_neighbour",
                "volunteer_for_fire_services"
            }
        },
        { RoleType.PRIMARY_SCHOOL_TEACHER, new List<string>
            {
                "classroom_fire_safety_workshop",
                "fire_safety_workshop",
                "support_bushfire_victims"
            }
        },
        { RoleType.CATTLE_FARMER, new List<string>
            {
                "buy_private_firefighting_equipment",
                "participate_in_emergency_planning",
                "sell_farm_goods"
            }
        },
        { RoleType.INDIGENOUS_FARMER, new List<string>
            {
                "cultural_burning",
                "participate_in_emergency_planning",
                "resist_burning_of_sacred_area"
            }
        },
        { RoleType.LGA_MAYOR, new List<string>
            {
                "advocate_for_funding",
                "build_firebreak_road",
                "clear_public_green_space"
            }
        },
        { RoleType.RFS_MEMBER, new List<string>
            {
                "conduct_backburning_program",
                "participate_in_emergency_planning",
                "request_firefighting_equipment"
            }
        }
    };
}
