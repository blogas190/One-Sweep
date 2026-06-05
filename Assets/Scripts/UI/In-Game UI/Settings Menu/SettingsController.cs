using Michsky.UI.Reach;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : BaseMenu
{

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private HorizontalSelector screenMode;

    public bool isSettingsOpen = false;
    private bool isInitializing = false;

    public void SyncSliders()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        masterSlider.SetValueWithoutNotify(AudioManager.Instance.masterVolume);
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
        uiSlider.SetValueWithoutNotify(AudioManager.Instance.uiVolume);
    }

    public void OnMasterVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetMasterVolume(masterSlider.value);
    }

    public void OnMusicVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetMusicVolume(musicSlider.value);
    }

    public void OnSFXVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);
    }

    public void OnUIVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetUIVolume(uiSlider.value);
    }

    public void SetScreenMode(int screenMode)
    {
        if (isInitializing) return; // ignore calls during init

        FullScreenMode mode = screenMode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.FullScreenWindow
        };
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt("ScreenMode", screenMode);
    }

    public void OnBack()
    {
        UIManager.instance.SettingsBack();
        //PlayerPrefs.Save();
        //saveManager.SaveGame();
    }

    private void OnEnable()
    {
        isInitializing = true; // block saves during init

        masterSlider.onValueChanged.AddListener(delegate { OnMasterVolumeChanged(); });
        musicSlider.onValueChanged.AddListener(delegate { OnMusicVolumeChanged(); });
        sfxSlider.onValueChanged.AddListener(delegate { OnSFXVolumeChanged(); });
        uiSlider.onValueChanged.AddListener(delegate { OnUIVolumeChanged(); });

        int savedMode = PlayerPrefs.GetInt("ScreenMode", 1);
        screenMode.defaultIndex = savedMode;
        SyncSliders();

        isInitializing = false; // allow saves again
    }

    private void OnDisable()
    {
        masterSlider.onValueChanged.RemoveListener(delegate { OnMasterVolumeChanged(); });
        musicSlider.onValueChanged.RemoveListener(delegate { OnMusicVolumeChanged(); });
        sfxSlider.onValueChanged.RemoveListener(delegate { OnSFXVolumeChanged(); });
        uiSlider.onValueChanged.RemoveListener(delegate { OnUIVolumeChanged(); });

        PlayerPrefs.Save();
    }
}
