using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    public TownResourceList townResources;
    private readonly Dictionary<Player, PlayerResourceList> playerResources = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureExists()
    {
        if (instance == null)
        {
            var go = new GameObject("ResourceManager");
            var rm = go.AddComponent<ResourceManager>();

            rm.townResources = Resources.Load<TownResourceList>("TownResources");
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // fallback if not set via Resources.Load or Inspector
        if (townResources == null)
            townResources = Resources.Load<TownResourceList>("TownResources");
    }

    public static ResourceManager GetInstance() => instance;

    // public void AllocateTownResources(TownResourceList list, int amount)
    // {
    //     if (list == null) return;
    //     list.AdjustProvisions(amount);
    // }

    // public PlayerResourceList GetOrCreatePlayerResources(Player p)
    // {
    //     if (p == null) return null;
    //     if (!playerResources.TryGetValue(p, out var pr))
    //     {
    //         pr = new PlayerResourceList();
    //         playerResources[p] = pr;
    //     }
    //     return pr;
    // }

    // public void AllocatePersonalResource(Player p, int moneyAmount)
    // {
    //     var pr = GetOrCreatePlayerResources(p);
    //     if (pr == null) return;
    //     pr.AdjustMoney(moneyAmount);
    // }

    // public void RemovePlayer(Player p) => playerResources.Remove(p);


}
