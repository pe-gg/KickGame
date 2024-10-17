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
        VelocityDampen();
    }

    public void HandleMovementInput(Vector2 input)
    {
        _movement = input;
    }

    private void Move()
    {
        Vector3 _horiz = (transform.right * _movement.x + transform.forward * _movement.y) * _movementSpeed;
        _rb.AddForce(_horiz * (Mathf.Clamp(Mathf.Lerp(_movementSpeed, 0f, _friction), 0f, 20f)) * Time.fixedDeltaTime, ForceMode.Force);
    }

    private void VelocityDampen() 
    {
        float xSlow = _rb.velocity.x;
        float zSlow = _rb.velocity.z;
        if(_movement.x == 0)
        {
            xSlow = 0f;
        }
        else
        {
            xSlow = _rb.velocity.x;
        }
        if(_movement.y == 0)
        {
            zSlow = 0f;
        }
        else
        {
            zSlow = _rb.velocity.z;
        }
        _rb.velocity = new Vector3(Mathf.Lerp(_rb.velocity.x, xSlow, 0.1f), _rb.velocity.y, Mathf.Lerp(_rb.velocity.z, zSlow, 0.1f));
    }
}
