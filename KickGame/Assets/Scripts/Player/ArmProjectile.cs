using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmProjectile : MonoBehaviour
{
    private CapsuleCollider _trigger;
    private PlayerArm arm;
    private void Awake()
    {
        _trigger = GetComponentInChildren<CapsuleCollider>();
        _trigger.enabled = false;
        Invoke("ColliderEnable", 0.5f);
    }
    private void ColliderEnable()
    {
        _trigger.enabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        PlayerArm arm = other.GetComponent<PlayerArm>();
        arm.HasArm = true;
        Destroy(this.gameObject);
    }
}
