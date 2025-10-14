using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    public TownResourceList townResources;

    private readonly Dictionary<Player, PlayerResourceList> playerResources = new();

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public static ResourceManager GetInstance() => instance;

    public void AllocateTownResources(TownResourceList list, int amount)
    {
        if (list == null) return;
        list.AdjustProvisions(amount);
    }

    public PlayerResourceList GetOrCreatePlayerResources(Player p)
    {
        if (p == null) return null;
        if (!playerResources.TryGetValue(p, out var pr))
        {
            pr = new PlayerResourceList();
            playerResources[p] = pr;
        }
        return pr;
    }

    public void AllocatePersonalResource(Player p, int moneyAmount)
    {
        var pr = GetOrCreatePlayerResources(p);
        if (pr == null) return;
        pr.AdjustMoney(moneyAmount); // int overload handles it
    }

    public void RemovePlayer(Player p) => playerResources.Remove(p);
}
