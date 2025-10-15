using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ActionCardGenerator
{
    [MenuItem("Tools/Generate Action Cards")]
    public static void GenerateCards()
    {
        string folderPath = "Assets/Cards";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Cards");


        var cards = new Dictionary<string, (RoleType role, string desc, Dictionary<string, int> effects, Dictionary<string, int> civBonus)>
        {
            //CIVILIAN
            ["volunteer_for_fire_services"] = (RoleType.CIVILIAN,
                "Aid fire services with tasks such as food prep, water runs, and recovery.",
                new Dictionary<string, int> {
                    { "FirefightingEquipment", 1 },
                    { "Happiness", 5 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            ["prepare_evacuation_plans"] = (RoleType.CIVILIAN,
                "Work with your household and neighbours to create a clear, rehearsed bushfire evacuation plan.",
                new Dictionary<string, int> {
                    { "Education", 10 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            ["support_a_neighbour"] = (RoleType.CIVILIAN,
                "Provide emotional or logistical support to a neighbour struggling with fire anxiety or displacement.",
                new Dictionary<string, int> {
                    { "Happiness", 5 },
                    { "Respect", 5 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            ["community_cleanup"] = (RoleType.CIVILIAN,
                "Help clear debris or repair public areas to improve community spaces.",
                new Dictionary<string, int> {
                    { "Respect", 5 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            ["local_story_night"] = (RoleType.CIVILIAN,
                "Organise an event where residents share fire stories to strengthen local bonds.",
                new Dictionary<string, int> {
                    { "Happiness", 5 },
                    { "Respect", 5 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            ["social_media_campaign"] = (RoleType.CIVILIAN,
                "Promote fire safety events online to raise community awareness.",
                new Dictionary<string, int> {
                    { "Morale", 10 }
                },
                new Dictionary<string, int> {
                    { "Morale", 10 }
                }),

            //LGA MAYOR
            ["build_firebreak_road"] = (RoleType.LGA_MAYOR,
                "Construct a wide road to act as a fire barrier through residential areas.",
                new Dictionary<string, int> {
                    { "Money", -3 },
                    { "FuelLoad", -15 },
                    { "Respect", -5 }
                },
                new Dictionary<string, int> {
                    { "Happiness", 5 }
                }),

            ["clear_public_green_space"] = (RoleType.LGA_MAYOR,
                "Demolish a very high risk area of greenery to reduce fuel load.",
                new Dictionary<string, int> {
                    { "Money", -2 },
                    { "FuelLoad", -10 },
                    { "Respect", -5 }
                },
                new Dictionary<string, int> {
                    { "Happiness", 5 }
                }),

            ["advocate_for_funding"] = (RoleType.LGA_MAYOR,
                "Petition the government for additional funds to prepare for bushfire season.",
                new Dictionary<string, int> {
                    { "Money", 3 },
                    { "Respect", 10 }
                },
                new Dictionary<string, int> {
                    { "Happiness", 5 }
                }),

            // PRIMARY SCHOOL TEACHER
            ["fire_safety_workshop"] = (RoleType.PRIMARY_SCHOOL_TEACHER,
                "Run fire safety workshops across schools to educate children.",
                new Dictionary<string, int> {
                    { "Education", 10 }
                },
                new Dictionary<string, int> {
                    { "Education", 5 }
                }),

            ["classroom_fire_safety_workshop"] = (RoleType.PRIMARY_SCHOOL_TEACHER,
                "Educate kids about fire safety and encourage family discussion.",
                new Dictionary<string, int> {
                    { "Education", 10 },
                    { "Happiness", 5 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Education", 5 }
                }),

            ["support_bushfire_victims"] = (RoleType.PRIMARY_SCHOOL_TEACHER,
                "Connect affected children with social services for trauma support.",
                new Dictionary<string, int> {
                    { "Happiness", 5 },
                    { "Respect", 5 }
                },
                new Dictionary<string, int> {
                    { "Education", 5 }
                }),

            // RFS MEMBER
            ["participate_in_emergency_planning"] = (RoleType.RFS_MEMBER,
                "Coordinate emergency plans with other services and local groups.",
                new Dictionary<string, int> {
                    { "Education", 3 },
                    { "Respect", 5 }
                },
                new Dictionary<string, int> {
                    { "Education", 5 }
                }),

            ["conduct_backburning_program"] = (RoleType.RFS_MEMBER,
                "Conduct controlled burning in high risk areas to reduce fuel load.",
                new Dictionary<string, int> {
                    { "Money", -2 },
                    { "FuelLoad", -10 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "FuelLoad", -5 }
                }),

            ["request_firefighting_equipment"] = (RoleType.RFS_MEMBER,
                "Request more firefighting equipment from authorities.",
                new Dictionary<string, int> {
                    { "FirefightingEquipment", 1 },
                    { "FuelLoad", -10 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "FuelLoad", -5 }
                }),

            // CATTLE FARMER
            ["sell_farm_goods"] = (RoleType.CATTLE_FARMER,
                "Harvest and sell a portion of your crop yield to markets and buyers.",
                new Dictionary<string, int> {
                    { "Provisions", -10 },
                    { "Money", 2 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Morale", 5 }
                }),

            ["buy_private_firefighting_equipment"] = (RoleType.CATTLE_FARMER,
                "Invest in firefighting gear such as a water tank, pump, and hose rig.",
                new Dictionary<string, int> {
                    { "Money", -1 },
                    { "FirefightingEquipment", 1 },
                    { "Morale", 5 }
                },
                new Dictionary<string, int> {
                    { "Education", 5 }
                }),

            // INDIGENOUS FARMER
            ["cultural_burning"] = (RoleType.INDIGENOUS_FARMER,
                "Use traditional cultural burning practices in high risk areas.",
                new Dictionary<string, int> {
                    { "FuelLoad", -15 },
                    { "Morale", 5 },
                    { "Happiness", 5 }
                },
                new Dictionary<string, int> {
                    { "Happiness", 5 }
                }),

            ["resist_burning_of_sacred_area"] = (RoleType.INDIGENOUS_FARMER,
                "Block the burning of a sacred area proposed by another player.",
                new Dictionary<string, int> {
                    { "Happiness", 10 },
                    { "Morale", 10 },
                    { "Respect", 5 }
                },
                new Dictionary<string, int> {
                    { "Happiness", 5 }
                })
        };




        foreach (var kvp in cards)
        {
            var card = ScriptableObject.CreateInstance<ActionCardData>();
            card.cardName = kvp.Key;
            card.role = kvp.Value.role;
            card.actionDescription = kvp.Value.desc;

            card.effects = new List<ResourceEffect>();
            foreach (var e in kvp.Value.effects)
                card.effects.Add(new ResourceEffect { resourceName = e.Key, value = e.Value });

            card.civilianBonus = new List<ResourceEffect>();
            foreach (var c in kvp.Value.civBonus)
                card.civilianBonus.Add(new ResourceEffect { resourceName = c.Key, value = c.Value });

            string safeName = kvp.Key.Replace(" ", "_");
            AssetDatabase.CreateAsset(card, $"{folderPath}/{safeName}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Action cards generated!");
    }
}
