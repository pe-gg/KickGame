using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the player's mouse input and converts it into camera aiming.
/// </summary>
public class PlayerAim : MonoBehaviour
{
    [SerializeField] public float aimSensitivity;
    [SerializeField] private Transform _camera;
    private Vector2 _mouseDir;

    private float _clamp = 89f;
    private float _xRot;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }
    private void LateUpdate() //got lazy and copied this from a youtube tutorial... not ideal but needed to just get SOMETHING done
    {
        transform.Rotate(Vector3.up, _mouseDir.x * Time.deltaTime);
        _xRot -= _mouseDir.y;
        _xRot = Mathf.Clamp(_xRot, -_clamp, _clamp);
        Vector3 targetRot = transform.eulerAngles;
        targetRot.x = _xRot;
        _camera.eulerAngles = targetRot;
    }
    public void HandleMouseInput(Vector2 input)
    {
        _mouseDir = new Vector2(input.x * aimSensitivity, input.y * (aimSensitivity / 16));
    }
}
