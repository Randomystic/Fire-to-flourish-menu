using UnityEngine;

[CreateAssetMenu(fileName = "TownResources", menuName = "FTF/Town Resources")]
public class TownResourceList : ScriptableObject
{
    public int provisions = 12;
    public int education = 20;
    public int firefightingEquipment = 2;
    public int fireSafetyRating = 0;
    public int windSpeed = 2;
    public int temperatureSeason = 2;
    public int happiness = 15;

    public float averageFuelLoad = 2f;


    [SerializeField, HideInInspector] private int oldProvisions;
    [SerializeField, HideInInspector] private int oldEducation;
    [SerializeField, HideInInspector] private int oldFirefightingEquipment;
    [SerializeField, HideInInspector] private int oldFireSafetyRating;
    [SerializeField, HideInInspector] private int oldWindSpeed;
    [SerializeField, HideInInspector] private int oldTemperatureSeason;
    [SerializeField, HideInInspector] private int oldHappiness;
    [SerializeField, HideInInspector] private float oldAverageFuelLoad;

    public float CalculateFireSafety()
    {
        float result =
            20f +
            (averageFuelLoad * 10f) +
            (2f * (temperatureSeason * windSpeed)) -
            (2f * (education / 10f)) -
            (2f * firefightingEquipment);

        fireSafetyRating = Mathf.Clamp(Mathf.RoundToInt(result), 0, 100);
        return fireSafetyRating;
    }

    public void AdjustProvisions(int amount)
    {
        provisions = Mathf.Clamp(provisions + amount, 0, 25);
    }

    public void AdjustEducation(int amount)
    {
        education = Mathf.Clamp(education + amount, 0, 50);
    }

    public void AdjustHappiness(int amount)
    {
        happiness = Mathf.Clamp(happiness + amount, 0, 25);
    }

    public void AdjustFireFightingEquipment(int amount)
    {
        firefightingEquipment = Mathf.Clamp(firefightingEquipment + amount, 0, 5);
    }

    public void AdjustWindSpeed(int amount)
    {
        windSpeed = Mathf.Clamp(windSpeed + amount, 1, 4);
    }

    public void AdjustTemperatureSeason(int amount)
    {
        temperatureSeason = Mathf.Clamp(temperatureSeason + amount, 1, 5);
    }

    public void AdjustFireSafetyRating(int amount)
    {
        fireSafetyRating = Mathf.Clamp(fireSafetyRating + amount, 0, 100);
    }


    public void ResetToDefaults()
    {
        provisions = 12;
        education = 20;
        firefightingEquipment = 2;
        fireSafetyRating = 0;
        windSpeed = 2;
        temperatureSeason = 2;
        happiness = 15;
        averageFuelLoad = 2f;
    }

    public void GetOldValuesBeforeTurn()
    {
        oldProvisions = provisions;
        oldEducation = education;
        oldFirefightingEquipment = firefightingEquipment;
        oldFireSafetyRating = fireSafetyRating;
        oldWindSpeed = windSpeed;
        oldTemperatureSeason = temperatureSeason;
        oldHappiness = happiness;
    }

    public string GetResourceSummary()
    {

        return $"Provisions: {oldProvisions} -> {provisions}\n" +
               $"Education: {oldEducation} -> {education}\n" +
               $"Firefighting Equipment: {oldFirefightingEquipment} -> {firefightingEquipment}\n" +
               $"Fire Safety Rating: {oldFireSafetyRating} -> {fireSafetyRating}\n" +
               $"Wind Speed: {oldWindSpeed} -> {windSpeed}\n" +
               $"Temperature/Season: {oldTemperatureSeason} -> {temperatureSeason}\n" +
               $"Happiness: {oldHappiness} -> {happiness}";
    }
}
