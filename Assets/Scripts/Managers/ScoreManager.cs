using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    private int _score = 0;
    private int _highScore = 0;

    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _ballsLeftText;
    [SerializeField] private TMP_Text _highScoreText;

    [SerializeField] private int _pointsPerBall = 100; 

    private void Awake()
    {
        instance = this;
        LoadHighScore();
    }

    private void Start()
    {
        UpdateScoreUI();
        UpdateBallsLeft(SpeacialBubbleSpawner.instance.GetRemainingBalls());
    }

    public void AddScore(int ballCount)
    {
        int scoreForGroup = 0;

        for (int i = 1; i <= ballCount; i++)
        {
            scoreForGroup += i * _pointsPerBall;
        }

        _score += scoreForGroup;
        UpdateScoreUI();
        CheckAndSaveHighScore();
    }

    public void UpdateBallsLeft(int ballsLeft)
    {
        _ballsLeftText.text = ballsLeft.ToString();
    }

    private void UpdateScoreUI()
    {
        _scoreText.text = "Счет: " + _score;
        _highScoreText.text = "Рекорд: " + _highScore;
    }

    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void CheckAndSaveHighScore()
    {
        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt("HighScore", _highScore);
            PlayerPrefs.Save();
        }
    }
}
