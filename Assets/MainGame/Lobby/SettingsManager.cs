using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Defaults")]
    public float defaultGrassDistance = 100f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ApplySettings();
    }

    public void ApplySettings()
    {
        ApplyResolutionAndFullscreen();
        ApplyGrassRenderDistance();
    }

    void ApplyResolutionAndFullscreen()
    {
        // Default to current native resolution if nothing is saved yet
        int width = PlayerPrefs.GetInt("PrefWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("PrefHeight", Screen.currentResolution.height);
        
        // Default to fullscreen enabled (1)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1; 

        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(width, height, mode);
    }

    void ApplyGrassRenderDistance()
    {
        float distance = PlayerPrefs.GetFloat("GrassDistance", defaultGrassDistance);

        // TODO: Apply this value to your terrain or third-party grass solution.
        // Example for Unity Terrain:
        // if (Terrain.activeTerrain != null) { Terrain.activeTerrain.detailObjectDistance = distance; }
        
        Debug.Log($"Applied Grass Distance: {distance}");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}