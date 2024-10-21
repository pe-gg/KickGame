using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles kicking actions associated with the Mouse1 input.
/// </summary>
public class PlayerKick : MonoBehaviour
{
    #region Variables
    [SerializeField] private float _kickRange;
    [SerializeField] private float _kickForce;
    [SerializeField] private float _diveKickSpeed;
    [SerializeField] private float _kickJump;
    private int _diveKickTimeout = 600;
    private bool _diving = false;
    private Camera _cam;
    private PlayerState _state;
    private FauxGravity _grav;
    private Rigidbody _rb;
    private PlayerMovement _move;
    private PlayerDiveKickCollider _col;
    private ViewmodelAnimator _anim;

    private Quaternion _camClamped;
    #endregion
    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        _state = GetComponent<PlayerState>();
        _grav = GetComponent<FauxGravity>();
        _rb = GetComponent<Rigidbody>();
        _move = GetComponent<PlayerMovement>();
        _anim = GetComponent<ViewmodelAnimator>();
        _col = _cam.gameObject.GetComponentInChildren<PlayerDiveKickCollider>();
        _camClamped = Quaternion.Euler(Mathf.Clamp(_cam.transform.localEulerAngles.x, -89f, 0f), _cam.transform.localEulerAngles.y, _cam.transform.localEulerAngles.z);
    }

    /// <summary>
    /// Called by PlayerInputManager. If the player is in the air, divekick. Otherwise, ground kick.
    /// </summary>
    public void Kick()
    {
        if (_state.currentState == PlayerState.PState.JUMPING) 
        {
            DiveKick();
        }
        else
        {
            GroundKick();
        }
    }
    /// <summary>
    /// Does a raycast and checks if the component has a rigidbody. If so, apply force to the rigidbody in the direction that the player's camera is facing.
    /// Implementation is currently very basic and WIP.
    /// </summary>
    private void GroundKick()
    {
        _anim.GroundKick();
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, _cam.transform.forward, out hit, _kickRange))
        {
            EnemyAI enemyHit = hit.transform.GetComponentInParent<EnemyAI>();
            if (enemyHit != null)
                EnemyKnockback(enemyHit);
            Rigidbody rbHit = hit.transform.GetComponent<Rigidbody>();
            if (rbHit == null)
                return;
            rbHit.AddForce(_cam.transform.forward * _kickForce, ForceMode.Impulse);
            Debug.Log("Kick success!");
        }
    }

    private void EnemyKnockback(EnemyAI en)
    {
        Debug.Log("Enemy kicked!");
        en.ApplyStun(1f);
        en.ApplyKnockback(_cam.transform.forward * _kickForce / 4);
    }

    /// <summary>
    /// Initiates the divekick sequence. Thrusts the player upwards, sets the playerstate to divekicking, etc.
    /// </summary>
    private void DiveKick()
    {
        if (_diving)
            return;
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _anim.DiveKickStart();
        _rb.AddForce(Vector3.up * _kickJump, ForceMode.Impulse);
        Debug.Log("Divekick");
        _state.currentState = PlayerState.PState.DIVEKICKING;
        _diving = true;
        bool disable = true;
        _grav.TempDisableGravity(disable);
        _move.LockMovement(disable);
        _move.MultiplySpeedCap(2f);
        Invoke("DiveStart", 0.33f);
    }

    /// <summary>
    /// Enables divekick collider and starts the divekick coroutine.
    /// </summary>
    private void DiveStart()
    {
        _col.gameObject.SetActive(true);
        StartCoroutine("DiveKickLoop");
    }

    /// <summary>
    /// Thrusts the player downwards and forwards relative to their facing direction. 
    /// If the player does not collide with anything for 10 seconds, exits automatically.
    /// </summary>
    private IEnumerator DiveKickLoop()
    {
        while (_diving)
        {
            _cam.transform.localRotation = _camClamped;
            _rb.AddForce(_diveKickSpeed * _cam.transform.forward, ForceMode.Impulse);
            _rb.AddForce(_diveKickSpeed * 0.75f * Vector3.down, ForceMode.Impulse);
            _diveKickTimeout--;
            if (_diveKickTimeout <= 0 || _state.currentState != PlayerState.PState.DIVEKICKING)
            {
                _diving = false;
                break;
            }
            yield return new WaitForFixedUpdate();
            }
        _col.gameObject.SetActive(false);
        _anim.DiveKickEnd();
        Debug.Log("Divekick complete!");
        _diveKickTimeout = 600;
        bool enable = false;
        _grav.TempDisableGravity(enable);
        _move.LockMovement(enable);
        _move.ResetSpeedCap();
        yield return new WaitForFixedUpdate();
    }
 }
