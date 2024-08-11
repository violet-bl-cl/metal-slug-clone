using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class PlayerController : InputManager
{
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    private Rigidbody2D _playerrb;
    [SerializeField]
    private float _groundRay = 0.5f;
    [SerializeField]
    private LayerMask _layerMasks;
    // Start is called before the first frame update
    void Start()
    {
        _playerrb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0.0f)
        {
            Vector2 newPosition = new Vector2((horizontal * 100 * _movementSpeed *Time.deltaTime),0f);
            _playerrb.velocity = new Vector2(newPosition.x,_playerrb.velocity.y) ;
        }
        if (Input.GetKeyDown(JumpKey) ) //&& IsGround()
        {
            Vector2 jumpAmount = Vector2.up * _jumpForce * 100 * Time.deltaTime;
            _playerrb.velocity = jumpAmount;
        }
    }
    private bool IsGround()
    {
        return Physics.Raycast(transform.up, Vector2.down, _groundRay, _layerMasks);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, -transform.up * _groundRay);
    }
}
