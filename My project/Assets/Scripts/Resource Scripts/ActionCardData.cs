using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewActionCard", menuName = "Cards/ActionCard")]
public class ActionCardData : ScriptableObject
{
    public string cardName;
    public RoleType role;
    [TextArea] public string actionDescription;

    public List<ResourceEffect> effects = new List<ResourceEffect>();
    public List<ResourceEffect> civilianBonus = new List<ResourceEffect>();
}

[System.Serializable]
public class ResourceEffect
{
    public string resourceName;
    public int value;
}
 