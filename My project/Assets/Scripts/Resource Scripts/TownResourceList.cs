using UnityEngine;

[CreateAssetMenu(fileName = "TownResources", menuName = "FTF/Town Resources")]
public class TownResourceList : ScriptableObject
{
    public int provisions = 12;
    public int education = 20;
    // public int population = 4000;
    public int firefightingEquipment = 2;
    public int fireSafetyRating = 0;
    public int windSpeed = 2;
    public int temperatureSeason = 2;
    public int happiness = 15;

    public float averageFuelLoad = 2f;

    public float CalculateFireSafety()
    {
        float result =
            20f +
            (averageFuelLoad * 10f) +
            (2f * (temperatureSeason * windSpeed)) -
            (2f * (education / 10f)) -
            (2f * firefightingEquipment);

        fireSafetyRating = Mathf.RoundToInt(result);
        return fireSafetyRating;
    }

    public void AdjustProvisions(int amount) => provisions += amount;
    public void AdjustEducation(int amount) => education += amount;
    // public void AdjustPopulation(int amount) => population += amount;
    public void AdjustFireFightingEquipment(int amount) => firefightingEquipment += amount;
    public void AdjustWindSpeed(int amount) => windSpeed += amount;
    public void AdjustTemperatureSeason(int amount) => temperatureSeason += amount;
    public void AdjustHappiness(int amount) => happiness += amount;
}
