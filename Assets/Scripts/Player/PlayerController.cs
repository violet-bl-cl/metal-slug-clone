using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : InputManager
{
    #region OpenFields
    [SerializeField, Range(0.0f, 360.0f)]
    private float _slopeMax;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _walkSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _crouchSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    [SerializeField]
    private float _groundRay = 0.5f;
    [SerializeField]
    private LayerMask _groundLayerMask;
    [SerializeField]
    private LayerMask _overGroundLayerMask;
    [SerializeField]
    private LayerMask _underGroundLayerMask;
    [SerializeField]
    private bool _enableVisual = false;
    #endregion

    [SerializeField]
    private Vector2 _sideBoxSize = new Vector2(0.2f, 1.3f);
    private float _sideBoxDistance = 0.5f;
    [SerializeField]
    private float _bottomGroundRadius = 0.1f;
    [SerializeField]
    private float _bottomGroundDistnace = 1.1f;
    [SerializeField, Range(0.01f, 1.0f)]
    private float _slopeCastRadius = 0.2f;
    private Vector2 _topHeadBoxSize = new Vector2(0.4f, 1.5f);
    private float _topHeadBoxDistance = 0.5f;
    private float _topHeadRadius = 0.5f;
    private float _topHeadDistance = 0.8f;
    private float _forceAmount = 10.0f;
    private float _jumpDelayTime = 0.4f;
    private float _jumpGuardDuration = 0.12f;
    private float _jumpGuardTimer = 0f;
    private bool _allowInput = false;
    private bool _allowShoot = false;
    private float _inputDelayTime = 0.8f;
    private float _shootDelayTime = 0.05f;
    [SerializeField]
    private float _gravityScale = 0.0f;
    private CapsuleCollider2D _mainCapsuleCollider;
    private float _playerHeight;
    private Rigidbody2D _playerRb;
    private Coroutine _jumpCoroutine;
    private Coroutine _inputCoroutine;
    private Coroutine _shootCoroutine;
    private Coroutine _wallCoroutine;
    private Coroutine _overWallCoroutine;
    private Vector2 _slopePerpendicular;
    private Direction _direction;

    // Animation State Machine (toggled in LateUpdate)
    private PlayerTopAction _playerTopAction;
    private PlayerBottomAction _playerBotAction;
    private PlayerFullAction _playerFullAction;
    // player's chidleren capsules
    private CapsuleCollider2D _topCapsuleCollider;
    private CapsuleCollider2D _bottomCapsuleCollider;
    private CapsuleCollider2D _fullCapsuleCollider;
    //sprite editors
    private SpriteRenderer _topRenderer;
    private SpriteRenderer _bottomRenderer;
    private SpriteRenderer _fullRenderer;
    // PlayerAction Status
    [SerializeField]
    private bool _isLeftPressed, _isRightPressed;
    [SerializeField] private bool _isGround, _isSlope;
    [SerializeField]
    private bool _isCrouch, _isLookUp, _isCrouchOverDetected, _isCrouchUnderDetected, _isAnyCrouchUpDetected; 
    [SerializeField]
    private bool _isWallLeftCheck = false, _isWallRightCheck = false, _isAnyWallDetected;
    [SerializeField]
    private bool _isWallCheck = false;
    private bool _isOverWall = false;
    private bool _isAnyDirectionKeyPressed, _isAnyDirectionKeyNotPressed, _isAnyDirectionKeyUp;
    private float _horizontal;
    private Vector3 _capusleSize = new Vector3();
    private TextMeshPro _debugSlopeText;

    // --- Buffered input, written in Update(), consumed/cleared in FixedUpdate() ---
    // This exists because Update() can fire multiple times per FixedUpdate() (or vice versa);
    // buffering a "was this pressed" flag means a fast tap never gets missed by physics.
    private bool _jumpRequested;
    private bool _shootRequested;

    void Awake()
    {
        _playerRb = transform.GetComponent<Rigidbody2D>();
        _mainCapsuleCollider = transform.GetComponent<CapsuleCollider2D>();
        _playerFullAction = transform.GetComponentInChildren<PlayerFullAction>(true);
        _playerBotAction = transform.GetComponentInChildren<PlayerBottomAction>(true);
        _playerTopAction = transform.GetComponentInChildren<PlayerTopAction>(true);
        _topCapsuleCollider = transform.Find("Top").GetComponent<CapsuleCollider2D>();
        _bottomCapsuleCollider = transform.Find("Bottom").GetComponent<CapsuleCollider2D>();
        _fullCapsuleCollider = transform.Find("Full").GetComponent<CapsuleCollider2D>();
        _topRenderer = transform.Find("Top").GetComponent<SpriteRenderer>();
        _bottomRenderer = transform.Find("Bottom").GetComponent<SpriteRenderer>();
        _fullRenderer = transform.Find("Full").GetComponent<SpriteRenderer>();
        _debugSlopeText = GetComponentInChildren<TextMeshPro>(true);
    }

    void Start()
    {
        _capusleSize.Set(0, _mainCapsuleCollider.size.y / 2, 0);
    }

    // ------------------------------------------------------------------
    // UPDATE: input reading only. No physics, no rigidbody writes here.
    // ------------------------------------------------------------------
    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _isLeftPressed = Input.GetKey(MoveLeft);
        _isRightPressed = Input.GetKey(MoveRight);
        _isCrouch = Input.GetKey(MoveDown) && _isGround || Input.GetKey(MoveDown) && _isSlope;
        _isLookUp = Input.GetKey(MoveUp);

        _isAnyDirectionKeyPressed = _isLeftPressed || _isRightPressed;
        _isAnyDirectionKeyNotPressed = !Input.GetKey(MoveLeft) || !Input.GetKey(MoveRight);
        _isAnyDirectionKeyUp = Input.GetKeyUp(MoveLeft) || Input.GetKeyUp(MoveRight);
        
        // Buffer button-down events so FixedUpdate can't miss them.
        if (Input.GetKeyDown(JumpKey))
        {
            _jumpRequested = true;
        }
        if (Input.GetKeyDown(ShootKey) && !_allowShoot)
        {
            _shootRequested = true;
        }

        if (_horizontal > 0.1f) _direction = Direction.Left;
        else if (_horizontal < -0.1f) _direction = Direction.Right;

        // Shooting only spawns a pooled object / starts a coroutine - no rigidbody
        // interaction - so it's fine to resolve here using last-known ground state.
        HandleShoot();
    }

    // ------------------------------------------------------------------
    // FIXEDUPDATE: everything physics - ground/slope/wall queries,
    // rigidbody velocity, gravity toggling. Runs on the fixed timestep.
    // ------------------------------------------------------------------
    void FixedUpdate()
    {
        _jumpGuardTimer = Mathf.Max(0f, _jumpGuardTimer - Time.fixedDeltaTime);

        _isSlope = OnSlope(_bottomGroundDistnace, _groundLayerMask);
        _isGround = transform.CheckCircleSide(Vector2.down, _bottomGroundRadius, _bottomGroundDistnace, _groundLayerMask);
        if (!_isGround && _isSlope && _playerRb.velocity.y <= 0.1f)
        {
            _isGround = true;
        }
        _isCrouchOverDetected = transform.CheckCircleSide(Vector2.up,0.5f,0.5f, _overGroundLayerMask);
        _isCrouchUnderDetected = transform.CheckCircleSide(Vector2.up,0.5f,0.5f, _underGroundLayerMask);
        _isAnyCrouchUpDetected = _isCrouchOverDetected || _isCrouchUnderDetected;
        _isWallLeftCheck = transform.CheckBoxSide(Vector2.left, _sideBoxDistance, _sideBoxSize, _groundLayerMask);
        _isWallRightCheck = transform.CheckBoxSide(Vector2.right, _sideBoxDistance, _sideBoxSize, _groundLayerMask);
        _isAnyWallDetected = _isWallCheck && (_isWallLeftCheck || _isWallRightCheck);

        _movementSpeed = (_isCrouch && _isGround || _isCrouchOverDetected || _isCrouchUnderDetected) ? _crouchSpeed : _walkSpeed;

        HandleJump();

        if (_isGround && _jumpGuardTimer <= 0f && _playerRb.velocity.y <= 0.1f)
        {
            _isWallCheck = false;
        }

        if (!_isCrouch && _allowInput && (!_isLookUp || _isLookUp))
        {
            EndInputWindow();
        }

        bool isJumpingUp = _playerRb.velocity.y > 0.1f;

        if (!_isAnyDirectionKeyPressed && _isSlope && _isGround && !isJumpingUp && _jumpGuardTimer <= 0f)
        {
            _playerRb.velocity = Vector2.zero;
        }
        else if (_isAnyWallDetected && !_isGround && !isJumpingUp)
        {
            _playerRb.velocity = new Vector2(0, _playerRb.velocity.y);
        }

        if (_allowInput)
        {
            UseGravity(!_isGround);
            return;
        }

        HandleMovement();
        HandleCrouch();
        bool crouchState =  !_isCrouch && !_isAnyCrouchUpDetected || (_isCrouch && !_isGround);
        _mainCapsuleCollider.enabled = crouchState;
        UseGravity(!_isGround);
    }
    // ------------------------------------------------------------------
    // LATEUPDATE: animation / visual state, resolved after Update and
    // FixedUpdate have both finished for the frame.
    // ------------------------------------------------------------------
    void LateUpdate()
    {
        //flip x image depending on the key
        //update all animation logics here now.

        _fullRenderer.flipX = _direction == Direction.Right;
        _topRenderer.flipX = _direction == Direction.Right;
        _bottomRenderer.flipX = _direction == Direction.Right;
        //add anycorouch logic here.
        _playerFullAction.gameObject.SetActive(_isCrouch && _isGround || _isAnyCrouchUpDetected && _isGround);
        // !_isCrouch && !_isCrouchUnderDetected ||

        bool crouchState =  !_isCrouch && !_isAnyCrouchUpDetected || (_isCrouch && !_isGround);
        //you must edit maincapsule method here
        _playerTopAction.gameObject.SetActive(crouchState);
        _playerBotAction.gameObject.SetActive(crouchState);
    }
    //handle crouch method
    private void HandleCrouch()
    {
        if(_topCapsuleCollider == null || _mainCapsuleCollider == null){
            return;
        }
        _mainCapsuleCollider.enabled = !_isCrouch && !_isAnyCrouchUpDetected;
        _topCapsuleCollider.enabled = _isCrouch;
        _bottomCapsuleCollider.enabled = _isCrouch;
    }
    private void HandleJump()
    {
        bool jumpAllowed = _jumpRequested && _jumpCoroutine == null &&
            (_isGround || _isSlope || (int)_playerRb.gravityScale == 0);
        List<CapsuleCollider2D> colliders =  new List<CapsuleCollider2D>{_topCapsuleCollider, _bottomCapsuleCollider, _fullCapsuleCollider, _mainCapsuleCollider};
        // Consume the buffered request either way - it shouldn't persist across ticks.
        _jumpRequested = false;
        _isOverWall = false;
        //add wall over logic here
        if (_isCrouchOverDetected)
        {
            _isOverWall = true;
        }

        if (!jumpAllowed || _isCrouchUnderDetected) return;

        Action initOverAction = () =>
        {
            if (_isOverWall)
            {
                foreach(CapsuleCollider2D collider in colliders)
                {
                    collider.isTrigger = true;
                }
            }
        };
        Action endOverAction = () =>
        {
           if (_isOverWall)
            {
                 foreach(CapsuleCollider2D collider in colliders)
                {
                    collider.isTrigger = false;
                }
            }   
            _isOverWall = false;
            StopCoroutine(_overWallCoroutine);
            _overWallCoroutine = null;
        };
        Action wallEndAction = () =>
        {
            _isWallCheck = true;
            StopCoroutine(_wallCoroutine);
            _wallCoroutine = null;
        };

        Action endAction = () =>
        {
            _jumpCoroutine = null;
        };
        //init over wall logic
        _overWallCoroutine = StartCoroutine(DelayAction(0.8f,initOverAction, endOverAction));
        _wallCoroutine = StartCoroutine(DelayAction(0.1f, null, wallEndAction));
        _jumpCoroutine = StartCoroutine(DelayAction(_jumpDelayTime, null, endAction));

        _jumpGuardTimer = _jumpGuardDuration;
        _playerRb.velocity = new Vector2(_playerRb.velocity.x, _jumpForce * _forceAmount);
    }

    private void HandleMovement()
    {
        float inputX = _horizontal * _movementSpeed;
        if (_isGround && !_isSlope)
        {
            _mainCapsuleCollider.isTrigger = false;
            _mainCapsuleCollider.enabled = true;
            Vector2 movePosition = new Vector2(
                _isAnyDirectionKeyPressed && !_isAnyWallDetected ? inputX : 0.0f,
                _playerRb.velocity.y);
            _playerRb.velocity = movePosition;
            _isWallCheck = false;
        }
        else if (_isAnyDirectionKeyPressed && _isGround && _isSlope && !_isAnyWallDetected && _jumpGuardTimer <= 0f)
        {
            Vector2 slopeDirection = _slopePerpendicular;
            Vector2 desiredDirection = _horizontal > 0f ? Vector2.right : Vector2.left;
            if (Vector2.Dot(slopeDirection, desiredDirection) < 0f)
            {
                slopeDirection = -slopeDirection;
            }
            float slopeSpeed = Mathf.Abs(_horizontal) * _movementSpeed;
            Vector2 movePosition = slopeDirection * slopeSpeed;
            movePosition.x = _isAnyDirectionKeyPressed ? movePosition.x : 0.0f;
            _playerRb.velocity = movePosition;
        }
        else if (_isAnyDirectionKeyUp && _isGround && _isSlope && !_isAnyWallDetected && _jumpGuardTimer <= 0f)
        {
            _playerRb.velocity = Vector2.zero;
        }
        else if (!_isGround && !_isSlope)
        {
            Vector2 movePosition = new Vector2(
                !_isAnyWallDetected ? inputX : 0.0f,
                _playerRb.velocity.y);
            _playerRb.velocity = movePosition;
        }
    }

    private void HandleShoot()
    {
        bool onGroundMove = _isAnyDirectionKeyPressed && _isGround && !_isCrouch;
        bool offGroundMove = _isAnyDirectionKeyPressed && !_isGround && !_isCrouch;
        bool offGroundLookDown = _isAnyDirectionKeyPressed && !_isGround && _isCrouch;
        bool offGroundIdle = _isAnyDirectionKeyNotPressed && !_isGround;
        bool offGroundIdleLookDown = _isAnyDirectionKeyNotPressed && !_isGround && _isCrouch;
        bool onGroundCrouchMove = _isAnyDirectionKeyPressed && _isGround && _isCrouch;
        bool onGroundCrouchIdle = _isAnyDirectionKeyNotPressed && _isGround && _isCrouch;

        bool isShoot = _shootRequested;
        _shootRequested = false;
        if (!isShoot) return;

        if (onGroundMove && _isLookUp) Shoot(Direction.Up);
        else if (onGroundMove && !_isLookUp) Shoot(_direction);
        else if (offGroundMove && !_isLookUp) Shoot(_direction);
        else if (offGroundMove && _isLookUp) Shoot(Direction.Up);
        else if (offGroundLookDown && !_isLookUp) Shoot(Direction.Down);
        else if (offGroundIdle && _isLookUp) Shoot(Direction.Up);
        else if (offGroundIdle && !_isLookUp) Shoot(_direction);
        else if (offGroundIdleLookDown && !_isLookUp) Shoot(_direction);
        else if (onGroundCrouchMove && !_isLookUp)
        {
            if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(DelayAction(_inputDelayTime, StartInputWindow, EndInputWindow));
            Shoot(_direction);
        }
        else if (onGroundCrouchIdle && !_isLookUp)
        {
            if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(DelayAction(_inputDelayTime, StartInputWindow, EndInputWindow));
            Shoot(_direction);
        }
    }

    private void StartInputWindow()
    {
        _allowInput = true;
    }

    private void EndInputWindow()
    {
        if (_inputCoroutine != null)
        {
            StopCoroutine(_inputCoroutine);
            _inputCoroutine = null;
        }
        _allowInput = false;
    }

    private IEnumerator DelayAction(float delaySeconds, Action onStartAction, Action onEndAction)
    {
        onStartAction?.Invoke();
        yield return new WaitForSeconds(delaySeconds);
        onEndAction?.Invoke();
    }

    private void Shoot(Direction direction)
    {
        GameObject bullet = ObjectPool.Instance.GetObjectPool();
        if (_shootCoroutine == null && bullet != null)
        {
            Action startAction = () => { _allowShoot = true; };
            Action endAction = () =>
            {
                _shootCoroutine = null;
                _allowShoot = false;
            };
            _shootCoroutine = StartCoroutine(DelayAction(_shootDelayTime, startAction, endAction));
        }
    }

    private void UseGravity(bool isGravity)
    {
        _playerRb.gravityScale = isGravity ? _gravityScale : 0.0f;
    }

    private bool OnSlope(float distance, LayerMask layerMask)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (_mainCapsuleCollider.size.y * 0.05f - _bottomGroundRadius);
        RaycastHit2D hitInfo = Physics2D.CircleCast(origin, _bottomGroundRadius, Vector2.down, distance + _bottomGroundRadius, layerMask);
        if (hitInfo.collider != null)
        {
            _slopePerpendicular = new Vector2(hitInfo.normal.y, -hitInfo.normal.x).normalized;
            float angle = Vector2.Angle(hitInfo.normal, Vector2.up);
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.blue);
            Debug.DrawRay(hitInfo.point, _slopePerpendicular, Color.cyan);
            return angle > 0f && angle <= _slopeMax;
        }
        _slopePerpendicular = Vector2.zero;
        return false;
    }

    private void OnDrawGizmos()
    {
        if(!_enableVisual)
        {
            return;
        }
        Gizmos.color = Color.cyan;
        transform.DrawRaySphere(Vector2.up, _topHeadDistance, _topHeadRadius);
        transform.DrawRaySphere(Vector2.down, _bottomGroundDistnace, _bottomGroundRadius);
        Gizmos.color = Color.red;
        transform.DrawRayBox(Vector2.right, _sideBoxDistance, _sideBoxSize);
        transform.DrawRayBox(Vector2.left, _sideBoxDistance, _sideBoxSize);
        Gizmos.color = Color.green;
        transform.DrawRaySphere(Vector2.down, 0.5f,0.5f);
        if (!_isCrouch && !_isAnyCrouchUpDetected)
        {
             transform.DrawRaySphere(Vector2.up, 0.5f,0.5f);
        }
        if(_isAnyCrouchUpDetected){
            Gizmos.color = Color.red;
            transform.DrawRaySphere(Vector2.up,0.5f,0.5f);
        }
      
        
    }
}