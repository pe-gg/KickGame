using UnityEngine;

/// <summary>
/// Manages player movement, interactions, and testing functionalities using CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Testing Settings")]
    public KeyCode toggleEnemyShootingKey = KeyCode.T;
    public KeyCode toggleEnemyChasingKey = KeyCode.C;
    public KeyCode shootKey = KeyCode.F;
    public KeyCode stunEnemyKey = KeyCode.G;

    [Header("Projectile Settings")]
    public GameObject playerProjectilePrefab;
    public Transform firePoint;

    // Components
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool enemyCanShoot = true;
    private bool enemyCanChase = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
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
    /// Handles inputs for testing enemy AI functionalities.
    /// </summary>
    void HandleTestingInputs()
    {
        if (Input.GetKeyDown(toggleEnemyShootingKey))
        {
            ToggleEnemyShooting();
        }

        if (Input.GetKeyDown(toggleEnemyChasingKey))
        {
            ToggleEnemyChasing();
        }

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
    /// Toggles the ability of enemies to shoot.
    /// </summary>
    void ToggleEnemyShooting()
    {
        enemyCanShoot = !enemyCanShoot;
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.canShoot = enemyCanShoot;
        }
        Debug.Log("Enemy shooting toggled: " + enemyCanShoot);
    }

    /// <summary>
    /// Toggles the ability of enemies to chase the player.
    /// </summary>
    void ToggleEnemyChasing()
    {
        enemyCanChase = !enemyCanChase;
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.canChase = enemyCanChase;
        }
        Debug.Log("Enemy chasing toggled: " + enemyCanChase);
    }

    /// <summary>
    /// Shoots a projectile towards the enemy.
    /// </summary>
    void ShootAtEnemy()
    {
        Instantiate(playerProjectilePrefab, firePoint.position, firePoint.rotation);
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
