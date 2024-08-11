using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : InputManager
{
    [SerializeField, Range(0.0f, 10.0f)]
    private float _movementSpeed = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpForce = 0.5f;
    private Rigidbody2D _playerrb;
    // Start is called before the first frame update
    void Start()
    {
        _playerrb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        //_playerrb.velocity = horizontal * _movementSpeed* Time.deltaTime * Vector2.right;  
        //_playerrb.AddForce((horizontal * _movementSpeed) * Vector2.right, ForceMode2D.Impulse);
        transform.Translate(horizontal * _movementSpeed * Vector2.right * Time.deltaTime);
        if (Input.GetKeyDown(JumpKey))
        {
            Vector2 jumpAmount = Vector2.up * _jumpForce * 100 * Time.deltaTime;
            _playerrb.velocity = jumpAmount;
        }
    }
}
