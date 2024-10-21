using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
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

    private Quaternion _camClamped;
    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        _state = GetComponent<PlayerState>();
        _grav = GetComponent<FauxGravity>();
        _rb = GetComponent<Rigidbody>();
        _move = GetComponent<PlayerMovement>();
        _camClamped = Quaternion.Euler(Mathf.Clamp(_cam.transform.localEulerAngles.x, -89f, 0f), _cam.transform.localEulerAngles.y, _cam.transform.localEulerAngles.z);
    }

    public void Kick()
    {
        if (_state.CompareState(1)) //1 = jumping
        {
            DiveKick();
        }
        else
        {
            GroundKick();
        }
    }

    private void GroundKick()
    {
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, _cam.transform.forward, out hit, _kickRange))
        {
            Rigidbody rbHit = hit.transform.GetComponent<Rigidbody>();
            if (rbHit == null)
                return;
            rbHit.AddForce(_cam.transform.forward * _kickForce, ForceMode.Impulse);
            Debug.Log("Kick success!");
        }
    }

    private void DiveKick()
    {
        if (_diving)
            return;
        _rb.velocity = new Vector3(_rb.velocity.x / 0.5f, 0f, _rb.velocity.z / 0.5f);
        _rb.AddForce(Vector3.up * _kickJump, ForceMode.Impulse);
        Debug.Log("Divekick");
        _state.currentState = PlayerState.PState.DIVEKICKING;
        _diving = true;
        bool disable = true;
        _grav.TempDisableGravity(disable);
        _move.LockMovement(disable);
        Invoke("DiveStart", 0.33f);
    }

    private void DiveStart()
    {
        StartCoroutine("DiveKickLoop");
    }

    private IEnumerator DiveKickLoop()
    {
        while (_diving)
        {
            _cam.transform.localRotation = _camClamped;
            _rb.AddForce(_diveKickSpeed * _cam.transform.forward, ForceMode.Impulse);
            _rb.AddForce(_diveKickSpeed * 0.75f * Vector3.down, ForceMode.Impulse);
            _diveKickTimeout--;
            if (_diveKickTimeout <= 0 || _state.currentState == PlayerState.PState.DEFAULT)
            {
                _diving = false;
                break;
            }
            yield return new WaitForFixedUpdate();
            }
        Debug.Log("Divekick complete!");
        _diveKickTimeout = 600;
        _state.currentState = PlayerState.PState.DEFAULT; //just in case
        bool enable = false;
        _grav.TempDisableGravity(enable);
        _move.LockMovement(enable);
        _move.TempSlow();
        yield return new WaitForFixedUpdate();
    }

    private void DiveKickBounce()
    {

    }
 }
