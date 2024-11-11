using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to check if the player 'hits' a divekick. 
/// </summary>
public class PlayerDiveKickCollider : MonoBehaviour
{
    [SerializeField] private float _kickHitForce;
    private Rigidbody _rb;
    private PlayerState _state;
    private PlayerJump _jump;
    public Collider col;
    AudioManager audioManager;
    
    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _state = GetComponentInParent<PlayerState>();
        _jump = GetComponentInParent<PlayerJump>();
        col = GetComponent<Collider>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy") || other.CompareTag("Kickable"))
        {
            col.enabled = false;
            _jump.JumpDisable();
            StartCoroutine(ResetRBAndJump());
            _state.currentState = PlayerState.PState.JUMPING;
            audioManager.PlaySFX(audioManager.sfxclips[0]);

            EnemyAI en = other.GetComponentInParent<EnemyAI>();
            if (en == null)
                return;
            en.ApplyStun(2f);
        }
    }

    IEnumerator ResetRBAndJump()
    {
        var oldConstraints = _rb.constraints;
        Debug.Log("Coroutine Started!");
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        yield return new WaitForFixedUpdate();
        _rb.constraints = oldConstraints;
        yield return new WaitForFixedUpdate();
        _rb.AddForce(Vector3.up * _kickHitForce, ForceMode.Impulse);
        Debug.Log("Velocity is " + _rb.velocity);
        Debug.Log("Angular Vel is " + _rb.angularVelocity);
    }
}
