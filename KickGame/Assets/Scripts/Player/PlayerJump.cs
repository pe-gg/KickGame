using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class PlayerJump : MonoBehaviour
{
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
    private void Awake()
    {
        _state = GetComponent<PlayerState>();
        _rb = GetComponent<Rigidbody>();
        _grav = GetComponent<FauxGravity>();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.position, _wallJumpRadius);
    }
    private void FixedUpdate()
    {
        if (_jumpStarted)
            return;
        RaycastHit hit;
        if(Physics.SphereCast(this.transform.position, _checkSize, Vector3.down, out hit, _checkDist, _mask))
        {
            if (!grounded)
            {
                _state.SwitchState(0);
            }
            grounded = true;
        }
        else
            grounded = false;
    }

    public void Jump()
    {
        if (!grounded || _jumpStarted)
        {
            WallJump();
            return;
        }
        _jumpStarted = true;
        _state.SwitchState(0);
        StartCoroutine("TempJumpDisable");
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    public void JumpCancel()
    {
        if (_jumpStarted)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, (-_jumpForce * 0.5f), _rb.velocity.z);
        }
    }

    private void WallJump()
    {
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
            _rb.AddForce(_jumpForce * hit.normal, ForceMode.Impulse);
            _rb.AddForce(_jumpForce * 1.5f * Vector3.up, ForceMode.Impulse);
        }
    }

    private IEnumerator TempJumpDisable()
    {
        yield return new WaitForSeconds(_jumpCancelWindow);
        _jumpStarted = false;
        StopCoroutine("TempJumpDisable");
    }
}
