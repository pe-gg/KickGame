using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles functions relating to the player's WASD input.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 100f;
    [SerializeField] private float _friction = 0.2f;
    [SerializeField] private float _speedCap;
    private float _defaultSpeedCap;
    private Rigidbody _rb;
    private Vector2 _movement;
    private bool _lock;
    private bool _slow;
    private int _slowTime = 30;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _defaultSpeedCap = _speedCap;
    }

    private void FixedUpdate()
    {
        Move();
        VelocityDampen();
    }

    /// <summary>
    /// Parse user input from PlayerInputManager.
    /// </summary>
    public void HandleMovementInput(Vector2 input)
    {
        _movement = input;
    }

    /// <summary>
    /// Adds force to the rigidbody. relative to the player's input.
    /// Uses forces instead of setting velocity directly so that the player can have multiple forces applied to it at once.
    /// Could use more work, as the player feels very 'slippery' currently.
    /// </summary>
    private void Move()
    {
        _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, -_speedCap, _speedCap), _rb.velocity.y, Mathf.Clamp(_rb.velocity.z, -_speedCap, _speedCap));
        if (_lock)
            return;
        Vector3 _horiz = (transform.right * _movement.x + transform.forward * _movement.y) * _movementSpeed;
        _rb.AddForce(_horiz * (Mathf.Clamp(Mathf.Lerp(_movementSpeed, 0f, _friction), 0f, 20f)) * Time.fixedDeltaTime, ForceMode.Force);
    }

    /// <summary>
    /// If the player isn't pressing an input, lerp the corresponding velocity to 0.
    /// </summary>
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
    /// <summary>
    /// Used by external scripts to lock movement if it is undesired (i.e. If a player is divekicking, then movement input should not be enabled).
    /// </summary>
    public void LockMovement(bool set)
    {
        _lock = set;
    }
    /// <summary>
    /// 
    /// </summary>
    public void MultiplySpeedCap(float cap)
    {
        _speedCap = _speedCap * cap;
    }
    public void ResetSpeedCap()
    {
        _speedCap = _defaultSpeedCap;
    }

}
