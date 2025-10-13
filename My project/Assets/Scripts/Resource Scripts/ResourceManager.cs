using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    public TownResourceList townResources;
    public Dictionary<Player, PlayerResourceList> playerResources = new Dictionary<Player, PlayerResourceList>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static ResourceManager GetInstance()
    {
        return instance;
    }

    public void AllocateTownResources(TownResourceList list, int amount)
    {
        list.AdjustProvisions(amount);
    }

    public void AllocatePersonalResource(Player p, PlayerResourceList resource, int amount)
    {
        if (!playerResources.ContainsKey(p))
            playerResources[p] = resource;

        playerResources[p].AdjustMoney(amount);
    }
}
