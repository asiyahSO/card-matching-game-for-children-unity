using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    public static ButtonClickSound instance;
    private AudioSource audioSource;
    private bool isMuted = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick()
    {
        if (!isMuted && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        Debug.Log("PlayClick() called. Muted: " + isMuted);
    }

    public void SetClickSoundMuted(bool mute)
    {
        isMuted = mute;
    }

    public bool IsClickSoundMuted()
    {
        return isMuted;
    }
}

