using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsClickSound : MonoBehaviour
{
    public Button soundOnButton;
    public Button soundOffButton;

    public Image soundIconImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    void Start()
    {
        soundOnButton.onClick.AddListener(() => SetClickSound(true));
        soundOffButton.onClick.AddListener(() => SetClickSound(false));

        UpdateSoundIcon();
    }

    void SetClickSound(bool isOn)
    {
        Debug.Log("SetClickSound: " + isOn);
        if (ButtonClickSound.instance != null)
        {
            ButtonClickSound.instance.SetClickSoundMuted(!isOn);
        }

        UpdateSoundIcon();
    }

    void UpdateSoundIcon()
    {
        if (ButtonClickSound.instance == null || soundIconImage == null) return;

        if (ButtonClickSound.instance.IsClickSoundMuted())
        {
            soundIconImage.sprite = soundOffSprite;
        }
        else
        {
            soundIconImage.sprite = soundOnSprite;
        }
    }
}

