using UnityEngine;

[CreateAssetMenu(fileName = "TownResources", menuName = "FTF/Town Resources")]
public class TownResourceList : ScriptableObject
{
    public int provisions = 12;
    public int education = 20;
    public int population = 4000;
    public int firefightingEquipment = 2;
    public int fireSafetyRating = 60;
    public int windSpeed = 2;
    public int temperatureSeason = 2;

    public float CalculateFireSafety() => (fireSafetyRating + firefightingEquipment) * 0.5f;

    public void AdjustProvisions(int amount) => provisions += amount;
    public void AdjustEducation(int amount) => education += amount;
    public void AdjustPopulation(int amount) => population += amount;
    public void AdjustFireFightingEquipment(int amount) => firefightingEquipment += amount;
    public void AdjustWindSpeed(int amount) => windSpeed += amount;
    public void AdjustTemperatureSeason(int amount) => temperatureSeason += amount;
}
