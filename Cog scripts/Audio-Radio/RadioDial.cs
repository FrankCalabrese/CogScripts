/* Frank Calabrese
 * RadioDial.cs
 * 3/24/21
 * controls the dials on the radio
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class RadioDial : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] Image myImage;
    [SerializeField] RectTransform rotator;
    [SerializeField] private DialType dial;
    
    enum DialType
    {
        Station,
        Master,
        Radio,
        BGM,
        SFX
    };
    
    private bool isMouseOverObject;
    private bool locked = false;
    private RadioManager radioManager;
    private AudioSettings audioSettings;

    private float value;

    private void Awake()
    {
        radioManager = GetComponentInParent<RadioManager>();
        audioSettings = FindObjectOfType<AudioSettings>();

        if (dial == DialType.Station)//SavingLoadingManager.instance.GetHasSave() 
        {
            LoadRadioSettings();
        }
        else SetAudioSettingsValues();
    }
    
    private void OnEnable()
    {
        audioSettings = FindObjectOfType<AudioSettings>();

        if (dial == DialType.Station)//SavingLoadingManager.instance.GetHasSave() 
        {
            LoadRadioSettings();
        }
        else SetAudioSettingsValues();
    }
    
    private void OnDisable()
    {
        SendAudioSettingsValues();
        if (dial == DialType.Station) SaveRadioSettings();
    }
    private void OnDestroy()
    {
        SendAudioSettingsValues();
        if (dial == DialType.Station) SaveRadioSettings();
    }
    

    void Update()
    {
        //ensure dial is unlocked
        if (rotator.rotation.eulerAngles.z > 3 && rotator.rotation.eulerAngles.z < 357) locked = false;

        //player click/drag
        if (Input.GetMouseButton(0) && isMouseOverObject && !locked)
        {
            //dial sprite turns towards mouse position
            Vector3 difference = rotator.transform.InverseTransformPoint(Input.mousePosition);
            var angle = Mathf.Atan2(difference.x, difference.y) * Mathf.Rad2Deg;
            rotator.transform.Rotate(0f, 0f, -angle);

            //set slider values based on rotation
            if (dial == DialType.Station)
            {
                //within 4 degrees of each notch (20, 45, 90, 270, 315, 340 degrees) switch stations. 
                if (rotator.rotation.eulerAngles.z > 18 && rotator.rotation.eulerAngles.z <= 22) value = 0;
                else if (rotator.rotation.eulerAngles.z > 43 && rotator.rotation.eulerAngles.z <= 55) value = 1;
                else if (rotator.rotation.eulerAngles.z > 88 && rotator.rotation.eulerAngles.z <= 92) value = 2;
                else if (rotator.rotation.eulerAngles.z > 268 && rotator.rotation.eulerAngles.z <= 272) value = 3;
                else if (rotator.rotation.eulerAngles.z > 305 && rotator.rotation.eulerAngles.z <= 317) value = 4;
                else if (rotator.rotation.eulerAngles.z > 338 && rotator.rotation.eulerAngles.z <= 342) value = 5;

            }
            else value = 1 - (rotator.rotation.eulerAngles.z / 360);

            UpdateRadioManager();
            
            //fill volume bar based on slider value
            if (dial != DialType.Station) myImage.fillAmount = value;
        }

        //if player lets go outside of hitbox, let go
        if(Input.GetMouseButtonUp(0)) isMouseOverObject = false;

        if(Input.GetKeyDown(KeyCode.Escape)) SendAudioSettingsValues();
    }

    private void UpdateRadioManager()
    {
        switch (dial)
        {
            case DialType.Station:
                radioManager.RadioStationSlider(value);
                return;
            case DialType.Master:
                radioManager.MasterVolSlider(value);
                break;
            case DialType.Radio:
                radioManager.RadioVolSlider(value);
                break;
            case DialType.BGM:
                radioManager.BGMVolSlider(value);
                break;
            case DialType.SFX:
                radioManager.SFXVolSlider(value);
                break;
            default:
                Debug.LogError("Radio Dial Type Not Set");
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isMouseOverObject = true;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!Input.GetMouseButton(0)) isMouseOverObject = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!Input.GetMouseButton(0)) isMouseOverObject = false;
    }

    private void SaveRadioSettings()
    {
        SavingLoadingManager.instance.Save<float>("sliderVal", value);
    }

    private void LoadRadioSettings()
    {
        value = SavingLoadingManager.instance.Load<float>("sliderVal");
        
        UpdateRadioManager();
    }

    public void SetAudioSettingsValues()
    {
        switch (dial)
        {
            case DialType.Station:
                return;
            case DialType.Master:
                value = AudioSettings.masterVol;
                break;
            case DialType.Radio:
                value = AudioSettings.radioVol;
                break;
            case DialType.BGM:
                value = AudioSettings.bgmVol;
                break;
            case DialType.SFX:
                value = AudioSettings.sfxVol;
                break;
            default:
                Debug.LogError("Radio Dial Type Not Set");
                break;
        }
        
        UpdateRadioManager();

        rotator.transform.rotation = Quaternion.identity;
        rotator.transform.Rotate(0f, 0f, -(value * 360f));
        if (dial != DialType.Station) myImage.fillAmount = value;
    }

    private void SendAudioSettingsValues()
    {
        switch (dial)
        {
            case DialType.Station:
                return;
            case DialType.Master:
                AudioSettings.masterVol = value;
                break;
            case DialType.Radio:
                AudioSettings.radioVol = value;
                break;
            case DialType.BGM:
                AudioSettings.bgmVol = value;
                break;
            case DialType.SFX:
                AudioSettings.sfxVol = value;
                break;
            default:
                Debug.LogError("Radio Dial Type Not Set");
                break;
        }
        
        audioSettings?.SaveAudioSettings();
    }
}