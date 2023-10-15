using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _coinCount = 0;
    private int _score = 0;
    private float _scoreCounter;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _coinText;
    public static ScoreManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
        IncreaseCoin(0);
        _scoreText.text = _score.ToString("D6");
    }
    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameEnded) return;
        _scoreCounter += Time.deltaTime * 10f;
        _score = (int)_scoreCounter;
        _scoreText.text = _score.ToString("D6");
    }
    public void IncreaseCoin(int value)
    {
        _coinCount += value;
        _coinText.text = _coinCount.ToString();
    }
}
