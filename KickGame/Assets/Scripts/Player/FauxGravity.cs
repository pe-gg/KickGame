using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravity : MonoBehaviour
{
    [SerializeField] private float _gravity;
    private float _localGrav;
    private PlayerJump _groundcheck;
    private Rigidbody _rb;

    private void Awake()
    {
        _groundcheck = GetComponent<PlayerJump>();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_groundcheck.grounded)
        {
            _localGrav = _gravity;
            return;
        }
        _localGrav = _localGrav + _gravity * 1.5f;
        _localGrav = Mathf.Clamp(_localGrav, 0f, Mathf.Clamp(_gravity * 100f, 0f, 10000f));
        _rb.AddForce(Vector3.down * _localGrav, ForceMode.Acceleration);
    }

    public void ResetLocalGravity()
    {
        _localGrav = 0f;
    }
}
