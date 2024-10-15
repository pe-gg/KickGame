using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 100f;
    [SerializeField] private float _friction = 0.2f;
    private Rigidbody _rb;
    private Vector2 _movement;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void HandleMovementInput(Vector2 input)
    {
        _movement = input;
    }

    private void Move()
    {
        Vector3 _horiz = (transform.right * _movement.x + transform.forward * _movement.y) * _movementSpeed;
        _rb.AddForce(_horiz * (Mathf.Lerp(0f, _movementSpeed, _friction)) * Time.fixedDeltaTime, ForceMode.Force);
    }
}
