using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmProjectile : MonoBehaviour
{
    private CapsuleCollider _trigger;
    private PlayerArm _arm;
    private bool _canBePickedUp = false;
    private void Awake()
    {
        _arm = GameObject.Find("Player").GetComponent<PlayerArm>();
        _trigger = GetComponentInChildren<CapsuleCollider>();
        Invoke("ColliderEnable", 0.5f);
    }
    private void ColliderEnable()
    {
        _canBePickedUp = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _canBePickedUp)
        {
            Destroy(this.gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Arm hit enemy!");
            EnemyAI en = other.GetComponent<EnemyAI>();
            if (en == null)
                en = other.GetComponentInParent<EnemyAI>();
            en.ApplyStun(3f);
            Destroy(this.gameObject);
        }
        else
            return;
        
    }

    private void OnDestroy()
    {
        _arm.HasArm = true;
    }
}
