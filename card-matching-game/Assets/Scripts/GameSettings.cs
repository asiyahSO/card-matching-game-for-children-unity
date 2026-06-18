using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private readonly Dictionary<EPuzzleTopics, string> _TopicsDirectory = new Dictionary<EPuzzleTopics, string>();
    private int _settings;
    private const int SettingsNumber = 2;

    public enum EPairNumber
    {
        NotSet = 0,
        easy = 3,
        normal = 5,
        hard = 9,
    }

    public enum EPuzzleTopics
    { 
        NotSet,
        Colour,
        Animals,
        Fruits
    }

    public struct Settings
    {
        public EPairNumber PairsNum;
        public EPuzzleTopics PuzzleTopic;
    };

    private Settings _gameSettings;

    public static GameSettings Instance;


    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SetPuzzleTopDirectory();
        _gameSettings = new Settings();
        ResetGameSettings();
    }

    public void SetPuzzleTopDirectory()
    {
        _TopicsDirectory.Add(EPuzzleTopics.Fruits, "Fruits");
        _TopicsDirectory.Add(EPuzzleTopics.Colour, "Colour");
        _TopicsDirectory.Add(EPuzzleTopics.Animals, "Animals");
    }

    public void SetPairNumber(EPairNumber Number)
    {
        if (_gameSettings.PairsNum == EPairNumber.NotSet)
            _settings++;

        _gameSettings.PairsNum = Number;
    }

    public void SetPuzzleTopics(EPuzzleTopics topic)
    {
        if(_gameSettings.PuzzleTopic == EPuzzleTopics.NotSet)
            _settings++;

        _gameSettings.PuzzleTopic = topic;
    }


    public EPairNumber GetPairNumber()
    {
        return _gameSettings.PairsNum;
    }

    public EPuzzleTopics GetPuzzleTopic()
    {
        return _gameSettings.PuzzleTopic;
    }

    public void ResetGameSettings()
    {
        _settings = 0;
        _gameSettings.PuzzleTopic = EPuzzleTopics.NotSet;
        _gameSettings.PairsNum = EPairNumber.NotSet;
    }    

    public bool AllSettingsReady()
    {
        return _settings == SettingsNumber;
    }

    public string GetMaterialDirectoryName()
    {
        return "Materials/";
    }

    public string GetPuzzleTopicsTextureDirectoryName()
    {
        if(_TopicsDirectory.ContainsKey(_gameSettings.PuzzleTopic))
        {
            return "Topics/" + _TopicsDirectory[_gameSettings.PuzzleTopic] + "/";
        }
        else
        {
            Debug.LogError("ERROR: CANNOT GET DIRECTORY NAME");
            return "";
        }
    }

    public string GetPronunciationDirectoryName()
    {
        if (_TopicsDirectory.ContainsKey(_gameSettings.PuzzleTopic))
        {
            // Example: "Audio/Pronunciation/Fruits/"
            return "Audio/Pronunciation/" + _TopicsDirectory[_gameSettings.PuzzleTopic] + "/";
        }
        else
        {
            Debug.LogError("ERROR: CANNOT GET PRONUNCIATION DIRECTORY NAME");
            return "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
