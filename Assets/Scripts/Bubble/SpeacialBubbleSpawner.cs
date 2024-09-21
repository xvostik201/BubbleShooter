using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpeacialBubbleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform[] _ballSpawnPoint;
    [SerializeField] private int _ballsToLevel;
    [SerializeField] private float _spawnDelay;

    private GameObject _firstBall;
    private GameObject _secondBall;
    private bool _isFirstSpawn = true;

    [SerializeField] private LineRenderer _mainLineRenderer;
    [SerializeField] private LineRenderer _leftLineRenderer;
    [SerializeField] private LineRenderer _rightLineRenderer;
    [SerializeField] private float _predictableLineMaxRange = 10f;
    [SerializeField] private float _minX = -8.5f;
    [SerializeField] private float _maxX = 8.5f;

    [SerializeField] private float _spreadAngle = 10f;

    public static SpeacialBubbleSpawner instance;

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
    }

    public float GetSpreadAngle()
    {
        return _spreadAngle;
    }

    void Start()
    {
        if (DefaultBubbleSpawner.instance != null)
        {
            TrySpawnBall();
        }
    }

    private void Update()
    {
        if (_firstBall != null && _mainLineRenderer.enabled)
        {
            float pullStrength = _firstBall.GetComponent<Bubble>().GetPullStrength();
            UpdateLineRenderer(_firstBall.transform.position, pullStrength);
        }
    }

    public void UpdateLineRenderer(Vector3 currentPosition, float pullStrength)
    {
        if (_firstBall == null)
            return;

        Vector2 direction = _firstBall.GetComponent<Bubble>().PredictableLine();
        _mainLineRenderer.positionCount = 1;
        _mainLineRenderer.SetPosition(0, currentPosition);

        Vector3 endPoint = currentPosition;
        Vector2 currentDirection = direction;

        for (int i = 1; i < 10; i++)
        {
            endPoint += (Vector3)currentDirection * _predictableLineMaxRange * 0.1f;

            if (endPoint.x <= _minX || endPoint.x >= _maxX)
            {
                currentDirection.x *= -1;
                endPoint.x = Mathf.Clamp(endPoint.x, _minX, _maxX);
            }

            _mainLineRenderer.positionCount = i + 1;
            _mainLineRenderer.SetPosition(i, endPoint);
        }

        if (pullStrength >= 1f)
        {
            _mainLineRenderer.enabled = false;
            _leftLineRenderer.enabled = true;
            _rightLineRenderer.enabled = true;
            DrawSpreadLine(_leftLineRenderer, currentPosition, direction, -_spreadAngle);
            DrawSpreadLine(_rightLineRenderer, currentPosition, direction, _spreadAngle);
        }
        else
        {
            _mainLineRenderer.enabled = true;
            _leftLineRenderer.enabled = false;
            _rightLineRenderer.enabled = false;
        }
    }

    private void DrawSpreadLine(LineRenderer lineRenderer, Vector3 startPosition, Vector2 direction, float angle)
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, startPosition);

        Vector2 spreadDirection = Quaternion.Euler(0, 0, angle) * direction;
        Vector3 spreadEndPoint = startPosition;

        for (int i = 1; i < 10; i++)
        {
            spreadEndPoint += (Vector3)spreadDirection * _predictableLineMaxRange * 0.1f;

            if (spreadEndPoint.x <= _minX || spreadEndPoint.x >= _maxX)
            {
                spreadDirection.x *= -1;
                spreadEndPoint.x = Mathf.Clamp(spreadEndPoint.x, _minX, _maxX);
            }

            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, spreadEndPoint);
        }
    }

    private void TrySpawnBall()
    {
        if (_ballsToLevel > 0)
        {
            List<Color> activeColors = DefaultBubbleSpawner.instance.GetActiveColors();
            if (activeColors.Count == 0)
            {
                GameManager.instance.GameOver();
                return;
            }

            if (_isFirstSpawn)
            {
                _isFirstSpawn = false;

                _firstBall = Instantiate(_ballPrefab, _ballSpawnPoint[0].position, Quaternion.identity);
                _firstBall.GetComponent<SpriteRenderer>().color = GetRandomColor(activeColors);
                _firstBall.GetComponent<Bubble>().SetBallActive(true, _ballSpawnPoint[0].position);
                _firstBall.GetComponent<Bubble>().typeOfBall = Bubble.TypeOfBall.Special;

                _secondBall = Instantiate(_ballPrefab, _ballSpawnPoint[1].position, Quaternion.identity);
                _secondBall.GetComponent<SpriteRenderer>().color = GetRandomColor(activeColors);
                _secondBall.GetComponent<Bubble>().SetBallActive(false);
                _secondBall.GetComponent<Bubble>().typeOfBall = Bubble.TypeOfBall.Special;
            }
            else
            {
                _firstBall = _secondBall;
                _firstBall.transform.position = _ballSpawnPoint[0].position;
                _firstBall.GetComponent<Bubble>().SetBallActive(true, _ballSpawnPoint[0].position);

                if (_ballsToLevel > 1)
                {
                    _secondBall = Instantiate(_ballPrefab, _ballSpawnPoint[1].position, Quaternion.identity);
                    _secondBall.GetComponent<SpriteRenderer>().color = GetRandomColor(activeColors);
                    _secondBall.GetComponent<Bubble>().SetBallActive(false);
                    _secondBall.GetComponent<Bubble>().typeOfBall = Bubble.TypeOfBall.Special;
                }
            }
            _ballsToLevel--;
            ScoreManager.instance.UpdateBallsLeft(_ballsToLevel);
        }
        else
        {
            GameManager.instance.GameOver();
        }
        StartCoroutine(LineRendererStateWithDelay(1f, true));
    }

    public void SpawnNewBallWithDelay()
    {
        StartCoroutine(SpawnDelay(_spawnDelay));
    }

    public void EnableLineRenderer()
    {
        _mainLineRenderer.enabled = true;
        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;
    }

    public void DisableLineRenderer()
    {
        _mainLineRenderer.enabled = false;
        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;
    }

    private IEnumerator SpawnDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TrySpawnBall();
    }

    private IEnumerator LineRendererStateWithDelay(float delay, bool state)
    {
        yield return new WaitForSeconds(delay);
        _mainLineRenderer.enabled = state;
    }

    public int GetRemainingBalls()
    {
        return _ballsToLevel;
    }

    private Color GetRandomColor(List<Color> activeColors)
    {
        if (activeColors.Count > 0)
        {
            return activeColors[Random.Range(0, activeColors.Count)];
        }
        else
        {
            return Color.white;
        }
    }
}
