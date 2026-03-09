using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    void Start()
    {
        SetupResolutions();
        LoadSettings();
    }

    [Header("Grass")]
    public Slider grassDistanceSlider;
    public TMP_Text grassDistanceText;

    public void SetGrassDistance()
    {
        grassDistanceText.text = "Grass Render Distance: " + grassDistanceSlider.value.ToString("F0");
        PlayerPrefs.SetInt("GrassDistance", (int)grassDistanceSlider.value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";

            // Only add unique width x height
            bool exists = false;
            foreach (var r in uniqueResolutions)
            {
                if (r.width == resolutions[i].width && r.height == resolutions[i].height)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                options.Add(option);
                uniqueResolutions.Add(resolutions[i]);

                // Check if this is the current resolution
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = uniqueResolutions.Count - 1;
                }
            }
        }

        // Replace resolutions with filtered list
        resolutions = uniqueResolutions.ToArray();

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("GrassDistance", (int)grassDistanceSlider.value);
        PlayerPrefs.Save();

        SettingsManager.Instance.ApplySettings();
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int index = PlayerPrefs.GetInt("ResolutionIndex");
            resolutionDropdown.value = index;
            SetResolution(index);
        }

        int grassDistance = PlayerPrefs.GetInt("GrassDistance", 4);
        grassDistanceSlider.value = grassDistance;
        grassDistanceText.text = "Grass Render Distance: " + grassDistance.ToString("F0");
    }

}

