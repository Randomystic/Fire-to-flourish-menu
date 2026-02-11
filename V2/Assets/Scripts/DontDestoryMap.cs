using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class DontDestroyMap : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] private string mainMapSceneName = "MainMap";

    private void Awake()
    {
        // Prevent duplicate persistent Maps
        var existing = GameObject.Find(gameObject.name);
        if (existing != null && existing != gameObject && existing.scene.name != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Apply visibility for whatever scene we start in
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

        // Hide/show visuals
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = shouldBeVisible;

        // Optional: prevent interactions in non-MainMap scenes
        foreach (var c in GetComponentsInChildren<Collider2D>(true))
            c.enabled = shouldBeVisible;
    }
}
