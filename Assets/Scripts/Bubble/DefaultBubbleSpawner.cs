using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultBubbleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _bubblePrefab;
    [SerializeField] private Transform _spawnPointTransform;
    [SerializeField] private Transform _bubblesParent;
    [SerializeField] private float _horizontalSpacing = 1.5f;
    [SerializeField] private float _verticalSpacing = 1.5f;

    private LevelData _levelData;
    public static DefaultBubbleSpawner instance;

    private List<Bubble> _allBalls = new List<Bubble>();
    private HashSet<Color> _activeColors = new HashSet<Color>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (_bubblesParent == null)
        {
            _bubblesParent = new GameObject("BubblesParent").transform;
        }
    }

    public void SetLevelData(LevelData levelData)
    {
        _levelData = levelData;
        SpawnBalls();
        UpdateActiveColors(); 
    }

    private void SpawnBalls()
    {
        if (_levelData != null && _levelData.rows != null)
        {
            float startY = _spawnPointTransform.position.y;

            for (int row = 0; row < _levelData.rows.Length; row++)
            {
                LevelRow levelRow = _levelData.rows[row];
                float yPos = startY + row * _verticalSpacing;

                float totalRowWidth = (levelRow.balls.Length - 1) * _horizontalSpacing;
                float startX = _spawnPointTransform.position.x - totalRowWidth / 2f;

                for (int col = 0; col < levelRow.balls.Length; col++)
                {
                    LevelBall levelBall = levelRow.balls[col];

                    if (levelBall.isEmpty)
                        continue;

                    float xPos = startX + col * _horizontalSpacing;
                    Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
                    GameObject ballObj = Instantiate(_bubblePrefab, spawnPos, Quaternion.identity, _bubblesParent);
                    Bubble ball = ballObj.GetComponent<Bubble>();
                    ball.typeOfBall = Bubble.TypeOfBall.Default;
                    ball.GetComponent<SpriteRenderer>().color = levelBall.color;
                    ball.SetBallActive(true, spawnPos);
                    _allBalls.Add(ball);

                    if (row == 0)
                    {
                        ball.isVisited = true;
                    }

                    _activeColors.Add(levelBall.color);
                }
            }
        }
    }

    public List<Color> GetActiveColors()
    {
        return new List<Color>(_activeColors);
    }

    public List<Bubble> GetAllBalls()
    {
        return _allBalls;
    }

    public void AddBall(Bubble ball)
    {
        if (!_allBalls.Contains(ball))
        {
            _allBalls.Add(ball);
            _activeColors.Add(ball.GetComponent<SpriteRenderer>().color);
        }
    }

    public void RemoveBall(Bubble ball)
    {
        if (ball != null && _allBalls.Contains(ball))
        {
            _allBalls.Remove(ball);
        }
        UpdateActiveColors(); 
    }

    public void UpdateActiveColors()
    {
        _activeColors.Clear();  
        foreach (Bubble ball in _allBalls)
        {
            if (!ball.IsFalling())  
            {
                _activeColors.Add(ball.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    public int GetBallsInLastRow()
    {
        int ballsInLastRow = 0;
        foreach (Bubble ball in _allBalls)
        {
            if (ball != null && Mathf.Abs(ball.transform.position.y - GetTopBoundary()) < 0.1f)
            {
                ballsInLastRow++;
            }
        }
        return ballsInLastRow;
    }

    public int GetTotalBallsInLastRow()
    {
        if (_levelData != null && _levelData.rows.Length > 0)
        {
            return _levelData.rows[_levelData.rows.Length - 1].balls.Length;
        }
        return 0;
    }

    public float GetTopBoundary()
    {
        return _spawnPointTransform.position.y + _verticalSpacing * (_levelData.rows.Length - 1);
    }

    public void CheckForFloatingBalls()
    {
        List<Bubble> allBalls = GetAllBalls();

        foreach (Bubble ball in allBalls)
        {
            ball.isVisited = false;
        }

        foreach (Bubble ball in allBalls)
        {
            if (ball.transform.position.y >= GetTopBoundary())
            {
                ball.isVisited = true;
                FindConnectedBalls(ball);
            }
        }

        foreach (Bubble ball in allBalls)
        {
            if (!ball.isVisited)
            {
                StartCoroutine(FallAndDestroy(ball));
            }
        }
    }

    public void MakeRemainingBallsFall()
    {
        List<Bubble> ballsCopy = new List<Bubble>(_allBalls);

        foreach (Bubble ball in ballsCopy)
        {
            if (ball != null)
            {
                StartCoroutine(FallAndDestroy(ball));
            }
        }
    }

    private void FindConnectedBalls(Bubble ball)
    {
        Queue<Bubble> queue = new Queue<Bubble>();
        queue.Enqueue(ball);
        ball.isVisited = true;

        while (queue.Count > 0)
        {
            Bubble currentBall = queue.Dequeue();
            List<Bubble> neighbors = currentBall.GetNeighborBalls();

            foreach (Bubble neighbor in neighbors)
            {
                if (!neighbor.isVisited)
                {
                    neighbor.isVisited = true;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private IEnumerator FallAndDestroy(Bubble ball)
    {
        if (ball == null) yield break;

        ball.SetFalling(true);

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }

        while (ball != null && ball.transform.position.y > -6f)
        {
            yield return null;
        }

        if (ball != null)
        {
            ScoreManager.instance.AddScore(1);

            DefaultBubbleSpawner.instance.RemoveBall(ball);
            Destroy(ball.gameObject);
        }

        GameManager.instance.CheckWinCondition();
    }

    public Color GetRandomActiveColor()
    {
        if (_activeColors.Count > 0)
        {
            List<Color> colors = new List<Color>(_activeColors);
            return colors[Random.Range(0, colors.Count)];
        }
        else
        {
            return Color.white;
        }
    }
}
