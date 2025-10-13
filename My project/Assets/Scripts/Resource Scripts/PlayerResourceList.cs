using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerResourceList
{
    public float money;
    public int morale;
    public int respect;

    public void AdjustMoney(float amount) => money += amount;
    public void AdjustMorale(int amount) => morale += amount;
    public void AdjustRespect(int amount) => respect += amount;

    public void UpdateAllResources()
    {
        // Implement global updates here
    }
}
