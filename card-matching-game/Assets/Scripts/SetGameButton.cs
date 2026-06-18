using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetGameButton : MonoBehaviour
{
    public enum EButtonType
    {
        NotSet,
        DifficultyBtn,
        TopicsBtn,
    };

    [SerializeField] public EButtonType ButtonType= EButtonType.NotSet;
    [HideInInspector] public GameSettings.EPairNumber PairNum = GameSettings.EPairNumber.NotSet;
    [HideInInspector] public GameSettings.EPuzzleTopics Topics = GameSettings.EPuzzleTopics.NotSet;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGameOption(string GameSceneName)
    {
        var comp = gameObject.GetComponent<SetGameButton>();

        switch(comp.ButtonType)
        {
            case SetGameButton.EButtonType.DifficultyBtn:
                GameSettings.Instance.SetPairNumber(comp.PairNum);
                break;

            case EButtonType.TopicsBtn:
                GameSettings.Instance.SetPuzzleTopics(comp.Topics);
                break;
        }

        SceneManager.LoadScene(GameSceneName);
    }
}
