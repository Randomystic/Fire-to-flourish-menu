using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerResourceList
{
    public float money = 0;
    public int morale = 0;
    public int respect = 0;

    public void AdjustMorale(int amount)
    {
        morale = Mathf.Clamp(morale + amount, 0, 25);
    }

    public void AdjustRespect(int amount)
    {
        respect = Mathf.Clamp(respect + amount, 0, 20);
    }

    public void AdjustMoney(int amount)
    {
        money = Mathf.Max(money + amount, 0);
    }

    public void UpdateAllResources()
    {
        // Implement global updates here
    }
}
