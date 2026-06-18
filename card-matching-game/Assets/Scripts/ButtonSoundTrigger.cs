using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSoundTrigger : MonoBehaviour
{
    public enum SoundType { Color, Animal, Fruit }
    public SoundType soundType;

    public void PlaySound()
    {
        if (soundManager.instance == null)
        {
            Debug.LogError("SoundManager not found.");
            return;
        }

        switch (soundType)
        {
            case SoundType.Color:
                soundManager.instance.PlayColorSound();
                break;
            case SoundType.Animal:
                soundManager.instance.PlayAnimalSound();
                break;
            case SoundType.Fruit:
                soundManager.instance.PlayFruitSound();
                break;
        }
    }
}
