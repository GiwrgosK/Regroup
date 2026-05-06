using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour {
    [Header("Settings Menu Configuration")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;

    [Header("Settings Menu Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    [SerializeField] private AudioClip buttonClickSoundEffect;

    private Resolution[] resolutions;
    private bool isInitialized = false;

    private void OnEnable() {
        InitializeSettings();
    }

    private void InitializeSettings() {
        isInitialized = false;

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        effectsSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.onValueChanged.RemoveAllListeners();
        effectsSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        effectsSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> resOptions = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++) {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resOptions);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
        
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless", "Windowed" });

        int currentWindowModeIndex = 0;
        switch (Screen.fullScreenMode) {
            case FullScreenMode.ExclusiveFullScreen: currentWindowModeIndex = 0; break;
            case FullScreenMode.FullScreenWindow:    currentWindowModeIndex = 1; break;
            case FullScreenMode.Windowed:            currentWindowModeIndex = 2; break;
        }
        
        windowModeDropdown.value = currentWindowModeIndex;
        windowModeDropdown.RefreshShownValue();

        windowModeDropdown.onValueChanged.RemoveAllListeners();
        windowModeDropdown.onValueChanged.AddListener(SetWindowMode);

        isInitialized = true;
    }

    public void SetResolution(int resolutionIndex) {
        if(isInitialized) AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void SetWindowMode(int index) {
        if(isInitialized) AudioManager.Instance.PlayClip(buttonClickSoundEffect);
        switch (index) {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }
}