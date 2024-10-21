using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

/// <summary>
/// Handles the player's jumping functions and actions tied to the spacebar. 
/// </summary>
public class PlayerJump : MonoBehaviour
{
    #region variables
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _checkSize;
    [SerializeField] private float _checkDist;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpCancelWindow;
    [SerializeField] private float _wallJumpRadius;
    private PlayerState _state;
    private Rigidbody _rb;
    private FauxGravity _grav;
    public bool grounded { get; private set; }
    private bool _jumpStarted;
    #endregion
    private void Awake()
    {
        _state = GetComponent<PlayerState>();
        _rb = GetComponent<Rigidbody>();
        _grav = GetComponent<FauxGravity>();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, _wallJumpRadius);
    }

    /// <summary>
    /// Checks if the player is grounded by using a SphereCast.
    /// </summary>
    private void FixedUpdate()
    {
        RaycastHit hit;
        if(Physics.SphereCast(this.transform.position, _checkSize, Vector3.down, out hit, _checkDist, _mask))
        {
            if (!grounded)
            {
                _state.currentState = PlayerState.PState.DEFAULT;
                _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            }
            grounded = true;
        }
        else
            grounded = false;
    }

    /// <summary>
    /// Called by PlayerInputManager. If the player can jump, then do so. Also responsible for calling WallJump.
    /// </summary>
    public void Jump()
    {
        if (!grounded || _jumpStarted)
        {
            WallJump();
            return;
        }
        _jumpStarted = true;
        _state.currentState = PlayerState.PState.JUMPING;
        StartCoroutine("TempJumpDisable");
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Called by PlayerInputManager. If the player releases the jump button early, their jump will end early.
    /// </summary>
    public void JumpCancel()
    {
        if (_jumpStarted)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, (-_jumpForce * 0.5f), _rb.velocity.z);
        }
    }

    /// <summary>
    /// Checks if the player is near a wall - if so, gets the normal direction of the wall and thrusts the player upwards and away from it.
    /// </summary>
    private void WallJump()
    {
        if (_state.currentState != PlayerState.PState.JUMPING)
            return;
        RaycastHit hit;
        Collider[] walls = Physics.OverlapSphere(this.transform.position, _wallJumpRadius, _mask);
        if (walls[0] == null)
        {
            Debug.Log("Walljump attempted, but failed");
            return;
        }
        Vector3 dir = walls[0].transform.position - this.transform.position;
        if (Physics.Raycast(this.transform.position, dir, out hit, 100f, _mask))
        {
            _grav.ResetLocalGravity();
            _rb.AddForce(_jumpForce * 2f * hit.normal, ForceMode.Impulse);
            _rb.AddForce(_jumpForce * 1.5f * Vector3.up, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Disables jumping temporarily to prevent 'double jumps' if the players jumps twice too close to the ground. Also doubles as the timing window for a jump cancel.
    /// </summary>
    private IEnumerator TempJumpDisable()
    {
        yield return new WaitForSeconds(_jumpCancelWindow);
        _jumpStarted = false;
        StopCoroutine("TempJumpDisable");
    }
}
