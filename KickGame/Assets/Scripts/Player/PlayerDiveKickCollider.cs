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
    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _state = GetComponentInParent<PlayerState>();
        Invoke("Dummy", 0.1f);
    }
    private void Dummy()
    {
        this.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy") || other.CompareTag("Kickable"))
        {
            this.gameObject.SetActive(false);
            _state.currentState = PlayerState.PState.JUMPING;
            _rb.velocity = Vector3.zero;
            _rb.AddForce(Vector3.up * _kickHitForce, ForceMode.Impulse);
            EnemyAI en = other.GetComponentInParent<EnemyAI>();
            if (en == null)
                return;
            en.ApplyStun(2f);
        }
    }
}
