using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] public float aimSensitivity;
    private Vector2 _mouseDir;

    private float _clamp = 89f;

    private void Awake()
    {
        
    }
    private void LateUpdate()
    {
        
    }
    public void HandleMouseInput(Vector2 input)
    {
        _mouseDir = input * aimSensitivity;
    }
}
