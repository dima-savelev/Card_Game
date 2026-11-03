using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using TMPro;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown resolurionDropdown;
    Resolution[] resolutions;
    public Canvas canvas;
    public Slider slider;
    private AudioSource audioSrc;
    private float musicVolume = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        audioSrc= canvas.GetComponent<AudioSource>();
        resolurionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " " + resolutions[i].refreshRateRatio + "Hz";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolurionDropdown.AddOptions(options);
        resolurionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
    }
    private void Update()
    {
        audioSrc.volume = musicVolume;
    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen= isFullscreen;
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionPreference", resolurionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.SetFloat("Volume", musicVolume);
    }
    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("ResolutionPreference"))
            resolurionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference");
        else
            resolurionDropdown.value = currentResolutionIndex;
        if (PlayerPrefs.HasKey("FullscreenPreference"))
            Screen.fullScreen = System.Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        else
            Screen.fullScreen = true;
        if (PlayerPrefs.HasKey("Volume"))
        {
            musicVolume = PlayerPrefs.GetFloat("Volume");
            slider.value = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            musicVolume = 0.1f;
            slider.value = 0.1f;
        }
    }
    public void SetVolume(float volume)
    {
        musicVolume = volume;
    }
}
