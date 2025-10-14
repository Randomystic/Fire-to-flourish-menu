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
    


    private void takeTurn()
    {
        // Logic for taking a turn
    }

    public void useActionCard(ActionCard card)
    {
        Debug.Log(name + " is using card: " + card.name);
        // Apply card effects to resources here
    }
}