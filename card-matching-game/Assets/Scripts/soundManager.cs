using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    public static soundManager instance;

    public AudioSource audioSource;
    public AudioClip colorClip;
    public AudioClip animalClip;
    public AudioClip fruitClip;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // This persists between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayColorSound()
    {
        PlaySound(colorClip);
    }

    public void PlayAnimalSound()
    {
        PlaySound(animalClip);
    }

    public void PlayFruitSound()
    {
        PlaySound(fruitClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Clip or AudioSource missing!");
        }
    }
}
