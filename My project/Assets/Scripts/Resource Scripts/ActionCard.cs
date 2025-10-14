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
