using System;
using System.Collections;
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
    private bool _enableVisual = false;
    #endregion
    [SerializeField]
    private Vector2 _sideBoxSize = new Vector2(0.2f, 1.3f);
    private float _sideBoxDistance = 0.5f;
    private float _bottomGroundRadius = 0.1f;
    [SerializeField]
    private float _bottomGroundDistnace = 1.1f;
    private Vector2 _topHeadBoxSize = new Vector2(0.4f, 1.5f);
    private float _topHeadBoxDistance = 0.5f;
    private float _topHeadRadius = 0.5f;
    private float _topHeadDistance = 0.8f;
    private float _forceAmount = 100.0f;
    private float _jumpDelayTime = 0.4f;
    private bool _allowInput = false;
    private bool _allowShoot = false;
    private float _inputDelayTime = 0.8f;
    private float _shootDelayTime = 0.05f;
    private CapsuleCollider2D _playerCapsule;
    private float _playerHeight;
    private Rigidbody2D _playerRb;
    private Coroutine _jumpCoroutine;
    private Coroutine _inputCoroutine;
    private Coroutine _shootCoroutine;
    private Vector2 _slopePerpendicular;
    private Direction _direction;
    //Animation State Machine
    private PlayerTopAction _playerTopAction;
    private PlayerBottomAction _playerBotAction;
    private PlayerFullAction _playerFullAction;
    // player controller;
    //PlayerAction Status.
    private bool _isLeftPressed, _isRightPressed;
    [SerializeField] private bool _isGround, _isSlope;
    private bool _isCrouch, _isLookUp;
    private bool _isShoot;
    private bool _isAnyDirectionKeyPressed, _isAnyDirectionKeyNotPressed;
    private float _horizontal;
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCapsule = GetComponent<CapsuleCollider2D>();
        _playerFullAction = transform.GetComponentInChildren<PlayerFullAction>(true);
        _playerBotAction = transform.GetComponentInChildren<PlayerBottomAction>(true);
        _playerTopAction = transform.GetComponentInChildren<PlayerTopAction>(true);
        _playerHeight = _playerCapsule.size.y / 2;
    }
    void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _isLeftPressed = !transform.CheckBoxSide(Vector2.left, _sideBoxDistance, _sideBoxSize, _groundLayerMask) && Input.GetKey(MoveLeft);
        _isRightPressed = !transform.CheckBoxSide(Vector2.right, _sideBoxDistance, _sideBoxSize, _groundLayerMask) && Input.GetKey(MoveRight);
        _isGround = transform.CheckCircleSide(Vector2.down, _bottomGroundRadius, _bottomGroundDistnace, _groundLayerMask);
        _isSlope = CheckSphereSlope(-Vector2.up, _bottomGroundRadius, _bottomGroundDistnace, _groundLayerMask);
        _isCrouch = Input.GetKey(MoveDown);
        _isLookUp = Input.GetKey(MoveUp);
        _isAnyDirectionKeyNotPressed = !_isLeftPressed || !_isRightPressed;
        _isAnyDirectionKeyPressed = _isLeftPressed || _isRightPressed;
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
        if (Input.GetKeyDown(JumpKey) && _isGround && _jumpCoroutine == null)
        {
            Action endAction = () =>
            {
                StopCoroutine(_jumpCoroutine);
                _jumpCoroutine = null;
                _playerCapsule.isTrigger = false;
            };
            _jumpCoroutine = StartCoroutine(DelayAction(_jumpDelayTime, null, endAction));
            Vector2 jumpAmount = Vector2.up * _jumpForce * _forceAmount * Time.fixedDeltaTime;
            _playerRb.velocity = jumpAmount;
            _playerRb.gravityScale = 2.0f;
            _playerCapsule.isTrigger = true;
        }
        if (!_isCrouch && _allowInput && (!_isLookUp || _isLookUp))
        {
            endInputAction();
        }

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
    }

    void FixedUpdate()
    {
        //fIX PLAYER CONTORLLER HERE.
        if (_isAnyDirectionKeyPressed && !_isSlope && !_allowInput)
        {
            Vector2 movePosition = new Vector2(_horizontal * _forceAmount * _movementSpeed, 0.0f);
            _playerRb.isKinematic = false;
            //_playerRb.gravityScale = 2.0f;
            _playerRb.velocity = new Vector2(movePosition.x * Time.fixedDeltaTime, _playerRb.velocity.y);
        }
        else if (_isAnyDirectionKeyPressed && _isSlope && !_allowInput)
        {
            float inputX = _horizontal * _movementSpeed * Time.fixedDeltaTime;
            Vector2 slopePosition = GetSlopePosition(Vector2.down,_bottomGroundDistnace, _groundLayerMask );
            slopePosition.x = inputX + transform.localPosition.x;
             Vector2 movePosition = new Vector2(slopePosition.x, slopePosition.y);
             _playerRb.isKinematic = true;
             //_playerRb.gravityScale = 2.0f;
             transform.localPosition = movePosition;
             Debug.Log(slopePosition);
        // Vector2 slopeDirection = new Vector2(_horizontal * _forceAmount * _movementSpeed * -_slopePerpendicular.x, -_slopePerpendicular.y * _movementSpeed);
        // Vector2 movePosition = new Vector2(slopeDirection.x * Time.fixedDeltaTime, _playerRb.velocity.y);
        // _playerRb.velocity = movePosition;
        }
        else if (_isAnyDirectionKeyNotPressed && _isSlope)
        {
            _playerRb.velocity = Vector2.zero;
        }

        //Vector2 fullPosiition = _isLeftPressed ? new Vector2(0.34f, -1.0f) : new Vector2(-0.34f, -1.0f);
        //Vector2 topPosition = _isLeftPressed ? new Vector2(0.34f, 0.6f) : new Vector2(-0.34f, 0.6f);
        //Vector2 botPosition = _isLeftPressed ? new Vector2(0.35f, -0.3f) : new Vector2(-0.35f, -0.3f);
//
        //SpriteHelper.ChangeSpritePosition(_playerFullAction.gameObject, _isLeftPressed, fullPosiition);
        //SpriteHelper.ChangeSpritePosition(_playerTopAction.gameObject, _isLeftPressed, topPosition);
        //SpriteHelper.ChangeSpritePosition(_playerBotAction.gameObject, _isLeftPressed, botPosition);

        if (_isLeftPressed) _direction = Direction.Left;
        else if (_isRightPressed) _direction = Direction.Right;

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
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction , distance, layerMask);
        Vector3 slopePlayerPosition = transform.localPosition;
        if(hitInfo.collider != null){
            slopePlayerPosition.y = hitInfo.point.y;
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.green);
        }
        return slopePlayerPosition;
    }
    private bool CheckSphereSlope(Vector2 direction, float radius, float distance, LayerMask layerMask)
    {
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, radius, direction, distance, layerMask);
        if (hitInfo.collider != null)
        {
            _slopePerpendicular = Vector2.Perpendicular(hitInfo.normal).normalized;
            float angle = Vector2.Angle(hitInfo.normal, direction);
            //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
            Debug.DrawRay(hitInfo.point, _slopePerpendicular, Color.cyan);            
            return angle != 0 && angle < _slopeMax;
        }
        _slopePerpendicular = Vector2.zero;
        return false;
    }

    private void OnDrawGizmos()
    {
        transform.DrawRaySphere(Vector2.up, _topHeadDistance, _topHeadRadius);
        transform.DrawRaySphere(Vector2.down, _bottomGroundDistnace, _bottomGroundRadius);
        transform.DrawRayBox(Vector2.up, _topHeadBoxDistance, _topHeadBoxSize);
        transform.DrawRayBox(Vector2.right, _sideBoxDistance, _sideBoxSize);
        transform.DrawRayBox(Vector2.left, _sideBoxDistance, _sideBoxSize);
        //transform.DrawyRayLine(Vector2.down, _playerHeight + _groundRay);
        transform.DrawCapsule(_playerHeight, 0.5f);
    }
}
