    using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown;
    public Slider volumeSlider;

    public MainMenu panels;

    private Resolution[] resolutions;
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        int savedResIndex = PlayerPrefs.GetInt("resolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();


        fullscreenDropdown.ClearOptions();
        List<string> fullscreenOptions = new List<string> { "Windowed", "Borderless Windowed", "Fullscreen" };
        fullscreenDropdown.AddOptions(fullscreenOptions);
        int savedFullscreenIndex = PlayerPrefs.GetInt("fullscreenMode", 2);
        fullscreenDropdown.value = savedFullscreenIndex;
        fullscreenDropdown.RefreshShownValue();

        SetFullscreenMode(savedFullscreenIndex);
        SetResolution(savedResIndex);

        volumeSlider.value = PlayerPrefs.GetFloat("volume", 1f);
        AudioListener.volume = volumeSlider.value;

        ApplyDisplaySettings();
    }

public void SetResolution(int index)
{
    PlayerPrefs.SetInt("resolutionIndex", index);
    PlayerPrefs.Save();

    ApplyDisplaySettings();
}

    public void SetFullscreenMode(int index)
    {
        PlayerPrefs.SetInt("fullscreenMode", index);
        PlayerPrefs.Save();

        ApplyDisplaySettings();
    }

private void ApplyDisplaySettings()
{
    int resIndex = PlayerPrefs.GetInt("resolutionIndex", 0);
    int modeIndex = PlayerPrefs.GetInt("fullscreenMode", 2);

    Resolution res = resolutions[Mathf.Clamp(resIndex, 0, resolutions.Length - 1)];
    FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;

    switch (modeIndex)
    {
        case 0:
            mode = FullScreenMode.Windowed;
            break;
        case 1:
            mode = FullScreenMode.FullScreenWindow;
            break;
        case 2:
            mode = FullScreenMode.ExclusiveFullScreen;
            break;
    }

    Screen.SetResolution(res.width, res.height, mode);
    Screen.fullScreenMode = mode;
}

    public void SetVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("volume", v);
        PlayerPrefs.Save();
    }
}
