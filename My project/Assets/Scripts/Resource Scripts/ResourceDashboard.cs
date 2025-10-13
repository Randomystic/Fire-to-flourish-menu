using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDashboard : MonoBehaviour
{
    public Map map; // Assuming you have a Map class
    public List<Player> players = new List<Player>();
    public TownResourceList townResource;

    public object GetDashboardContent()
    {
        return new
        {
            Provisions = townResource.provisions,
            Education = townResource.education,
            Population = townResource.population
        };
    }
}
 
