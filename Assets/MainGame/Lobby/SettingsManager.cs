using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Defaults")]
    public float defaultGrassDistance = 100f;

    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;

        ApplySettings();
    }

    public void ApplySettings()
    {
        ApplyResolution();
    }

    void ApplyResolution()
    {
        if (!PlayerPrefs.HasKey("ResolutionIndex"))
            return;

        Resolution[] resolutions = Screen.resolutions;
        int index = PlayerPrefs.GetInt("ResolutionIndex");

        if (index < 0 || index >= resolutions.Length)
            return;

        Resolution r = resolutions[index];
        Screen.SetResolution(
            r.width,
            r.height,
            FullScreenMode.FullScreenWindow
        );
    }

    public void quitApplication()
    {
        Application.Quit();
    }
}
