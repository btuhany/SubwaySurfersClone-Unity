using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] float _lifeRegenTime = 4.2f;
    [SerializeField] AudioClip[] _sounds;
    private int _life = 2;
    public event System.Action OnBump;
    public event System.Action OnLifeRegenerated;
    public event System.Action OnGameEnd;
    public event System.Action OnLifeless;
    public event System.Action OnCrashed;
    public event System.Action OnIntroStarted;
    public event System.Action OnGameStarted;
    public bool IsGameEnded { get; private set; }
    public bool IsGameStarted { get; private set; }
    [HideInInspector] public bool isIntroStarted;
    public static GameManager Instance;

    private WaitForSeconds _startGameDelay = new WaitForSeconds(1.2f);
    private AudioSource _audioSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        IsGameStarted = false;
        _audioSource = GetComponent<AudioSource>();
    }
    public void ChangingSideCrash()
    {
        _life--;
        if (_life <= 0)
        {
            EndGame(true);
        }
        else
        {
            OnBump?.Invoke();
            StartCoroutine(LifeRegen());
        }
    }
    public void EndGame(bool isLifeless)
    {
        if(isLifeless)
        {
            PlaySound(5);
            OnLifeless?.Invoke();
        }
        else
        {
            PlaySound(6);
            OnCrashed?.Invoke();
        }
        IsGameEnded = true;
        OnGameEnd?.Invoke();
    }
    public void StartIntro()
    {
        if (isIntroStarted) return;
        PlaySound(4);
        isIntroStarted = true;
        OnIntroStarted?.Invoke(); 
        StartCoroutine(StartGameWithDelay());
    }
    public void StartGame()
    {
        IsGameStarted = true;
        OnGameStarted?.Invoke();
    }
    public void PlaySound(int index)
    {
        _audioSource.PlayOneShot(_sounds[index]);
    }
    private IEnumerator LifeRegen()
    {
        float timeCounter = 0f;
        do
        {
            timeCounter += Time.deltaTime;
            if(timeCounter >= _lifeRegenTime)
            {
                OnLifeRegenerated?.Invoke();
                timeCounter = 0f;
                _life++;
            }
            if (IsGameEnded)
                yield break;
            yield return null;
        }
        while (_life < 2);
    }
    private IEnumerator StartGameWithDelay()
    {
        yield return _startGameDelay;
        StartGame();
    }
}
