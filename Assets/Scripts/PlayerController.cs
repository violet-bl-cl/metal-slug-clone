using System;
using System.Collections;
using UnityEngine;

public class PlayerController : InputManager
{
    #region OpenFields
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    [SerializeField]
    private float _groundRay = 0.5f;
    [SerializeField]
    private LayerMask _layerMasks;
    #endregion

    private float _forceAmount = 100.0f;
    private CapsuleCollider2D _playerCapsule;
    private float _playerHeight;
    private Rigidbody2D _playerRb;
    private Coroutine _jumpCoroutine;

    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCapsule = GetComponent<CapsuleCollider2D>();
        _playerHeight = 1 / _playerCapsule.size.y;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0.0f)
        {
            Vector2 newPosition = new Vector2((horizontal * _forceAmount * _movementSpeed), 0.0f);
            _playerRb.velocity = new Vector2(newPosition.x * Time.deltaTime, _playerRb.velocity.y);
        }
        if (Input.GetKeyDown(JumpKey) && IsGround() && _jumpCoroutine == null)
        {
            _playerRb.velocity = Vector2.zero;
            _jumpCoroutine = StartCoroutine(nameof(DelayJump));
            Vector2 jumpAmount = Vector2.up * _jumpForce * _forceAmount * Time.deltaTime;
            _playerRb.velocity = jumpAmount;
        }
    }
    private bool IsGround()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, -Vector2.up, _groundRay + _playerHeight, _layerMasks);
        return hitInfo.collider != null;
    }

    private IEnumerator DelayJump(){
        yield return new WaitForSeconds(0.3f);
        StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, _groundRay + _playerHeight, 0));
    }
}
