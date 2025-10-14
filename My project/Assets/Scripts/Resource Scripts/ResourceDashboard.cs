using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDashboard : MonoBehaviour
{
    public Map map;
    public List<Player> players = new List<Player>();
    public TownResourceList townResource;

    public DashboardContent GetDashboardContent()
    {
        if (townResource == null) return new DashboardContent();
        return new DashboardContent
        {
            Provisions = townResource.provisions,
            Education  = townResource.education,
            Population = townResource.population
        };
    }
}

public class DashboardContent
{
    public int Provisions;
    public int Education;
    public int Population;
}
