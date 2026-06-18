using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayClickOnClick : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (ButtonClickSound.instance != null)
            {
                ButtonClickSound.instance.PlayClick();
            }
        });
    }
}

