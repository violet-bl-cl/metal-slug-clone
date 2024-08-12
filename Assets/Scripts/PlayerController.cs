using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
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

    private float _sideSphereRadius = 0.6f;
    private float _sideSphereDistance = 0.5f;
    private Vector2 _sideBoxSize = new Vector2(0.4f, 1.5f);
    private float _sideBoxDistance = 0.5f;
    private float _forceAmount = 100.0f;
    private float _jumpDelayTime = 0.3f;
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
        // Player movement controller
        if (horizontal != 0.0f)
        {
            if (!CheckBoxSide(Vector2.left, _sideBoxDistance) && Input.GetKey(KeyCode.A) || !CheckBoxSide(Vector2.right, _sideBoxDistance) && Input.GetKey(KeyCode.D))
            {
                Vector2 newPosition = new Vector2((horizontal * _forceAmount * _movementSpeed), 0.0f);
                _playerRb.velocity = new Vector2(newPosition.x * Time.deltaTime, _playerRb.velocity.y);
            }

        }
        //Player Jump 
        if (Input.GetKeyDown(JumpKey) && IsGround(-Vector2.up) && _jumpCoroutine == null)
        {
            _playerRb.velocity = Vector2.zero;
            _jumpCoroutine = StartCoroutine(nameof(DelayJump));
            Vector2 jumpAmount = Vector2.up * _jumpForce * _forceAmount * Time.deltaTime;
            _playerRb.velocity = jumpAmount;
        }
    }
    private bool CheckCapsuleSide(Vector2 direction, float distance)
    {
        RaycastHit2D hitInfo = Physics2D.CircleCast(transform.position, _sideSphereRadius, direction, distance, _layerMasks);
        if (hitInfo.collider != null)
        {
            Debug.Log(hitInfo.transform.name + $"{direction}");
        }
        return hitInfo.collider != null;
    }

    private bool CheckBoxSide(Vector2 direction, float distance)
    {
        RaycastHit2D hitInfo = Physics2D.BoxCast(transform.position, _sideBoxSize, 0f, direction, distance, _layerMasks);
        return hitInfo.collider != null;
    }
    private bool IsGround(Vector2 direction)
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, direction, _groundRay + _playerHeight, _layerMasks);
        if (hitInfo.collider != null) Debug.Log(hitInfo.collider.transform.name);
        return hitInfo.collider != null;
    }

    private IEnumerator DelayJump()
    {
        yield return new WaitForSeconds(_jumpDelayTime);
        StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = null;
    }

    private void DrawRayBox(Vector2 direction, float distance, Vector2 boxSize)
    {
        Gizmos.color = Color.green;
        Vector2 start = (Vector2)transform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, boxSize);
    }
    private void DrawRaySphere(Vector2 direction, float distance, float sphereRadius)
    {
        Gizmos.color = Color.red;
        Vector2 start = (Vector2)transform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, sphereRadius);
    }
    private void DrawyRayLine(Vector2 direction, float distance){
        Gizmos.color = Color.cyan;
        Vector2 start = (Vector2)transform.position;
        Vector2 end = start + direction * distance;
        Gizmos.DrawLine(start,end);
    }
    private void OnDrawGizmos()
    {
        DrawRaySphere(Vector2.right, _sideSphereDistance,_sideSphereRadius);
        DrawRayBox(Vector2.right, _sideBoxDistance, _sideBoxSize);
        DrawRayBox(Vector2.left, _sideBoxDistance, _sideBoxSize);
        DrawyRayLine(-Vector2.up,_groundRay + _playerHeight);
    }
}
