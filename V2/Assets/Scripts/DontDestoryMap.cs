using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyMap : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMapSceneName = "MainMap";

    [Header("Auto Load Settings")]
    [SerializeField] private string mapPrefabResourcesPath = "Map"; 
    // Put Map prefab inside: Assets/Resources/Map.prefab

    private static DontDestroyMap instance;

    private void Awake()
    {
        // Singleton protection
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyVisibility(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplyVisibility(newScene.name);
    }

    private void ApplyVisibility(string sceneName)
    {
        bool shouldBeVisible = sceneName == mainMapSceneName;

        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = shouldBeVisible;

        foreach (var c in GetComponentsInChildren<Collider2D>(true))
            c.enabled = shouldBeVisible;
    }


    // Auto-load Map if scene starts without it

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureMapExists()
    {
        if (instance != null) return;

        DontDestroyMap existing = FindObjectOfType<DontDestroyMap>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("Map");
        if (prefab == null)
        {
            Debug.LogError("[DontDestroyMap] Could not find Map prefab in Resources/Map");
            return;
        }

        GameObject mapInstance = Instantiate(prefab);
        instance = mapInstance.GetComponent<DontDestroyMap>();

        if (instance == null)
        {
            Debug.LogError("[DontDestroyMap] Map prefab is missing DontDestroyMap component.");
        }
    }
}
