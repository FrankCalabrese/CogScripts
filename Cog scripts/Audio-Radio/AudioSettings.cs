using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public static float masterVol = 1;
    public static float sfxVol = 1;
    public static float bgmVol = 1;
    public static float radioVol = 1;
    public static float ambientVol = 1;

    [SerializeField] Slider masterSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider radioSlider;
    [SerializeField] Slider ambientSlider;

    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AudioManager.instance != null);

        if (SavingLoadingManager.instance.GetHasSettingsSaved())
        {
            LoadAudioSettings();
        }
    }
    private void OnEnable()
    {
        if (SavingLoadingManager.instance.GetHasSettingsSaved())
        {
            LoadAudioSettings();
        }
    }

    void Update()
    {
        masterSlider.value = masterVol;
        sfxSlider.value = sfxVol;
        bgmSlider.value = bgmVol;
        radioSlider.value = radioVol;
        ambientSlider.value = ambientVol;
    }

    public void MasterVolSlider(float volume)
    {
        AudioManager.instance.MasterVolume = volume;
        masterVol = volume;
        ApplyRadioChanges();
        SaveAudioSettings();
    }
    public void RadioVolSlider(float volume)
    {
        AudioManager.instance.RadioVolume = volume;
        radioVol = volume;
        ApplyRadioChanges();
        SaveAudioSettings();
    }
    public void BGMVolSlider(float volume)
    {
        AudioManager.instance.MusicVolume = volume;
        bgmVol = volume;
        ApplyRadioChanges();
        SaveAudioSettings();
    }
    public void SFXVolSlider(float volume)
    {
        AudioManager.instance.SfxVolume = volume;
        sfxVol = volume;
        ApplyRadioChanges();
        SaveAudioSettings();
    }
    public void AmbientVolSlider(float volume)
    {
        AudioManager.instance.AmbienceVolume = volume;
        ambientVol = volume;
        SaveAudioSettings();
    }

    public void SaveAudioSettings()
    {
        SavingLoadingManager.instance.Save<float>("masterVol", masterVol);
        SavingLoadingManager.instance.Save<float>("sfxVol", sfxVol);
        SavingLoadingManager.instance.Save<float>("bgmVol", bgmVol);
        SavingLoadingManager.instance.Save<float>("radioVol", radioVol);
        SavingLoadingManager.instance.Save<float>("ambientVol", ambientVol);
    }
    public void LoadAudioSettings()
    {
        masterVol = SavingLoadingManager.instance.Load<float>("masterVol");
        sfxVol = SavingLoadingManager.instance.Load<float>("sfxVol");
        bgmVol = SavingLoadingManager.instance.Load<float>("bgmVol");
        radioVol = SavingLoadingManager.instance.Load<float>("radioVol");
        ambientVol = SavingLoadingManager.instance.Load<float>("ambientVol");

        masterSlider.value = masterVol;
        sfxSlider.value = sfxVol;
        bgmSlider.value = bgmVol;
        radioSlider.value = radioVol;
        ambientSlider.value = ambientVol;

        ApplyRadioChanges();
    }

    //send 
    private void ApplyRadioChanges()
    {
        if(FindObjectOfType<RadioDial>() != null)
        {
            RadioDial[] dials = FindObjectsOfType<RadioDial>();
            for (int i = 0; i < FindObjectsOfType<RadioDial>().Length; i++)
            {
                dials[i].SetAudioSettingsValues();
            }
        }
    }
    

}
