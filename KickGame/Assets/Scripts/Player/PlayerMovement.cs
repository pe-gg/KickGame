using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles functions relating to the player's WASD input.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 100f;
    [SerializeField] private float _friction = 0.2f;
    [SerializeField] private float _speedCap;
    [SerializeField] private float _turnSpeed;
    private float _defaultTurnSpeed;
    private float _defaultSpeedCap;
    private Rigidbody _rb;
    private PlayerState _state;
    private Vector2 _movement;
    private bool _airborne = false;
    private bool _lock;
    private bool _slow;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _state = GetComponent<PlayerState>();
        _defaultSpeedCap = _speedCap;
        _defaultTurnSpeed = _turnSpeed;
    }

    private void FixedUpdate()
    {
        if (_state.currentState == PlayerState.PState.JUMPING && !_airborne)
        {
            _airborne = true;
            _turnSpeed = _defaultTurnSpeed / _turnSpeed;
        }
        else if (_state.currentState == PlayerState.PState.DEFAULT)
        {
            _airborne = false;
            _turnSpeed = _defaultTurnSpeed;
        }
        Move();
        CapSpeed();
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
        if (_lock)
            return;
        Vector2 newMove = _movement;
        if (_movement.x * _rb.velocity.x < 0f)
        {
            newMove.x = _movement.x * _turnSpeed;
        }
        if (_movement.y * _rb.velocity.z < 0f)
        {
            newMove.y = _movement.y * _turnSpeed;
        }
        Vector3 _horiz = (transform.right * newMove.x + transform.forward * newMove.y) * _movementSpeed;
        _rb.AddForce(_horiz * (Mathf.Clamp(Mathf.Lerp(_movementSpeed, 0f, _friction), 0f, 20f)) * Time.fixedDeltaTime, ForceMode.Force);
    }
    /// <summary>
    /// Keeps the player's rigidbody velocity from going too high.
    /// </summary>
    private void CapSpeed()
    {
        _rb.velocity = new Vector3(Mathf.Clamp(_rb.velocity.x, -_speedCap, _speedCap), _rb.velocity.y, Mathf.Clamp(_rb.velocity.z, -_speedCap, _speedCap));
    }

    /// <summary>
    /// If the player isn't pressing an input, lerp the corresponding velocity to 0.
    /// </summary>
    private void VelocityDampen() 
    {
        float xSlow = transform.forward.x;
        float zSlow = transform.forward.z;
        if(_movement.magnitude <= 0.01f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), 0.1f);
        }
        
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
