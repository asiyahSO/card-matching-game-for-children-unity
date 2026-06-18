using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PictureManager : MonoBehaviour
{
    public Picture PicturePrefab;
    public Transform PicSpawnPosition;
    public Vector2 StartPosition = new Vector2(-2.15f, -3.62f);

    [Space]
    public GameObject[] WinPanels;
    public GameObject[] GameOverPanels;
    public enum GameState
    {
        NoAction,
        MovingOnPositions,
        DeletingPuzzles,
        FlipBack,
        Checking,
        GameEnd
    };

    public enum PuzzleState
    {
        PuzzleRotating,
        CanRotate
    };

    public enum RevealedState
    {
        NoRevealed,
        OneRevealed,
        TwoRevealed
    };

    [HideInInspector]
    public GameState CurrentGameState;
    [HideInInspector]
    public PuzzleState CurrentPuzzleState;
    [HideInInspector]
    public RevealedState PuzzleRevealedNumber;

    [HideInInspector]
    public List<Picture> PictureList;

    private Vector2 _offset = new Vector2(4.0f, 4.0f);
    private List<Material> _materialList = new List<Material>();
    private List<string> _texturePathList = new List<string>();
    private Material _firstMaterial;
    private string _firstTexturePath;

    [HideInInspector]
    public Picture FirstRevealedPicture;
    [HideInInspector]
    public Picture SecondRevealedPicture;

    private int _picToDestroy;
    private int _picToDestroy2;

    private int _pairNumbers;
    private int _removedPairs;

    private List<AudioClip> _pronunciationList_EN = new List<AudioClip>();
    private List<AudioClip> _pronunciationList_JP = new List<AudioClip>();
    private AudioSource _voiceSource;


    void Start()
    {
        CurrentGameState = GameState.NoAction;
        CurrentPuzzleState = PuzzleState.CanRotate;
        PuzzleRevealedNumber = RevealedState.NoRevealed;

        _removedPairs = 0;
        _pairNumbers = (int)GameSettings.Instance.GetPairNumber();

        LoadMaterials();
        int pairNumber = (int)GameSettings.Instance.GetPairNumber();

        int rows, columns;
        Vector2 offset;

        if (pairNumber == 9) // Hard level
        {
            CurrentGameState |= GameState.MovingOnPositions;
            SetupHardLevelLayout(out rows, out columns, out offset);
        }
        else // Other difficulties
        {
            // Keep your existing switch case for Easy/Normal
            switch (pairNumber)
            {
                case 3: // Easy
                    CurrentGameState |= GameState.MovingOnPositions;
                    rows = 2; columns = 3;
                    offset = new Vector2(3.0f, 3.0f);
                    break;
                case 5: // Normal
                    CurrentGameState |= GameState.MovingOnPositions;
                    rows = 2; columns = 5;
                    offset = new Vector2(2.5f, 3.0f);
                    break;
                default:
                    rows = 2; columns = 3;
                    offset = new Vector2(3.0f, 3.0f);
                    break;
            }
        }

        _voiceSource = gameObject.AddComponent<AudioSource>();
        _voiceSource.playOnAwake = false;
        _voiceSource.loop = false;
        _voiceSource.spatialBlend = 0f; // 2D
        _voiceSource.volume = 1.0f; // or 1.2f if you want, but Unity clamps at 1.0 by default

        SpawnPictureMesh(rows, columns);
        MovePicture(rows, columns, StartPosition, offset);
    }


    public IEnumerator CheckMatch()
    {
        // Disable clicks immediately
        CurrentPuzzleState = PuzzleState.PuzzleRotating;

        yield return new WaitForSeconds(0.5f);

        if (FirstRevealedPicture.GetIndex() == SecondRevealedPicture.GetIndex())
        {
            // ✅ Play EN -> JP for this word (from the first card)
            yield return StartCoroutine(PlayWordSequence(
                FirstRevealedPicture.GetPronunciationEN(),
                FirstRevealedPicture.GetPronunciationJP()
            ));

            yield return new WaitForSeconds(0.15f);
            FirstRevealedPicture.Deactivate();
            SecondRevealedPicture.Deactivate();

            _removedPairs++;

            // Check if all pairs are matched
            if (_removedPairs >= _pairNumbers)
            {
                // End game with win
                CurrentGameState = GameState.GameEnd;
                int randWin = UnityEngine.Random.Range(0, WinPanels.Length);
                GameObject winPanel = WinPanels[randWin];
                winPanel.SetActive(true);

                // 🔊 Play win panel sound
                AudioSource winAudio = winPanel.GetComponent<AudioSource>();
                if (winAudio != null && !ButtonClickSound.instance.IsClickSoundMuted())
                {
                    winAudio.Play();
                }

                Debug.Log("All cards matched! You win!");

                GameObject.FindObjectOfType<Timer>().HideTimer(); // ✅ Hide GUI timer

            }
        }
        else
        {
            yield return new WaitForSeconds(0.9f);
            FirstRevealedPicture.FlipBack();
            SecondRevealedPicture.FlipBack();

            // Wait for FlipBack animations to finish
            yield return new WaitForSeconds(0.6f); // Match duration in LoopRotation
        }

        FirstRevealedPicture = null;
        SecondRevealedPicture = null;

        // ✅ Only now allow clicking again
        CurrentPuzzleState = PuzzleState.CanRotate;
    }



    private void LoadMaterials()
    {
        var materialFilePath = GameSettings.Instance.GetMaterialDirectoryName();
        var textureFilePath = GameSettings.Instance.GetPuzzleTopicsTextureDirectoryName();
        var audioFilePath = GameSettings.Instance.GetPronunciationDirectoryName();
        var pairNumber = (int)GameSettings.Instance.GetPairNumber();
        const string matBaseName = "Pic";

        _materialList.Clear();
        _texturePathList.Clear();
        _pronunciationList_EN.Clear();
        _pronunciationList_JP.Clear();


        // Load difficulty-specific back card
        string firstMaterialName = GetBackMaterialName(pairNumber);
        _firstMaterial = LoadBackMaterial(materialFilePath, firstMaterialName);
        _firstTexturePath = textureFilePath + firstMaterialName;

        // Load front card materials
        for (var index = 1; index <= pairNumber; index++)
        {
            var currentFilePath = materialFilePath + matBaseName + index;
            Material mat = Resources.Load(currentFilePath, typeof(Material)) as Material;
            if (mat != null)
            {
                _materialList.Add(mat);
                _texturePathList.Add(textureFilePath + matBaseName + index);

                // Pronunciation (topic-scoped)
                // e.g. "Audio/Pronunciation/Fruits/Pic1_EN"
                AudioClip clipEN = Resources.Load<AudioClip>($"{audioFilePath}{matBaseName}{index}_EN");
                AudioClip clipJP = Resources.Load<AudioClip>($"{audioFilePath}{matBaseName}{index}_JP");

                if (clipEN == null) Debug.LogWarning($"[Pronunciation] Missing EN: {audioFilePath}{matBaseName}{index}_EN");
                if (clipJP == null) Debug.LogWarning($"[Pronunciation] Missing JP: {audioFilePath}{matBaseName}{index}_JP");

                _pronunciationList_EN.Add(clipEN);
                _pronunciationList_JP.Add(clipJP);
            }
            else
            {
                Debug.LogWarning($"Missing material: {currentFilePath}");
            }
        }
    }

    void Update()
    {

    }

    private string GetBackMaterialName(int pairNumber)
    {
        switch (pairNumber)
        {
            case 3: return "Back_Easy";
            case 5: return "Back_Normal";
            case 9: return "Back_Hard";
            default: return "Back";
        }
    }

    private Material LoadBackMaterial(string path, string materialName)
    {
        Material mat = Resources.Load(path + materialName, typeof(Material)) as Material;
        if (mat == null)
        {
            Debug.LogWarning($"Using default back material (failed to load {materialName})");
            mat = Resources.Load(path + "Back", typeof(Material)) as Material;
        }
        return mat;
    }

    private void SpawnPictureMesh(int rows, int columns)
    {
        int totalPictures = (int)GameSettings.Instance.GetPairNumber() * 2;
        int spawnedPictures = 0;
        int pairNumber = (int)GameSettings.Instance.GetPairNumber();


        Vector3 cardScale = pairNumber == 9
         ? new Vector3(7.5f, 2.3f, 3f) // Hard level scale
        : new Vector3(10f, 3f, 3f); // Default scale

        for (int col = 0; col < columns && spawnedPictures < totalPictures; col++)
        {
            for (int row = 0; row < rows && spawnedPictures < totalPictures; row++)
            {
                var tempPicture = Instantiate(PicturePrefab, PicSpawnPosition.position, PicturePrefab.transform.rotation);
                tempPicture.name = $"Picture_{spawnedPictures}";
                tempPicture.transform.localScale = cardScale; // Use the adjusted scale
                PictureList.Add(tempPicture);
                spawnedPictures++;
            }
        }

        ApplyTextures();
    }

    public void ApplyTextures()
    {
        // Create a list with each material index appearing twice
        List<int> materialIndices = new List<int>();
        for (int i = 0; i < _materialList.Count; i++)
        {
            materialIndices.Add(i);
            materialIndices.Add(i);
        }

        // Shuffle the material indices
        ShuffleList(materialIndices);

        // Apply to pictures
        for (int i = 0; i < PictureList.Count; i++)
        {
            int matIndex = materialIndices[i];
            PictureList[i].SetFirstMaterial(_firstMaterial, _firstTexturePath);
            PictureList[i].SetSecondMaterial(_materialList[matIndex], _texturePathList[matIndex]);
            PictureList[i].SetIndex(matIndex);

            // 👇 NEW: attach EN/ JP pronunciation for this word index

        PictureList[i].SetPronunciationClips(
            _pronunciationList_EN.Count > matIndex ? _pronunciationList_EN[matIndex] : null,
            _pronunciationList_JP.Count > matIndex ? _pronunciationList_JP[matIndex] : null
        );

            PictureList[i].Revealed = false;
            // Show the front card first instead of the back
            PictureList[i].ApplyFirstMaterial(); // Changed from ApplyFirstMaterial()
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void MovePicture(int rows, int columns, Vector2 pos, Vector2 offset)
    {
        int index = 0;
        int totalPictures = (int)GameSettings.Instance.GetPairNumber() * 2;
        int pairNumber = (int)GameSettings.Instance.GetPairNumber();

        // Get screen dimensions
        Camera mainCam = Camera.main;
        float screenHeight = 2f * mainCam.orthographicSize;
        float screenWidth = screenHeight * mainCam.aspect;

        // Base card size (matches your scale (10,3,3))
        float cardHeight = 3f;
        float cardWidth = 10f;

        // Difficulty-specific spacing adjustments
        float horizontalGapMultiplier, verticalGapMultiplier;
        float verticalOffsetAdjustment;

        if (pairNumber == 9) // Hard level specific positioning
        {
            MoveHardLevelPictures(rows, columns);
            return;
        }

        switch (pairNumber)
        {
            case 3: // Easy - 6 cards (2x3)
                horizontalGapMultiplier = 0.3f; // 30% gap
                verticalGapMultiplier = 1.2f;   // 40% gap
                verticalOffsetAdjustment = 0.009f; // 10% from top
                break;
            case 5: // Normal - 10 cards (2x5)
                horizontalGapMultiplier = 0.3f; // 20% gap
                verticalGapMultiplier = 1.2f;   // 30% gap
                verticalOffsetAdjustment = 0.009f; // 15% from top
                break;
            default:
                horizontalGapMultiplier = 0.3f;
                verticalGapMultiplier = 1.2f;
                verticalOffsetAdjustment = 1.5f;
                break;
        }

        // Calculate spacing including gaps
        float horizontalSpacing = cardWidth * horizontalGapMultiplier;
        float verticalSpacing = cardHeight * verticalGapMultiplier;

        // Calculate total grid size
        float gridWidth = (columns - 1) * horizontalSpacing + cardWidth;
        float gridHeight = (rows - 1) * verticalSpacing + cardHeight;

        // Calculate starting position (centered horizontally, adjusted vertically)
        Vector2 startPos = new Vector2(
            -gridWidth / 2 + cardWidth / 2,
            (screenHeight / 2 - gridHeight / 2) - (screenHeight * verticalOffsetAdjustment)
        );

        // Position each card
        for (int row = 0; row < rows && index < totalPictures; row++)
        {
            for (int col = 0; col < columns && index < totalPictures; col++)
            {
                Vector3 targetPosition = new Vector3(
                    startPos.x + (horizontalSpacing * col),
                    startPos.y - (verticalSpacing * row),
                    0.0f
                );

                Debug.Log($"Positioning card {index} at {targetPosition}");
                StartCoroutine(MoveToPosition(targetPosition, PictureList[index]));
                index++;
            }
        }
    }

    private void MoveHardLevelPictures(int rows, int columns)
    {
        Camera mainCam = Camera.main;
        float screenHeight = 2f * mainCam.orthographicSize;
        float screenWidth = screenHeight * mainCam.aspect;

        // Card dimensions after scaling
        float cardWidth = 6f;
        float cardHeight = 1.8f;

        // Calculate spacing
        float horizontalSpacing = (screenWidth * 0.65f) / columns;
        float verticalSpacing = (screenHeight * 0.75f) / rows;

        // Calculate grid dimensions
        float gridWidth = (columns - 1) * horizontalSpacing + cardWidth;
        float gridHeight = (rows - 1) * verticalSpacing + cardHeight;

        // Calculate start position (centered, slightly raised)
        Vector2 startPos = new Vector2(
            -gridWidth / 2 + cardWidth / 2,
            (screenHeight * 0.53f) - (gridHeight / 2) // 30% from top
        );

        // Position cards
        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns && index < PictureList.Count; col++)
            {
                Vector3 targetPos = new Vector3(
                    startPos.x + (horizontalSpacing * col),
                    startPos.y - (verticalSpacing * row),
                    0f
                );

                StartCoroutine(MoveToPosition(targetPos, PictureList[index++]));
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 target, Picture obj)
    {
        float randomDis = 20;
        while (obj.transform.position != target)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, randomDis * Time.deltaTime);
            yield return 0;
        }
    }

    private void SetupHardLevelLayout(out int rows, out int columns, out Vector2 offset)
    {
        rows = 3;
        columns = 6;
        offset = new Vector2(2.5f, 3f); // Base spacing

        // Get screen dimensions
        Camera mainCam = Camera.main;
        float screenHeight = 2f * mainCam.orthographicSize;
        float screenWidth = screenHeight * mainCam.aspect;

        // Card dimensions after scaling (6x1.8 from your previous settings)
        float cardWidth = 6f;
        float cardHeight = 1.8f;

        // Calculate required spacing
        float horizontalSpacing = (screenWidth * 0.9f) / columns; // Use 90% of screen width
        float verticalSpacing = (screenHeight * 0.6f) / rows; // Use 60% of screen height

        // Convert back to multipliers
        offset.x = horizontalSpacing / cardWidth;
        offset.y = verticalSpacing / cardHeight;
    }

    public IEnumerator PlayWordSequence(AudioClip en, AudioClip jp)
    {
        if (ButtonClickSound.instance != null && ButtonClickSound.instance.IsClickSoundMuted())
            yield break;

        if (en != null)
        {
            _voiceSource.clip = en;
            _voiceSource.Play();
            yield return new WaitForSeconds(en.length + 0.15f);
        }

        if (jp != null)
        {
            _voiceSource.clip = jp;
            _voiceSource.Play();
            yield return new WaitForSeconds(jp.length);
        }
    }

}