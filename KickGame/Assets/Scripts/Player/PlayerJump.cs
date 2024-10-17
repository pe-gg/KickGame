using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _checkSize;
    [SerializeField] private float _checkDist;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpCancelWindow;
    private PlayerState _state;
    private Rigidbody _rb;
    public bool grounded { get; private set; }
    private bool _jumpStarted;
    private void Awake()
    {
        _state = GetComponent<PlayerState>();
        _rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_jumpStarted)
            return;
        RaycastHit hit;
        if(Physics.SphereCast(this.transform.position, _checkSize, Vector3.down, out hit, _checkDist, _mask))
        {
            grounded = true;
        }
        else
            grounded = false;
    }

    public void Jump()
    {
        if (!grounded || _jumpStarted)
        {
            Debug.Log("Invalid Jump!");
            return;
        }
        _jumpStarted = true;
        StartCoroutine("TempJumpDisable");
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    public void JumpCancel()
    {
        if (_jumpStarted)
        {
            Debug.Log("cancelled");
            _rb.velocity = new Vector3(_rb.velocity.x, (-_jumpForce * 0.5f), _rb.velocity.z);
        }
    }

    private IEnumerator TempJumpDisable()
    {
        yield return new WaitForSeconds(_jumpCancelWindow);
        _jumpStarted = false;
        StopCoroutine("TempJumpDisable");
    }
}
