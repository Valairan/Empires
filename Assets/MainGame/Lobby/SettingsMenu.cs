using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    private List<Resolution> filteredResolutions = new List<Resolution>();

    [Header("Grass")]
    public Slider grassDistanceSlider;
    public TMP_Text grassDistanceText;

    [Header("Fullscreen")]
    public Toggle fullscreenToggle; // Added toggle reference for completeness

    void Start()
    {
        SetupResolutions();
        LoadSettings();
    }

    public void SetGrassDistance()
    {
        grassDistanceText.text = "Grass Render Distance: " + grassDistanceSlider.value.ToString("F0");
    }

    void SetupResolutions()
    {
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            // Filter out duplicates (different refresh rates for same width/height)
            bool exists = false;
            foreach (var r in filteredResolutions)
            {
                if (r.width == allResolutions[i].width && r.height == allResolutions[i].height)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                filteredResolutions.Add(allResolutions[i]);
                options.Add($"{allResolutions[i].width} x {allResolutions[i].height}");

                // Match currently active resolution
                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = filteredResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        // If we saved a resolution previously, let's find its index in our clean list
        if (PlayerPrefs.HasKey("PrefWidth") && PlayerPrefs.HasKey("PrefHeight"))
        {
            int savedWidth = PlayerPrefs.GetInt("PrefWidth");
            int savedHeight = PlayerPrefs.GetInt("PrefHeight");

            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                if (filteredResolutions[i].width == savedWidth && filteredResolutions[i].height == savedHeight)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SaveSettings()
    {
        // 1. Save Resolution by Width and Height
        Resolution selectedRes = filteredResolutions[resolutionDropdown.value];
        PlayerPrefs.SetInt("PrefWidth", selectedRes.width);
        PlayerPrefs.SetInt("PrefHeight", selectedRes.height);

        // 2. Save Grass Distance
        PlayerPrefs.SetFloat("GrassDistance", grassDistanceSlider.value);

        // 3. Save Fullscreen (1 for true, 0 for false)
        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        }

        PlayerPrefs.Save();

        // Tell the manager to apply everything right now
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplySettings();
        }
    }

    void LoadSettings()
    {
        // Load Grass Distance
        float defaultGrass = SettingsManager.Instance != null ? SettingsManager.Instance.defaultGrassDistance : 100f;
        float grassDistance = PlayerPrefs.GetFloat("GrassDistance", defaultGrass);
        grassDistanceSlider.value = grassDistance;
        grassDistanceText.text = "Grass Render Distance: " + grassDistance.ToString("F0");

        // Load Fullscreen Toggle UI state
        if (fullscreenToggle != null && PlayerPrefs.HasKey("Fullscreen"))
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
        }
    }
}