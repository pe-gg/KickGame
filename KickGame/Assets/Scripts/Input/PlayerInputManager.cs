using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerMovement playerController;
    private PlayerAim playerAim;
    private PlayerJump playerJump;

    PlayerInput playerInput;
    Vector2 mouseVector;

    private void OnEnable()
    {
        playerController = GetComponent<PlayerMovement>();
        playerAim = GetComponent<PlayerAim>();
        playerJump = GetComponent<PlayerJump>();

        if (playerInput == null)
        {
            playerInput = new PlayerInput();
            playerInput.PlayerMovement.Movement.performed += i => playerController?.HandleMovementInput(i.ReadValue<Vector2>());

            playerInput.Mouselook.MouseX.performed += i => mouseVector.x = i.ReadValue<float>();
            playerInput.Mouselook.MouseY.performed += i => mouseVector.y = i.ReadValue<float>();

            playerInput.PlayerActions.Jump.performed += i => playerJump?.Jump();
            playerInput.PlayerActions.Jump.canceled += i => playerJump?.JumpCancel();
        }

        playerInput.Enable();
    }
    private void Update()
    {
        playerAim.HandleMouseInput(mouseVector);
    }
}
