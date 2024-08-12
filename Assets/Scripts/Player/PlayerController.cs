using System;
using System.Collections;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : InputManager
{
    #region OpenFields
    [SerializeField, Range(0.0f, 360.0f)]
    private float _slopeMax;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    [SerializeField]
    private float _groundRay = 0.5f;
    [SerializeField]
    private LayerMask _layerMasks;
    [SerializeField]
    private LayerMask _slopeLayerMask;
    #endregion

    private float _sideSphereRadius = 0.6f;
    private float _sideSphereDistance = 0.5f;
    private Vector2 _sideBoxSize = new Vector2(0.2f, 1.5f);
    private float _sideBoxDistance = 0.5f;
    private float _forceAmount = 100.0f;
    private float _jumpDelayTime = 0.2f;
    private CapsuleCollider2D _playerCapsule;
    private float _playerHeight;
    private float _gravityScale = 0.0f;
    private Rigidbody2D _playerRb;
    private Coroutine _jumpCoroutine;
    private Vector2 _slopePerpendicular;


    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCapsule = GetComponent<CapsuleCollider2D>();
        _playerHeight = 1 / _playerCapsule.size.y;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        // Player movement controller
        bool isLeftPressed = !CheckBoxSide(Vector2.left, _sideBoxDistance) && Input.GetKey(MoveLeft);
        bool isRightPressed = !CheckBoxSide(Vector2.right, _sideBoxDistance) && Input.GetKey(MoveRight);
        bool isSlope = CheckSlope(-Vector2.up);
        if ((isLeftPressed || isRightPressed) && !isSlope)
        {
            Vector2 movePosition = new Vector2(horizontal * _forceAmount * _movementSpeed, 0.0f);
            _playerRb.gravityScale = 2.0f;
            _playerRb.velocity = new Vector2(movePosition.x * Time.fixedDeltaTime, _playerRb.velocity.y);
        }
        else if ((isLeftPressed || isRightPressed) && isSlope)
        {

            Vector2 slopeDirection = new Vector2(horizontal * _forceAmount * _movementSpeed * -_slopePerpendicular.x, -_slopePerpendicular.y * _movementSpeed);
            Vector2 movePosition = new Vector2(slopeDirection.x * Time.fixedDeltaTime, _playerRb.velocity.y);
            Debug.Log("On Slope");
            _playerRb.gravityScale = 0.0f;
            _playerRb.velocity = movePosition;
        }
        else if ((Input.GetKeyUp(MoveLeft) || Input.GetKeyUp(MoveRight)) && isSlope)
        {
            _playerRb.velocity = Vector2.zero;
        }
        //Player Jump 
        if (Input.GetKeyDown(JumpKey) && IsGround(-Vector2.up, _groundRay) && _jumpCoroutine == null)
        {
            _jumpCoroutine = StartCoroutine(nameof(DelayJump));
            Vector2 jumpAmount = Vector2.up * _jumpForce * _forceAmount * Time.fixedDeltaTime;
            _playerRb.velocity = jumpAmount;
            _playerRb.gravityScale = 2.0f;
        }
        else if (!IsGround(-Vector2.up, _groundRay))
        {
            _playerRb.velocity = Vector2.zero;
            _playerRb.gravityScale = 0.0f;
        }
    }
    private bool CheckSlope(Vector2 direction)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction, _groundRay + _playerHeight, _slopeLayerMask);
        if (hitInfo.collider != null)
        {
            _slopePerpendicular = Vector2.Perpendicular(hitInfo.normal).normalized;
            float angle = Vector2.Angle(hitInfo.normal, direction);
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
            Debug.DrawRay(hitInfo.point, _slopePerpendicular, Color.cyan);
            return angle != 0 && angle < _slopeMax;
        }
        _slopePerpendicular = Vector2.zero;
        return false;
    }
    private bool CheckCapsuleSide(Vector2 direction, float distance)
    {
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, _sideSphereRadius, direction, distance, _layerMasks);
        return hitInfo.collider != null;
    }
    private bool CheckBoxSide(Vector2 direction, float distance)
    {
        RaycastHit2D hitInfo = Physics2D.BoxCast(transform.position, _sideBoxSize, 0f, direction, distance, _layerMasks);
        return hitInfo.collider != null;
    }
    private bool IsGround(Vector2 direction, float distance)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction, distance + _playerHeight, _layerMasks + _slopeLayerMask);
        return hitInfo.collider != null;
    }
    private IEnumerator DelayJump()
    {
        yield return new WaitForSeconds(_jumpDelayTime);
        StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = null;
    }
    private void OnDrawGizmos()
    {
        DrawUtility.SetTransform(transform);
        DrawUtility.DrawRaySphere(Vector2.up, _sideSphereDistance, _sideSphereRadius);
        DrawUtility.DrawRayBox(Vector2.right, _sideBoxDistance, _sideBoxSize);
        DrawUtility.DrawRayBox(Vector2.left, _sideBoxDistance, _sideBoxSize);
    }
}
