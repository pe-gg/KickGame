using UnityEngine;

/// <summary>
/// Manages player movement, interactions, and camera rotation using CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 100f;

    [Header("Testing Settings")]
    public KeyCode shootKey = KeyCode.F;
    public KeyCode stunEnemyKey = KeyCode.G;

    [Header("Projectile Settings")]
    public GameObject playerProjectilePrefab;
    public Transform firePoint;

    // Components
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Mouse look variables
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleTestingInputs();
    }

    /// <summary>
    /// Handles player movement inputs using WASD keys with CharacterController.
    /// </summary>
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // Explicitly using WASD keys for movement
        if (Input.GetKey(KeyCode.W))
            moveVertical += 1f;
        if (Input.GetKey(KeyCode.S))
            moveVertical -= 1f;
        if (Input.GetKey(KeyCode.D))
            moveHorizontal += 1f;
        if (Input.GetKey(KeyCode.A))
            moveHorizontal -= 1f;

        Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handles camera rotation based on mouse movement for first-person view.
    /// </summary>
    void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player around the Y-axis
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera around the X-axis
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limit vertical rotation

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    /// <summary>
    /// Handles inputs for testing enemy AI functionalities.
    /// </summary>
    void HandleTestingInputs()
    {
        if (Input.GetKeyDown(shootKey))
        {
            ShootAtEnemy();
        }

        if (Input.GetKeyDown(stunEnemyKey))
        {
            StunEnemy();
        }
    }

    /// <summary>
    /// Shoots a projectile towards where the camera is facing.
    /// </summary>
    void ShootAtEnemy()
    {
        Instantiate(playerProjectilePrefab, firePoint.position, playerCamera.transform.rotation);
    }

    /// <summary>
    /// Stuns all enemies in the scene.
    /// </summary>
    void StunEnemy()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.ApplyStun(3f); // Stun duration of 3 seconds
        }
        Debug.Log("All enemies stunned.");
    }
}
