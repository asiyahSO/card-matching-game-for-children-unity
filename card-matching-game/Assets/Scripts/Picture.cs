using System.Collections;
using UnityEngine;

public class Picture : MonoBehaviour
{
    private Material _firstMaterial;
    private Material _secondMaterial;

    private Quaternion _currentRotation;

    [HideInInspector] public bool Revealed = false;
    private PictureManager _pictureManager;
    private bool _clicked = false;
    private int _index;  

    public void SetIndex(int id) { _index = id; }
    public int GetIndex() {  return _index; }

    private AudioSource audioSource;
    private AudioClip flipSound;

    private AudioClip _pronunciationEN;
    private AudioClip _pronunciationJP;

    // Start is called before the first frame update
    void Start()
    {
        Revealed = false;
        _clicked = false;
        _pictureManager = GameObject.Find("PictureManager").GetComponent<PictureManager>();
        _currentRotation = gameObject.transform.rotation;

        // Load and assign flip sound
        audioSource = gameObject.AddComponent<AudioSource>();
        flipSound = Resources.Load<AudioClip>("Audio/cardFlip");

        // Configure audio (optional: set volume, disable play on awake)
        audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        if (_pictureManager.CurrentPuzzleState != PictureManager.PuzzleState.CanRotate) return;
        if (_clicked || Revealed) return;

        _clicked = true;
        _pictureManager.CurrentPuzzleState = PictureManager.PuzzleState.PuzzleRotating;
        StartCoroutine(LoopRotation(45, false));
    }



    public void FlipBack()
    {
        if (gameObject.activeSelf)
        {
            _pictureManager.CurrentPuzzleState = PictureManager.PuzzleState.PuzzleRotating;
            Revealed = false;
            StartCoroutine(LoopRotation(45, true));
        }
    }

    IEnumerator LoopRotation(float angle, bool isFlippingBack)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion midRotation = startRotation * Quaternion.Euler(0, 90, 0);
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);

        Renderer rend = GetComponent<Renderer>();
        bool materialSwitched = false;

        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth flip
            if (t < 0.5f)
                transform.rotation = Quaternion.Lerp(startRotation, midRotation, t * 2);
            else
                transform.rotation = Quaternion.Lerp(midRotation, endRotation, (t - 0.5f) * 2);

            // Squeeze effect (make card flat at midpoint)
            float squeeze = Mathf.Abs(Mathf.Cos(t * Mathf.PI)); // from 1 -> 0 -> 1
            transform.localScale = new Vector3(originalScale.x * squeeze, originalScale.y, originalScale.z);

            if (!materialSwitched && t >= 0.5f)
            {
                rend.enabled = false;
                if (isFlippingBack)
                    ApplyFirstMaterial();
                else
                    ApplySecondMaterial();
                rend.enabled = true;

                materialSwitched = true;

                // 🔊 Play flip sound if sound is not muted
                if (!ButtonClickSound.instance.IsClickSoundMuted())
                {
                    audioSource.PlayOneShot(flipSound);
                }
            }

            yield return null;
        }

        transform.rotation = _currentRotation;
        transform.localScale = originalScale;

        if (!isFlippingBack)
        {
            Revealed = true;

            if (_pictureManager.FirstRevealedPicture == null)
            {
                _pictureManager.FirstRevealedPicture = this;
                _pictureManager.CurrentPuzzleState = PictureManager.PuzzleState.CanRotate;
            }
            else if (_pictureManager.SecondRevealedPicture == null)
            {
                _pictureManager.SecondRevealedPicture = this;
                _pictureManager.CurrentPuzzleState = PictureManager.PuzzleState.CanRotate;
                _pictureManager.StartCoroutine(_pictureManager.CheckMatch());
            }
        }
        else
        {
            _pictureManager.PuzzleRevealedNumber = PictureManager.RevealedState.NoRevealed;
            _pictureManager.CurrentPuzzleState = PictureManager.PuzzleState.CanRotate;
        }

        _clicked = false;
    }






    public void SetFirstMaterial(Material mat, string texturePath)
    {
        _firstMaterial = mat;
        _firstMaterial.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
    }

    public void SetSecondMaterial(Material mat, string texturePath)
    {
        _secondMaterial = mat;
        _secondMaterial.mainTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;
    }

    public void ApplyFirstMaterial()
    {
        gameObject.GetComponent<Renderer>().material = _firstMaterial;
    }

    public void ApplySecondMaterial()
    {
        gameObject.GetComponent<Renderer>().material = _secondMaterial;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void SetPronunciationClips(AudioClip enClip, AudioClip jpClip)
    {
        _pronunciationEN = enClip;
        _pronunciationJP = jpClip;
    }

    public AudioClip GetPronunciationEN() => _pronunciationEN;
    public AudioClip GetPronunciationJP() => _pronunciationJP;
}


