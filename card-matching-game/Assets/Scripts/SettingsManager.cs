using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Button musicOnButton;
    public Button musicOffButton;

    public Sprite muteSprite;    // Speaker with X
    public Sprite unmuteSprite; // Speaker on

    public Image musicIconImage; // The Image component you want to change

    void Start()
    {
        musicOnButton.onClick.AddListener(() => SetMusic(true));
        musicOffButton.onClick.AddListener(() => SetMusic(false));

        UpdateMusicIcon(); // Set correct icon on start
    }

    void SetMusic(bool isOn)
    {
        GameObject musicObj = GameObject.FindGameObjectWithTag("GameMusic");

        if (musicObj != null)
        {
            Music music = musicObj.GetComponent<Music>();
            if (music != null)
            {
                music.MuteMusic(!isOn);
            }
        }

        UpdateMusicIcon();
    }

    void UpdateMusicIcon()
    {
        GameObject musicObj = GameObject.FindGameObjectWithTag("GameMusic");

        if (musicObj != null)
        {
            Music music = musicObj.GetComponent<Music>();
            if (music != null && musicIconImage != null)
            {
                musicIconImage.sprite = music.IsMusicMuted() ? muteSprite : unmuteSprite;
            }
        }
    }
}
