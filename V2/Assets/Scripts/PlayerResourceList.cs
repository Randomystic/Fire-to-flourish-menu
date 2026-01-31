using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerResourceList
{
    public float money = 0; 

    public void AdjustMoney(int amount)
    {
        money = Mathf.Max(money + amount, 0);
    }

}
