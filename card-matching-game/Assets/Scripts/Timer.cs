using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private PictureManager pictureManager;

    public GUIStyle ClockStyle;

    private float _timer;
    private float _minutes;
    private float _seconds;

    private const float virtualWidth = 480.0f;
    private const float virtualHeight = 854.0f;

    private bool _hideTimer = false; // ⬅️ Add this
    private bool _stopTimer;
    private Matrix4x4 _matrix;
    private Matrix4x4 _oldMatrix;

    private float _countdownTime;

    // Start is called before the first frame update
    void Start()
    {
        _stopTimer = false;

        pictureManager = FindObjectOfType<PictureManager>(); // ✅ Assign it here
        if (pictureManager == null)
        {
            Debug.LogError("❌ PictureManager not found in the scene!");
        }

        // Scaling matrix remains
        float scale = Mathf.Min(Screen.width / virtualWidth, Screen.height / virtualHeight);
        _matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1.0f));
        _oldMatrix = GUI.matrix;

        ClockStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.05f);

        // Set countdown time based on difficulty
        switch (GameSettings.Instance.GetPairNumber())
        {
            case GameSettings.EPairNumber.easy:
                _countdownTime = 56f; 
                break;
            case GameSettings.EPairNumber.normal:
                _countdownTime = 71f; 
                break;
            case GameSettings.EPairNumber.hard:
                _countdownTime = 135f; 
                break;
            default:
                _countdownTime = 56f;
                break;
        }

        _timer = _countdownTime - 1f;
    }



    // Update is called once per frame
    void Update()
    {
        if (_stopTimer || pictureManager.CurrentGameState == PictureManager.GameState.GameEnd)
        {
            return;
        }

        if (!_stopTimer && _timer > 0f)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _timer = 0f;
                _stopTimer = true;

                if (pictureManager.CurrentGameState != PictureManager.GameState.GameEnd)
                {
                    pictureManager.CurrentGameState = PictureManager.GameState.GameEnd;
                    int randGameOver = Random.Range(0, pictureManager.GameOverPanels.Length);
                    GameObject gameOverPanel = pictureManager.GameOverPanels[randGameOver];
                    gameOverPanel.SetActive(true);

                    // 🔊 Play game over panel sound
                    AudioSource gameOverAudio = gameOverPanel.GetComponent<AudioSource>();
                    if (gameOverAudio != null && !ButtonClickSound.instance.IsClickSoundMuted())
                    {
                        gameOverAudio.Play();
                    }
                    Debug.Log("Time's up — GAME OVER!");

                    _hideTimer = true; // ✅ Hide GUI timer
                }
            }
        }
    }

    public void HideTimer()
    {
        _hideTimer = true;
    }

    private void OnGUI()
    {
        if (_hideTimer) return;
        GUI.matrix = _matrix;

        _minutes = Mathf.Floor(_timer / 60);
        _seconds = Mathf.RoundToInt(_timer % 60);
        if (_seconds == 60) _seconds = 59;

        GUI.Label(new Rect(Camera.main.rect.x + 20, 10, 120, 50), "" + _minutes.ToString("00") + ":" + _seconds.ToString("00"), ClockStyle);

        GUI.matrix = _oldMatrix; 
    }
}
