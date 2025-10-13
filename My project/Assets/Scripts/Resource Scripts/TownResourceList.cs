using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TownResourceList
{

    public int provisions;
    public int education;
    public int population;

    public int firefightingEquipment;
    public int fireSafetyRating
    ;

    public float CalculateFireSafety()
    {
        // Simple calculation example
        return (fireSafetyRating + firefightingEquipment) * 0.5f;
        
    }


    public void AdjustProvisions(int amount) => provisions += amount;
    public void AdjustEducation(int amount) => education += amount;
    public void AdjustPopulation(int amount) => population += amount;
    public void AdjustFireFightingEquipment(int amount) => firefightingEquipment += amount;

}
