using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fabricated gravity script that's slightly more controllable than Unity's default gravity.
/// </summary>
public class FauxGravity : MonoBehaviour
{
    [SerializeField] private float _gravity;
    private float _localGrav;
    private bool _disable = false;
    private PlayerJump _groundcheck;
    private Rigidbody _rb;

    private void Awake()
    {
        _groundcheck = GetComponent<PlayerJump>();
        _rb = GetComponent<Rigidbody>();
    }
    /// <summary>
    /// Checks if the player is grounded. If not, apply gravity force. Clamped up to a set point.
    /// </summary>
    private void FixedUpdate()
    {
        if (_groundcheck.grounded || _disable)
        {
            _localGrav = _gravity;
            return;
        }
        _localGrav = _localGrav + _gravity * 1.5f;
        _localGrav = Mathf.Clamp(_localGrav, 0f, 25f);
        _rb.AddForce(Vector3.down * _localGrav, ForceMode.Acceleration);
    }
    /// <summary>
    /// Used by other scripts if they want to temporarily disable gravity.
    /// </summary>
    public void TempDisableGravity(bool set)
    {
        _disable = set;
    }
    /// <summary>
    /// Used by other scripts if they want to reset gravity scaling to 0.
    /// (i.e. if the player walljumps, gravity would otherwise keep scaling in FixedUpdate
    /// as they are still in the air. By calling this, the gravity can be reset and start scaling
    /// again as if they had just jumped from the ground.)
    /// </summary>
    public void ResetLocalGravity()
    {
        _localGrav = 0f;
    }
}
