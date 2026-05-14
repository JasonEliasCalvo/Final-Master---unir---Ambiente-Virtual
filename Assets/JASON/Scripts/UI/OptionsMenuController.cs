using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown;
    public Slider volumeSlider, musicSlider, sfxSlider, voiceSlider;

    [SerializeField] private AudioMixer masterMixer;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string VoiceVolumeKey = "VoiceVolume";

    [SerializeField] private PlayerController player;
    [SerializeField] private Slider sensXSlider, sensYSlider;

    // Valores por defecto de sensibilidad
    private const float DefaultSensX = 0.02f;
    private const float DefaultSensY = 0.02f;

    private const string SensXKey = "SensitivityX";
    private const string SensYKey = "SensitivityY";

    void Start()
    {
        InitializeOptions();

        //qualityDropdown.onValueChanged.AddListener(SetQuality);

        sensXSlider.minValue = DefaultSensX * 0.5f; 
        sensXSlider.maxValue = DefaultSensX * 2f;

        sensYSlider.minValue = DefaultSensY * 0.5f;
        sensYSlider.maxValue = DefaultSensY * 2f;

        // Listeners
        sensXSlider.onValueChanged.AddListener(SetSensitivityX);
        sensYSlider.onValueChanged.AddListener(SetSensitivityY);

        // Cargar valores guardados o usar los predeterminados
        float sensX = PlayerPrefs.GetFloat(SensXKey, DefaultSensX);
        float sensY = PlayerPrefs.GetFloat(SensYKey, DefaultSensY);

        sensXSlider.value = sensX;
        sensYSlider.value = sensY;

        if (player != null)
            player.sensitivity = new Vector2(sensX, sensY);


        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
    }

    private void SetSensitivityX(float value)
    {
        if (player != null)
            player.sensitivity = new Vector2(value, player.sensitivity.y);

        PlayerPrefs.SetFloat(SensXKey, value);
    }

    private void SetSensitivityY(float value)
    {
        if (player != null)
            player.sensitivity = new Vector2(player.sensitivity.x, value);

        PlayerPrefs.SetFloat(SensYKey, value);
    }

    private void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Baja", "Media", "Alta" });
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    private float NormalizeToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
    }

    private void SetMasterVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    private void SetMusicVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    private void SetSFXVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    private void SetVoiceVolume(float volume)
    {
        float db = NormalizeToDecibels(volume);
        masterMixer.SetFloat("VoiceVolume", db);
        PlayerPrefs.SetFloat(VoiceVolumeKey, volume);
    }

    private void InitializeOptions()
    {
        // Valores predeterminados
        float defaultMasterVol = 1.0f;
        float defaultMusicVol = 0.0f; 
        float defaultVoiceVol = 0.5f; 
        float defaultSfxVol = 0.5f;   

        float masterVol = PlayerPrefs.HasKey(MasterVolumeKey) ? PlayerPrefs.GetFloat(MasterVolumeKey) : defaultMasterVol;
        masterMixer.SetFloat("MasterVolume", NormalizeToDecibels(masterVol));
        volumeSlider.value = masterVol;

        float musicVol = PlayerPrefs.HasKey(MusicVolumeKey) ? PlayerPrefs.GetFloat(MusicVolumeKey) : defaultMusicVol;
        masterMixer.SetFloat("MusicVolume", NormalizeToDecibels(musicVol));
        musicSlider.value = musicVol;

        float sfxVol = PlayerPrefs.HasKey(SFXVolumeKey) ? PlayerPrefs.GetFloat(SFXVolumeKey) : defaultSfxVol;
        masterMixer.SetFloat("SFXVolume", NormalizeToDecibels(sfxVol));
        sfxSlider.value = sfxVol;

        float voiceVol = PlayerPrefs.HasKey(SFXVolumeKey) ? PlayerPrefs.GetFloat(SFXVolumeKey) : defaultVoiceVol;
        masterMixer.SetFloat("VoiceVolume", NormalizeToDecibels(voiceVol));
        voiceSlider.value = voiceVol;
    }
}
