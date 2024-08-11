using System;
using UnityEngine;

public class PlayerController : InputManager
{
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    private CapsuleCollider2D _playerCapsule;
    private float _playerHeight;
    private Rigidbody2D _playerRb;
    private bool _allowJump;
    [SerializeField]
    private float _groundRay = 0.5f;
    [SerializeField]
    private LayerMask _layerMasks;
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerCapsule = GetComponent<CapsuleCollider2D>();
        _playerHeight = 1 / _playerCapsule.size.y;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0.0f)
        {
            Vector2 newPosition = new Vector2((horizontal * 100 * _movementSpeed), 0f);
            _playerRb.velocity = new Vector2(newPosition.x * Time.deltaTime, _playerRb.velocity.y);
        }

        if (Input.GetKeyDown(JumpKey) && IsGround() && !_allowJump)
        {
            _allowJump = true;
            Vector2 jumpAmount = Vector2.up * _jumpForce * 100 * Time.deltaTime;
            _playerRb.velocity = jumpAmount;
        }
    }
    private bool IsGround()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, -Vector2.up, _groundRay + _playerHeight, _layerMasks);
        if (hitInfo.collider != null)
        {
            Invoke(nameof(AllowJump) , 0.5f);
            return true;
        }
        return false;
    }
    private void AllowJump(){
        _allowJump = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, _groundRay + _playerHeight, 0));
    }
}
