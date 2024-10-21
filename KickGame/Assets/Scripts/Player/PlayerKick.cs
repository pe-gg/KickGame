using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    [SerializeField] private float _kickRange;
    [SerializeField] private float _kickForce;
    private Camera _cam;
    private PlayerState _state;
    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        _state = GetComponent<PlayerState>();
    }

    public void Kick()
    {
        if(_state.CompareState(1)) //1 = jumping
        {
            Debug.Log("IOU one divekick");
        }
        else
        {
            GroundKick();
        }
    }

    private void GroundKick()
    {
        RaycastHit hit;
        if(Physics.Raycast(this.transform.position, _cam.transform.forward, out hit, _kickRange))
        {
            Rigidbody rbHit = hit.transform.GetComponent<Rigidbody>();
            if (rbHit == null)
                return;
            rbHit.AddForce(_cam.transform.forward * _kickForce, ForceMode.Impulse);
            Debug.Log("Kick success!");
        }
    }
}
