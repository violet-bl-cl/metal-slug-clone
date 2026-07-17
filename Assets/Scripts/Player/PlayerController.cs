using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

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
    private CapsuleCollider2D _playerCapsule;
    private float _playerHeight;
    private Rigidbody2D _playerRb;
    private Coroutine _jumpCoroutine;
    private Coroutine _inputCoroutine;
    private Coroutine _shootCoroutine;
    private Coroutine _wallCoroutine;
    private Vector2 _slopePerpendicular;
    private Direction _direction;
    //Animation State Machine
    private PlayerTopAction _playerTopAction;
    private PlayerBottomAction _playerBotAction;
    private PlayerFullAction _playerFullAction;
    // player controller;
    //PlayerAction Status.
    [SerializeField]
    private bool _isLeftPressed, _isRightPressed;
    [SerializeField] private bool _isGround, _isSlope;
    private bool _isCrouch, _isLookUp, _isJumping;
    //activate wall check
    [SerializeField]
    private bool _isWallLeftCheck = false, _isWallRightCheck = false, _isAnyWallDetected;
    [SerializeField]
    private bool _isWallCheck = false;
    private bool _isShoot;
    private bool _isAnyDirectionKeyPressed, _isAnyDirectionKeyNotPressed, _isAnyDirectionKeyUp;
    private float _horizontal;
    private Vector3 _capusleSize = new Vector3();
    private TextMeshPro _debugSlopeText;
    void Awake()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCapsule = GetComponent<CapsuleCollider2D>();
        _playerFullAction = transform.GetComponentInChildren<PlayerFullAction>(true);
        _playerBotAction = transform.GetComponentInChildren<PlayerBottomAction>(true);
        _playerTopAction = transform.GetComponentInChildren<PlayerTopAction>(true);
        _debugSlopeText = GetComponentInChildren<TextMeshPro>(true);
    }
    void Start()
    {
        _capusleSize.Set(0, _playerCapsule.size.y / 2,0); 
    }
    void Update()
    {
        // decrement jump guard timer
        _jumpGuardTimer = Mathf.Max(0f, _jumpGuardTimer - Time.deltaTime);
        _isSlope = OnSlope(_bottomGroundDistnace, _groundLayerMask);
        _isGround = transform.CheckCircleSide(Vector2.down, _bottomGroundRadius, _bottomGroundDistnace, _groundLayerMask);
        if (!_isGround && _isSlope && _playerRb.velocity.y <= 0.1f)
        {
            _isGround = true;
        }
        _isJumping = Input.GetKeyDown(JumpKey) && _isGround && _jumpCoroutine == null ||
         Input.GetKeyDown(JumpKey) && _isSlope && _jumpCoroutine == null || 
         Input.GetKeyDown(JumpKey) &&  (int)_playerRb.gravityScale == 0 && _jumpCoroutine == null;
        _horizontal = Input.GetAxisRaw("Horizontal");
        //change the logic here
        _isWallLeftCheck = transform.CheckBoxSide(Vector2.left, _sideBoxDistance, _sideBoxSize, _groundLayerMask);
        _isWallRightCheck = transform.CheckBoxSide(Vector2.right, _sideBoxDistance, _sideBoxSize, _groundLayerMask);
        _isAnyWallDetected = _isWallCheck && (_isWallLeftCheck || _isWallRightCheck);
        _isLeftPressed = Input.GetKey(MoveLeft);
        _isRightPressed = Input.GetKey(MoveRight);
        _isCrouch = Input.GetKey(MoveDown);
        _isLookUp = Input.GetKey(MoveUp);
        //add new jump logic here

        _isAnyDirectionKeyPressed = _isLeftPressed || _isRightPressed;
        _isAnyDirectionKeyNotPressed = !Input.GetKey(MoveLeft) || !Input.GetKey(MoveRight);
        _isAnyDirectionKeyUp = Input.GetKeyUp(MoveLeft) || Input.GetKeyUp(MoveRight);
        _movementSpeed = (_isCrouch && _isGround) ? _crouchSpeed : _walkSpeed;
        bool onGroundMove = _isAnyDirectionKeyPressed && _isGround && !_isCrouch;
        bool offGroundMove = _isAnyDirectionKeyPressed && !_isGround && !_isCrouch;
        bool offGroundLookDown = _isAnyDirectionKeyPressed && !_isGround && _isCrouch;
        bool offGroundIdle = _isAnyDirectionKeyNotPressed && !_isGround;
        bool offGroundIdleLookDown = _isAnyDirectionKeyNotPressed && !_isGround && _isCrouch;
        bool onGroundCrouchMove = _isAnyDirectionKeyPressed && _isGround && _isCrouch;
        bool onGroundCrouchIdle = _isAnyDirectionKeyNotPressed && _isGround && _isCrouch;
        Action startInputAction = () =>
                  {
                      _allowInput = true;
                  };
        Action endInputAction = () =>
        {
            StopCoroutine(_inputCoroutine);
            _inputCoroutine = null;
            _allowInput = false;
        };
        if (_isJumping)
        {
            Debug.Log("jump Pressed!");
            Action wallEndAction = () =>{
                _isWallCheck = true;
                StopCoroutine(_wallCoroutine);
                _wallCoroutine = null;
            };

            Action endAction = () =>
            {
                StopCoroutine(_jumpCoroutine);
                _jumpCoroutine = null;
            };
            _wallCoroutine = StartCoroutine(DelayAction(0.1f, null, wallEndAction));
            _jumpCoroutine = StartCoroutine(DelayAction(_jumpDelayTime, null, endAction));
            // start short guard to prevent other code from overwriting jump velocity
            _jumpGuardTimer = _jumpGuardDuration;

            _playerRb.velocity = new Vector2(_playerRb.velocity.x, _jumpForce * _forceAmount);
        }
        if (_isGround && _jumpGuardTimer <= 0f && _playerRb.velocity.y <= 0.1f)
        {
            _isWallCheck = false;
        }
        if (!_isCrouch && _allowInput && (!_isLookUp || _isLookUp))
        {
            endInputAction();
        }

        bool isJumpingUp = _playerRb.velocity.y > 0.1f;

        // Gravity conditions rebuilding this conditions
        if (!_isAnyDirectionKeyPressed && _isSlope && _isGround && !isJumpingUp && _jumpGuardTimer <= 0f)
        {
            _playerRb.velocity = Vector2.zero;
        }
        // else if (_isAnyDirectionKeyPressed && _isSlope)
        // {
        //     // Debug.Log("OnSlope and pressed");

        // }
        else if (_isAnyWallDetected && !_isGround && !isJumpingUp)
        {
            _playerRb.velocity = new Vector2(0, _playerRb.velocity.y);
        }
       

        // Shooting Mechanics.

        if (onGroundMove && _isShoot && _isLookUp)
        {
            Shoot(Direction.Up);
        }
        else if (onGroundMove && _isShoot && !_isLookUp)
        {
            Shoot(_direction);
        }
        else if (offGroundMove && _isShoot && !_isLookUp)
        {
            Shoot(_direction);
        }
        else if (offGroundMove && _isShoot && _isLookUp)
        {
            Shoot(Direction.Up);
        }
        else if (offGroundLookDown && _isShoot && !_isLookUp)
        {
            Shoot(Direction.Down);
        }
        else if (offGroundIdle && _isShoot && _isLookUp)
        {
            Shoot(Direction.Up);
        }
        else if (offGroundIdle && _isShoot && !_isLookUp)
        {
            Shoot(_direction);
        }
        else if (offGroundIdleLookDown && _isShoot && !_isLookUp)
        {
            Shoot(_direction);
        }
        else if (onGroundCrouchMove && _isShoot && !_isLookUp)
        {
            if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(DelayAction(_inputDelayTime, startInputAction, endInputAction));
            Shoot(_direction);
        }
        else if (onGroundCrouchIdle && _isShoot && !_isLookUp)
        {
            if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(DelayAction(_inputDelayTime, startInputAction, endInputAction));
            Shoot(_direction);
        }

         if (_allowInput) return;
        //if the player is not on the ground or on the ground.
        float inputX = _horizontal * _movementSpeed;
        bool isJumping = _playerRb.velocity.y > 0.1f;
        if (_isGround && !_isSlope)
        {
            Vector2 movePosition = new Vector2(_isAnyDirectionKeyPressed && !_isAnyWallDetected ? inputX : 0.0f,_playerRb.velocity.y);
            _playerRb.velocity = movePosition;
            _isWallCheck = false;
        }
        // when the key has been pressed and on ground, on slope.
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
            _debugSlopeText.color = Color.green;
            _debugSlopeText.text = "Slope: " + movePosition + "\n" + "Player Pos" + transform.localPosition;
            movePosition.x = _isAnyDirectionKeyPressed ? movePosition.x : 0.0f;
            _playerRb.velocity = movePosition;
        }
        //fix the jumping issue.
        else if(_isAnyDirectionKeyUp && _isGround && _isSlope && !_isAnyWallDetected && _jumpGuardTimer <= 0f){
            _playerRb.velocity = Vector2.zero;
        }
        //add when the key is not pressed
        else if (!_isGround && !_isSlope)
        {
             Vector2 movePosition = new Vector2(!_isAnyWallDetected ? inputX : 0.0f,_playerRb.velocity.y);
            _playerRb.velocity = movePosition;
        }
        //add key up condition to set this vector.zero;
        


        if (_horizontal > 0.1f) _direction = Direction.Left;
        else if (_horizontal < -0.1f) _direction = Direction.Right;

        UseGravity(!_isGround);

        _playerFullAction.gameObject.SetActive(_isCrouch && _isGround);
        _playerBotAction.gameObject.SetActive(!_isCrouch || (_isCrouch && !_isGround));
        _playerTopAction.gameObject.SetActive(!_isCrouch || (_isCrouch && !_isGround));
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
            Action startAction = () =>
            {
                _allowShoot = true;
            };
            Action endAction = () =>
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
                _allowShoot = false;
            };
            _shootCoroutine = StartCoroutine(DelayAction(_shootDelayTime, startAction, endAction));
        }
    }
    private Vector3 GetSlopePosition(Vector2 direction, float distance, LayerMask layerMask)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction, distance, layerMask);
        Vector3 slopePlayerPosition = transform.localPosition;
        if (hitInfo.collider != null)
        {
            Vector2 slopeNormal = hitInfo.normal;
            slopePlayerPosition = new Vector2(slopeNormal.x, -slopeNormal.y).normalized;
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.green);
        }
        return slopePlayerPosition;
    }
    private void UseGravity(bool isActive)
    {
        _playerRb.gravityScale = isActive ? _gravityScale : 0.0f;
    }
    private bool OnSlope(float distance, LayerMask layerMask)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (_playerCapsule.size.y * 0.05f - _bottomGroundRadius);
        // Use a circle cast to better detect slopes under the player's feet
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
        transform.DrawRaySphere(Vector2.up, _topHeadDistance, _topHeadRadius);
        transform.DrawRaySphere(Vector2.down, _bottomGroundDistnace, _bottomGroundRadius);
        transform.DrawRayBox(Vector2.right, _sideBoxDistance, _sideBoxSize);
        transform.DrawRayBox(Vector2.left, _sideBoxDistance, _sideBoxSize);
        transform.DrawCapsule(_playerHeight, 0.5f);
    }
}
