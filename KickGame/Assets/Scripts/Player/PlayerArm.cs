using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArm : MonoBehaviour
{
    [SerializeField] private ArmProjectile _arm;
    [SerializeField] private float _armThrowForce;
    private Camera _cam;
    private ViewmodelAnimator _anim;
    public bool HasArm = true;
    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        _anim = GetComponent<ViewmodelAnimator>();
    }
    public void ArmThrow()
    {
        if (!HasArm)
            return;
        HasArm = false;
        ArmProjectile newArm = Instantiate(_arm, _cam.transform.position, Quaternion.Euler(90f, 0f, 0f));
        newArm.GetComponent<Rigidbody>().AddForce(_cam.transform.forward * _armThrowForce, ForceMode.Impulse);
    }
}
