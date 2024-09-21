using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bubble : MonoBehaviour
{
    public enum TypeOfBall
    {
        Special,
        Default
    }

    [SerializeField] private float _defaultForce = 20f;
    [SerializeField] private float _maxDistance = 3f;
    [SerializeField] private GameObject _destroyBallPrefab;

    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private TrailRenderer _trailRenderer;

    public TypeOfBall typeOfBall;

    private Vector2 _startPosition;
    private bool _isBallPreparingToShoot;
    private bool _isActive;
    private float _pullStrength = 0f;
    private Vector2 _velocity;
    private bool _isMoving = false;

    [SerializeField] private float _minX = -8.5f;
    [SerializeField] private float _maxX = 8.5f;

    public bool isVisited = false;
    private bool _isFalling = false;
    private bool _hasCollided = false;
    private bool _isFirstEnter = true;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();

        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (_trailRenderer != null)
        {
            _trailRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveBall();
        }
    }

    private void MoveBall()
    {
        Vector2 position = (Vector2)transform.position + _velocity * Time.deltaTime;
        transform.position = position;

        if (transform.position.x <= _minX || transform.position.x >= _maxX)
        {
            _velocity.x *= -1;
            float clampedX = Mathf.Clamp(transform.position.x, _minX, _maxX);
            transform.position = new Vector2(clampedX, transform.position.y);
        }
    }

    private void OnMouseDown()
    {
        if (typeOfBall == TypeOfBall.Special && !_isBallPreparingToShoot && _isActive)
        {
            SpeacialBubbleSpawner.instance.EnableLineRenderer();
            _startPosition = transform.position;
        }
    }

    private void OnMouseDrag()
    {
        if (typeOfBall == TypeOfBall.Special && !_isBallPreparingToShoot && _isActive)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector2 direction = mousePosition - (Vector3)_startPosition;
            _pullStrength = Mathf.Clamp(direction.magnitude / _maxDistance, 0, 1);

            if (direction.magnitude <= _maxDistance)
            {
                _rigidbody2D.MovePosition(mousePosition);
            }
            else
            {
                direction = direction.normalized;
                Vector2 newPos = _startPosition + direction * _maxDistance;
                _rigidbody2D.MovePosition(newPos);
            }

            SpeacialBubbleSpawner.instance.UpdateLineRenderer(transform.position, _pullStrength);
        }
    }

    private void OnMouseUp()
    {
        if (typeOfBall == TypeOfBall.Special && !_isBallPreparingToShoot && _isActive)
        {
            AudioManager.instance.PlayShootSound();
            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = true;
                _trailRenderer.startColor = _spriteRenderer.color;
                _trailRenderer.endColor = _spriteRenderer.color;
            }

            Shoot();
            SpeacialBubbleSpawner.instance.DisableLineRenderer();
        }
    }

    public Vector2 PredictableLine()
    {
        Vector2 direction = (_startPosition - (Vector2)transform.position).normalized;
        return direction;
    }

    private void Shoot()
    {
        _isBallPreparingToShoot = true;
        Vector2 direction = _startPosition - (Vector2)transform.position;
        direction.Normalize();

        if (_pullStrength >= 1f)
        {
            float angle = Random.value > 0.5f ? SpeacialBubbleSpawner.instance.GetSpreadAngle() : -SpeacialBubbleSpawner.instance.GetSpreadAngle();
            direction = Quaternion.Euler(0, 0, angle) * direction;
        }

        _velocity = direction * _defaultForce * _pullStrength;
        _isMoving = true;

        SpeacialBubbleSpawner.instance.SpawnNewBallWithDelay();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bubble collidedBall = collision.GetComponent<Bubble>();
        if (collidedBall != null && typeOfBall == TypeOfBall.Special)
        {
            if (_hasCollided)
            {
                return;
            }

            if (!collidedBall.IsActive() || collidedBall.IsFalling())
            {
                return;
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = false;
            }

            AudioManager.instance.PlayPopSound();

            if (_pullStrength >= 1f && _isFirstEnter)
            {
                _isFirstEnter = false;

                Vector2 targetPosition = collidedBall.transform.position;
                DefaultBubbleSpawner.instance.RemoveBall(collidedBall);
                collidedBall.InstantiateDestroyBall(collidedBall.transform.position, collidedBall.GetColor());
                Destroy(collidedBall.gameObject);

                transform.position = targetPosition;
                AttachToTopOrNeighbor();

                _isMoving = false;
                _velocity = Vector2.zero;

                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
                _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

                SetBallActive(true, transform.position);
                typeOfBall = TypeOfBall.Default;

                DefaultBubbleSpawner.instance.AddBall(this);

                StartCoroutine(CheckForMatchingColorsAfterAttach());
                return;
            }

            _isMoving = false;
            _velocity = Vector2.zero;

            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

            SetBallActive(true, transform.position);

            AttachToTopOrNeighbor();
            typeOfBall = TypeOfBall.Default;

            DefaultBubbleSpawner.instance.AddBall(this);
            StartCoroutine(CheckForMatchingColors());
            DefaultBubbleSpawner.instance.CheckForFloatingBalls();
            GameManager.instance.CheckWinCondition();
            StartCoroutine(ReenableCollider());
        }
    }

    private IEnumerator CheckForMatchingColorsAfterAttach()
    {
        yield return new WaitForSeconds(0.1f);

        List<Bubble> matchingBalls = GetConnectedBallsOfSameColor();
        int ballsCount = matchingBalls.Count;

        if (ballsCount >= 2)
        {
            foreach (Bubble ball in matchingBalls)
            {
                if (ball != null)
                {
                    DefaultBubbleSpawner.instance.RemoveBall(ball);
                    InstantiateDestroyBall(ball.transform.position, ball.GetComponent<SpriteRenderer>().color);
                    Destroy(ball.gameObject);
                    AudioManager.instance.PlayPopSound();
                }
            }

            ScoreManager.instance.AddScore(ballsCount);

            DefaultBubbleSpawner.instance.CheckForFloatingBalls();
            GameManager.instance.CheckWinCondition();
        }
    }

    private IEnumerator CheckForMatchingColors()
    {
        yield return new WaitForEndOfFrame();

        List<Bubble> matchingBalls = GetConnectedBallsOfSameColor();
        int ballsCount = matchingBalls.Count;

        if (ballsCount >= 2)
        {
            foreach (Bubble ball in matchingBalls)
            {
                if (ball != null)
                {
                    DefaultBubbleSpawner.instance.RemoveBall(ball);
                    InstantiateDestroyBall(ball.transform.position, ball.GetComponent<SpriteRenderer>().color);
                    Destroy(ball.gameObject);
                    AudioManager.instance.PlayPopSound();
                }
            }

            ScoreManager.instance.AddScore(ballsCount);

            DefaultBubbleSpawner.instance.CheckForFloatingBalls();
        }
    }

    public List<Bubble> GetConnectedBallsOfSameColor()
    {
        List<Bubble> connectedBalls = new List<Bubble>();
        Queue<Bubble> queue = new Queue<Bubble>();
        Color targetColor = _spriteRenderer.color;

        foreach (Bubble ball in DefaultBubbleSpawner.instance.GetAllBalls())
        {
            if (ball != null)
                ball.isVisited = false;
        }

        connectedBalls.Add(this);
        queue.Enqueue(this);
        isVisited = true;

        while (queue.Count > 0)
        {
            Bubble currentBall = queue.Dequeue();
            List<Bubble> neighbours = currentBall.GetNeighborBalls();

            foreach (Bubble neighbour in neighbours)
            {
                if (!neighbour.isVisited && neighbour.GetComponent<SpriteRenderer>().color == targetColor)
                {
                    neighbour.isVisited = true;
                    connectedBalls.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }

        return connectedBalls;
    }

    private IEnumerator ReenableCollider()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider2D>().enabled = true;
        _hasCollided = false;
    }

    public void SetBallActive(bool active, Vector2 pos = default(Vector2))
    {
        _isActive = active;
        if (active)
        {
            _startPosition = pos;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _isFalling = false;
        }
    }

    public float GetPullStrength()
    {
        return _pullStrength;
    }

    public Color GetColor()
    {
        return _spriteRenderer.color;
    }

    public bool IsActive()
    {
        return _isActive;
    }

    public void SetFalling(bool falling)
    {
        _isFalling = falling;
        if (falling)
        {
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody2D.gravityScale = 1f;

            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = true;
                _trailRenderer.startColor = _spriteRenderer.color;
                _trailRenderer.endColor = _spriteRenderer.color;
            }
        }
    }

    public bool IsFalling()
    {
        return _isFalling;
    }

    public List<Bubble> GetNeighborBalls()
    {
        List<Bubble> neighbors = new List<Bubble>();
        float radius = transform.localScale.x * 0.6f;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D col in colliders)
        {
            Bubble neighborBall = col.GetComponent<Bubble>();
            if (neighborBall != null && neighborBall != this && !neighborBall.IsFalling())
            {
                neighbors.Add(neighborBall);
            }
        }

        return neighbors;
    }

    private void AttachToTopOrNeighbor()
    {
        List<Bubble> neighborBalls = GetNeighborBalls();

        if (neighborBalls.Count == 0)
        {
            if (DefaultBubbleSpawner.instance.GetAllBalls().Find(b => b.isVisited && Vector2.Distance(b.transform.position, transform.position) < 1.5f))
            {
                DefaultBubbleSpawner.instance.AddBall(this);
                SetBallActive(true, transform.position);
            }
        }
        else
        {
            foreach (Bubble neighbor in neighborBalls)
            {
                if (neighbor != null && neighbor.isActiveAndEnabled)
                {
                    DefaultBubbleSpawner.instance.AddBall(this);
                    SetBallActive(true, transform.position);
                    return;
                }
            }
        }
    }

    private void InstantiateDestroyBall(Vector2 position, Color ballColor)
    {
        GameObject destroyBall = Instantiate(_destroyBallPrefab, position, Quaternion.identity);
        foreach (Transform child in destroyBall.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = ballColor;
            }
        }
    }
}
