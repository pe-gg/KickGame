using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controls the enemy behavior including patrolling, attacking, and reactions to damage.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // Enemy stats
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    private int currentHealth;
    public int ammoCapacity = 10;
    private int currentAmmo;
    public float reloadTime = 2f;
    public bool canShoot = true; // Ability to shoot
    public bool canChase = true; // Ability to chase
    public bool canFriendlyFire = true; // Ability to shoot

    // Movement and patrol
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex;
    private NavMeshAgent agent;

    // Detection
    [Header("Detection Settings")]
    public float detectionRadius = 15f;
    public float fieldOfViewAngle = 110f;
    public LayerMask playerLayer;
    private GameObject player;
    private bool playerInSight;

    // Debugging
    [Header("Debug Settings")]
    public bool showFieldOfView = true; // Toggle to debug field of view

    // Attack
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackInterval = 1f;
    private float attackTimer;

    // States
    private enum State { Patrolling, Attacking, Stunned, Pain }
    private State currentState;

    // Components
    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        currentAmmo = ammoCapacity;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        currentState = State.Patrolling;
        agent.autoBraking = false;
        GotoNextPatrolPoint();

        Debug.Log($"{gameObject.name} initialized. Starting in state: {currentState}");
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                DetectPlayer();
                break;
            case State.Attacking:
                if (canChase)
                {
                    Attack();
                }
                else
                {
                    currentState = State.Patrolling;
                    agent.isStopped = false;
                    Debug.Log($"{gameObject.name} cannot chase. Returning to Patrolling state.");
                }
                break;
            case State.Stunned:
                // Handle stunned behavior
                break;
            case State.Pain:
                // Handle pain reaction
                break;
        }
    }

    /// <summary>
    /// Moves the enemy to the next patrol point.
    /// </summary>
    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    /// <summary>
    /// Handles the patrolling behavior.
    /// </summary>
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPatrolPoint();
    }

    /// <summary>
    /// Detects the player within the detection radius and field of view.
    /// </summary>
    void DetectPlayer()
    {
        if (!canChase)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == player)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle < fieldOfViewAngle * 0.5f)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(transform.position + Vector3.up, direction, out raycastHit, detectionRadius))
                    {
                        if (raycastHit.collider.gameObject == player)
                        {
                            playerInSight = true;
                            currentState = State.Attacking;
                            agent.isStopped = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the attacking behavior.
    /// </summary>
    void Attack()
    {
        if (!canShoot)
        {
            Debug.Log($"{gameObject.name} cannot shoot. Attack aborted.");
            return;
        }

        // Rotate towards the player
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0; // Keep only horizontal rotation
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        attackTimer += Time.deltaTime;

        if (currentAmmo > 0)
        {
            if (attackTimer >= attackInterval)
            {
                Shoot();
                attackTimer = 0f;
            }
        }
        else
        {
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Shoots a projectile towards the player.
    /// </summary>
    void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        currentAmmo--;
    }

    /// <summary>
    /// Reloads the enemy's ammo after a delay.
    /// </summary>
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = ammoCapacity;
    }

    /// <summary>
    /// Applies knockback to the enemy.
    /// </summary>
    /// <param name="force">The force to apply.</param>
    public void ApplyKnockback(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        currentState = State.Stunned;
        Debug.Log($"{gameObject.name} received knockback. Switching to Stunned state.");
        // Handle stun duration and transition back to patrolling or attacking
    }

    /// <summary>
    /// Inflicts damage to the enemy.
    /// </summary>
    /// <param name="damage">Amount of damage.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = State.Pain;
            Debug.Log($"{gameObject.name} in Pain state.");
            // Handle pain animation and transition back to previous state
        }
    }

    /// <summary>
    /// Applies a stun effect to the enemy for a specified duration.
    /// </summary>
    /// <param name="duration">Duration of the stun in seconds.</param>
    public void ApplyStun(float duration)
    {
        if (currentState != State.Stunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        currentState = State.Stunned;
        agent.isStopped = true;
        Debug.Log($"{gameObject.name} is stunned for {duration} seconds.");
        // Play stun animation if available
        yield return new WaitForSeconds(duration);
        agent.isStopped = false;
        currentState = State.Patrolling;
        Debug.Log($"{gameObject.name} recovered from stun. Returning to Patrolling state.");
    }

    /// <summary>
    /// Handles enemy death.
    /// </summary>
    void Die()
    {
        // Play death animation and destroy enemy object
        Debug.Log($"{gameObject.name} died.");
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            Debug.Log($"{gameObject.name} entered hazard and died.");
            Die();
        }
    }

    /// <summary>
    /// Checks if the enemy is hit by another enemy's projectile.
    /// </summary>
    /// <param name="damage">Damage received.</param>
    public void FriendlyFire(int damage)
    {
        if(!canFriendlyFire) return;
        
        Debug.Log($"{gameObject.name} hit by friendly fire. Damage: {damage}");
        TakeDamage(damage);
        // Optionally change target to the enemy that shot
    }

    /// <summary>
    /// Draws debug visuals in the Scene view.
    /// </summary>
    void OnDrawGizmos()
    {
        if (showFieldOfView)
        {
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw field of view lines
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRadius);
        }
    }
}
