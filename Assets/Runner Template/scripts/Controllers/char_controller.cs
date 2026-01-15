using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class char_controller : MonoBehaviour
{
    public enum type
    {
        Human, AI
    }

    private Vector2 touchStartPos;
    private bool swipeDetected;

    public type charType;

    [Space(10)]
    public float xMoveLimit;
    public float min_yMoveLimit, max_yMoveLimit;

    [Header("Smooth lane change (Variant 1)")]
    public float laneStep = 3f;            // ширина полосы (у тебя было +/-3)
    public float laneChangeTime = 0.12f;   // чем больше — тем плавнее
    private float xSmoothVel;              // служебная скорость SmoothDamp

    public bool isTransitioning;
    public float targetPosition;           // плавная X позиция
    private float side = 0;                // целевая полоса (X)
    private float prevSide = 0;

    public delegate void charDelegate();
    public charDelegate OnSideWallHit, OnFrontHit, OnDead;

    public delegate void charPosDelegate(float pos);
    public charPosDelegate OnCharPositionChanged;

    [Space(10)]
    public Rigidbody rigidboddy;
    public float speed = 5;                // оставил, но теперь для SmoothDamp не нужен как скорость
    private float maxSpeed;
    public float forwardSpeed = 10;

    [Range(0, 1)]
    public float transitionValue;
    public bool isDead;

    public bool isGrounded;

    public int lives = 3;
    public List<string> obstacleTags;

    [Header("AI Paramters: ")]
    public char_controller _Target;
    public float rayDistance = 3;
    public LayerMask mask;
    public float minZ, maxZ;
    public float _keepDistance;
    private Coroutine coroutineRef;

    private float playerPos;

    void Start()
    {
        maxSpeed = speed;

        // чтобы старт не дёргался, синхронизируемся
        targetPosition = transform.position.x;
        side = targetPosition;

        if (charType == type.AI)
        {
            _Target.OnSideWallHit += OnTargetHit;
            _Target.OnDead += OnTargetDead;
            _Target.OnCharPositionChanged += OnTargetChangedPosition;
        }
    }

    #region AI Callbacks
    void OnTargetHit()
    {
        forwardSpeed = Mathf.Clamp(forwardSpeed + 10, 50, 70);
    }

    void OnTargetDead()
    {
        forwardSpeed = 0;
    }

    void OnTargetChangedPosition(float pos)
    {
        if (coroutineRef == null)
        {
            coroutineRef = StartCoroutine(changePosition(pos));
        }
    }

    IEnumerator changePosition(float pos)
    {
        yield return new WaitForSeconds(0.1f);
        playerPos = pos;
        coroutineRef = null;
    }
    #endregion

    void FixedUpdate()
    {
        if (isDead)
        {
            if (!rigidboddy.isKinematic)
            {
                rigidboddy.isKinematic = true;

                if (charType == type.Human)
                {
                    if (OnDead != null)
                        OnDead();
                }
            }
            return;
        }

        // ====== ПЛАВНОЕ ДВИЖЕНИЕ ПО X (SmoothDamp) ======
        side = Mathf.Clamp(side, -xMoveLimit, xMoveLimit);

        targetPosition = Mathf.SmoothDamp(
            targetPosition,
            side,
            ref xSmoothVel,
            laneChangeTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );

        Vector3 reqPos = new Vector3(
            targetPosition,
            rigidboddy.position.y,
            rigidboddy.position.z
        );

        rigidboddy.MovePosition(reqPos);

        // ====== ДВИЖЕНИЕ ВПЕРЁД (как у тебя, но аккуратнее) ======
        // ВАЖНО: forceVector у тебя был странный (x,y,forwardSpeed).
        // Я оставляю AddForce идею, но делаю вектор силы строго вперёд.
        rigidboddy.AddForce(Vector3.forward * forwardSpeed, ForceMode.Acceleration);

        // Ограничение скорости (если у тебя именно linearVelocity, ок; но обычно это velocity)
        // Оставляю как было у тебя, чтобы не сломать проект:
        rigidboddy.linearVelocity = Vector3.ClampMagnitude(rigidboddy.linearVelocity, forwardSpeed / 2f);
    }

    void LateUpdate()
    {
        if (charType == type.AI)
        {
            minZ = _Target.transform.position.z - _keepDistance;
            maxZ = _Target.transform.position.z - 2f;

            transform.position = new Vector3(
                transform.position.x,
                Mathf.Clamp(transform.position.y, min_yMoveLimit, max_yMoveLimit),
                Mathf.Clamp(transform.position.z, minZ, maxZ)
            );
        }
    }

    void Update()
    {
        if (charType == type.AI)
        {
            ObstacleAvoidance();
        }
        else
        {
            bool leftKey = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && !isTransitioning;
            bool rightKey = (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && !isTransitioning;

            bool leftSwipe = false;
            bool rightSwipe = false;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    swipeDetected = false;
                }

                if (touch.phase == TouchPhase.Ended && !swipeDetected)
                {
                    Vector2 delta = touch.position - touchStartPos;

                    if (Mathf.Abs(delta.x) > 30 && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        swipeDetected = true;

                        if (delta.x > 0) rightSwipe = !isTransitioning;
                        else leftSwipe = !isTransitioning;
                    }
                }
            }

            var left = leftKey || leftSwipe;
            var right = rightKey || rightSwipe;

            if (left)
            {
                prevSide = side;

                if (side == xMoveLimit)
                    side = 0;
                else
                    side -= laneStep; // было "-= 3"
            }
            else if (right)
            {
                prevSide = side;

                if (side == -xMoveLimit)
                    side = 0;
                else
                    side += laneStep; // было "+= 3"
            }

            OnCharPositionChanged?.Invoke(side);
        }

        Movement();
    }

    #region AI [EXPERIMENTAL]
    void ObstacleAvoidance()
    {
        if (isTransitioning || _Target.isDead)
            return;

        var hit = new RaycastHit();

        var isRightBlocked = false;
        var isLeftBlocked = false;
        var isFrontBlocked = false;

        if (side != playerPos)
        {
            if (playerPos == xMoveLimit)
            {
                isRightBlocked = Physics.Raycast(transform.position, transform.right, out hit, rayDistance, mask);
                if (!isRightBlocked)
                {
                    prevSide = side;
                    side = playerPos;
                    return;
                }
            }
            else if (playerPos == -xMoveLimit)
            {
                isLeftBlocked = Physics.Raycast(transform.position, -transform.right, out hit, rayDistance, mask);
                if (!isLeftBlocked)
                {
                    prevSide = side;
                    side = playerPos;
                    return;
                }
            }
            else
            {
                isFrontBlocked = Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, mask);

                var BlockageAhead = Physics.Raycast(_Target.transform.position, -_Target.transform.forward, out hit, rayDistance * 2.5f, mask);

                if (playerPos == 0)
                {
                    if (BlockageAhead && !isFrontBlocked)
                        return;

                    isLeftBlocked = Physics.Raycast(transform.position, -transform.right, out hit, rayDistance, mask);

                    if (isLeftBlocked)
                    {
                        isRightBlocked = Physics.Raycast(transform.position, transform.right, out hit, rayDistance, mask);
                        if (!isRightBlocked)
                        {
                            prevSide = side;
                            if (side == xMoveLimit || side == -xMoveLimit) side = 0;
                            else side += laneStep;
                        }
                    }
                    else
                    {
                        prevSide = side;
                        if (side == xMoveLimit || side == -xMoveLimit) side = 0;
                        else side -= laneStep;
                    }
                }
            }
        }
    }
    #endregion

    #region Movement
    void Movement()
    {
        // Плавный переход: сравниваем с небольшой погрешностью, чтобы не “дрожало”
        isTransitioning = Mathf.Abs(side - targetPosition) > 0.01f;

        side = Mathf.Clamp(side, -xMoveLimit, xMoveLimit);
        targetPosition = Mathf.Clamp(targetPosition, -xMoveLimit, xMoveLimit);

        transitionValue = Mathf.Abs(1 - (transform.position.x) / xMoveLimit);
    }
    #endregion

    void OnCollisionEnter(Collision other)
    {
        if (tag == "platform" || tag == "ground")
        {
            isGrounded = true;
        }
    }

    // ====== ONHIT ОСТАВЛЕН БЕЗ ИЗМЕНЕНИЙ, КАК ТЫ ПРОСИЛ ======
    public void OnHit(Collider col, sensor _sensor)
    {
        if (obstacleTags.Contains(col.tag))
        {
            switch (_sensor.sensorType)
            {
                case sensor.type.front:
                    lives = -1;

                    if (charType == type.AI)
                        ObstacleAvoidance();
                    break;
                case sensor.type.back:
                    lives = -1;
                    break;
                default:
                    side = prevSide;
                    lives--;
                    break;
            }

            if (charType == type.Human)
            {
                if (OnSideWallHit != null)
                    OnSideWallHit();
            }

            if (lives < 0)
            {
                lives = 0;
                switch (charType)
                {
                    case type.AI:

                        break;
                    default:
                        isDead = true;
                        GameManager.instance.OnPlayerDied();
                        break;
                }
            }
        }
    }
}
