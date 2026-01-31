// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class ReflectionPhase : MonoBehaviour
// {
//     [Header("References")]
//     public TownResourceList townResources;
//     public Map map;
//     public TMP_Text reflectionText;
//     public TMP_Text reflectionText2;
//     public TMP_Text reflectionText3;

//     void Start()
//     {
//         map = Map.Instance;
//         if (map == null)
//         {
//             Debug.LogWarning("No Map instance found — loading from Resources if available.");
//             var prefab = Resources.Load<GameObject>("Map");
//             if (prefab)
//             {
//                 var mapObj = Instantiate(prefab);
//                 mapObj.name = "PersistentMap";
//                 map = mapObj.GetComponent<Map>();
//                 map.EnsureInitialized();
//             }
//             else
//             {
//                 Debug.LogError("ReflectionPhase: Map prefab not found in Resources/Map!");
//                 return;
//             }
//         }

//         if (!townResources)
//         {
//             townResources = Resources.Load<TownResourceList>("TownResources");
//             if (!townResources)
//             {
//                 Debug.LogError("ReflectionPhase: TownResources not found in Resources/");
//                 return;
//             }
//         }

//         GenerateReflectionReport();
//     }

//     void GenerateReflectionReport()
//     {
//         float avgFuelLoad = townResources.averageFuelLoad;
//         float avgMoney = 0, avgMorale = 0, avgRespect = 0;
//         int playerCount = Player.allPlayers.Count;

//         foreach (var kv in Player.allPlayers)
//         {
//             avgMoney += kv.Value.resources.money;
//             avgMorale += kv.Value.resources.morale;
//             avgRespect += kv.Value.resources.respect;
//         }

//         if (playerCount > 0)
//         {
//             avgMoney /= playerCount;
//             avgMorale /= playerCount;
//             avgRespect /= playerCount;
//         }

//         bool indigenousBurned = CheckIndigenousDamage();

//         // Normalized components (0–1 range)
//         float provisionsNorm = townResources.provisions / 25f;
//         float educationNorm = townResources.education / 50f;
//         float happinessNorm = townResources.happiness / 25f;
//         float fireSafetyNorm = townResources.fireSafetyRating / 100f;
//         float firefightingNorm = townResources.firefightingEquipment / 5f;
//         float moraleNorm = avgMorale / 25f;
//         float respectNorm = avgRespect / 20f;
//         float moneyNorm = Mathf.Clamp01(avgMoney / 100f); // assume 100 baseline for scaling
//         float culturalNorm = indigenousBurned ? 0f : 1f;

//         // Weighted total
//         float finalScore =
//             (provisionsNorm * 0.1f +
//              educationNorm * 0.1f +
//              happinessNorm * 0.1f +
//              fireSafetyNorm * 0.2f +
//              firefightingNorm * 0.1f +
//              moraleNorm * 0.1f +
//              respectNorm * 0.1f +
//              moneyNorm * 0.1f +
//              culturalNorm * 0.1f) * 100f;

//         // Clamp and round
//         finalScore = Mathf.Clamp(finalScore, 0f, 100f);
//         string grade = CalculateGrade(finalScore);


//         // Compose report text
//         if (reflectionText && reflectionText2 && reflectionText3) 
//         {
//             reflectionText.text =
//                 $"FINAL REFLECTION REPORT\n\n" +
//                 $"Town Resources:\n" +
//                 $"- Provisions: {townResources.provisions}/25\n" +
//                 $"- Education: {townResources.education}/50\n" +
//                 $"- Happiness: {townResources.happiness}/25\n" +
//                 $"- Fire Safety Rating: {townResources.fireSafetyRating}/100\n" +
//                 $"- Firefighting Equipment: {townResources.firefightingEquipment}/5\n\n";

//             reflectionText2.text =
//                 $"Player Averages:\n" +
//                 $"- Money: {avgMoney:F1}\n" +
//                 $"- Morale: {avgMorale:F1}/25\n" +
//                 $"- Respect: {avgRespect:F1}/20\n\n" +

//                 $"Map Averages:\n" +
//                 $"- Average Fuel Load: {avgFuelLoad:F2}\n" +
//                 $"- Indigenous Land Burned: {(indigenousBurned ? "Yes" : "No")}\n\n";

//             reflectionText3.text =
//                 $"Final Grade: {grade}, Score: {finalScore:F1}/100\n\n" +

//                 (finalScore >= 60f
//                     ? "You have successfully managed your community, maintaining stability and morale through adversity."
//                     : "Your community struggled to recover - critical systems failed to maintain balance and safety.");
//         }

//         Debug.Log($"ReflectionPhase -> Final Grade: {finalScore:F1}/100");
//     }

//     string CalculateGrade(float score)
//     {
//         if (score >= 90) return "S";
//         if (score >= 75) return "A";
//         if (score >= 65) return "B";
//         if (score >= 55) return "C";
//         if (score >= 40) return "D";
//         if (score >= 25) return "E";
//         return "F";
//     }


//     bool CheckIndigenousDamage()
//     {
//         foreach (var kv in map.tiles)
//         {
//             if (kv.Value.tileName.ToLower().Contains("indigenous_land") && kv.Value.burnt)
//                 return true;
//         }
//         return false;
//     }
// }
