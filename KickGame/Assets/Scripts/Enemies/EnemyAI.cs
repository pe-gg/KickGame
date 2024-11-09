using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controls the enemy behavior including patrolling, detecting the player,
/// moving into range, attacking, and reactions to damage.
/// </summary>
[AddComponentMenu("AI/Enemy AI")]
public class EnemyAI : MonoBehaviour
{
   


    #region Enemy Stats

    [Header("Enemy Stats")]
    [Tooltip("The maximum health of the enemy.")]
    public int MaxHealth = 100;

    private int _currentHealth;

    [Tooltip("The maximum ammo capacity of the enemy.")]
    public int AmmoCapacity = 10;

    private int _currentAmmo;

    [Tooltip("The time it takes for the enemy to reload.")]
    public float ReloadTime = 2f;

    [Tooltip("Determines if the enemy can shoot.")]
    public bool CanShoot = true;

    [Tooltip("Determines if the enemy can chase the player.")]
    public bool CanChase = true;

    [Tooltip("Determines if the enemy can be damaged by friendly fire.")]
    public bool CanFriendlyFire = true;

    #endregion

    #region Movement and Patrol

    [Header("Patrol Settings")]
    [Tooltip("Points between which the enemy will patrol.")]
    public Transform[] PatrolPoints;

    private int _currentPatrolIndex;

    private NavMeshAgent _agent;

    #endregion

    #region Detection

    [Header("Detection Settings")]
    [Tooltip("The radius within which the enemy can detect the player.")]
    public float DetectionRadius = 15f;

    [Tooltip("The field of view angle for player detection.")]
    public float FieldOfViewAngle = 110f;

    [Tooltip("Layer mask to identify the player.")]
    public LayerMask PlayerLayer;

    private GameObject _player;

    private bool _playerInSight;

    #endregion

    #region Debugging

    [Header("Debug Settings")]
    [Tooltip("Enable to visualize the enemy's field of view in the Scene view.")]
    public bool ShowFieldOfView = true;

    #endregion

    #region Attack

    [Header("Attack Settings")]
    [Tooltip("The projectile prefab the enemy uses to attack.")]
    public GameObject ProjectilePrefab;

    [Tooltip("The point from which the enemy fires projectiles.")]
    public Transform FirePoint;

    [Tooltip("Time interval between enemy attacks.")]
    public float AttackInterval = 1f;

    [Tooltip("The range within which the enemy can attack the player.")]
    public float AttackRange = 10f;

    public float StunGroundCheckDistance = 55f;
    private float _attackTimer;

    #endregion

    #region States

    private enum State
    {
        Patrolling,
        Chasing,
        Attacking,
        Reloading,
        Stunned,
        Pain
    }

    private State _currentState;

    #endregion

    #region Components

    private Animator _animator;

    private Rigidbody _rigidbody;

    #endregion

    /// <summary>
    /// Initializes the enemy.
    /// </summary>
    private void Start()
    {
       
        _currentHealth = MaxHealth;
        _currentAmmo = AmmoCapacity;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _currentState = State.Patrolling;
        _agent.autoBraking = false;
        MoveToNextPatrolPoint();

        Debug.Log($"{gameObject.name} initialized. Starting in state: {_currentState}");
    }


    /// <summary>
    /// Updates the enemy's behavior each frame.
    /// </summary>
    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrolling:
                Patrol();
                DetectPlayer();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
            case State.Attacking:
                Attack();
                break;
            case State.Reloading:
                // Do nothing; wait for reloading to finish
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
    /// Checks if the enemy is on the ground.
    /// </summary>
    /// <returns>True if the enemy is on the ground, false otherwise.</returns>
    private bool CheckForGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, StunGroundCheckDistance))
        {
            Debug.Log($"{gameObject.name} found ground and can recover.");
            return true;
        }
        Debug.Log($"{gameObject.name} is not on the ground yet.");
        return false;
    }
    

    
    /// <summary>
    /// Moves the enemy to the next patrol point.
    /// </summary>
    private void MoveToNextPatrolPoint()
    {
        if (PatrolPoints.Length == 0)
        {
            return;
        }

        _agent.destination = PatrolPoints[_currentPatrolIndex].position;
        Debug.Log($"{gameObject.name} moving to patrol point {_currentPatrolIndex}");
        _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
    }

    /// <summary>
    /// Handles the patrolling behavior.
    /// </summary>
    private void Patrol()
    {
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Patrol"))
        {
            _animator.SetTrigger("Patrol");
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            MoveToNextPatrolPoint();
        }
    }

    /// <summary>
    /// Detects the player within the detection radius and field of view.
    /// </summary>
    private void DetectPlayer()
    {
        if (!CanChase)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, DetectionRadius, PlayerLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == _player)
            {
                Vector3 direction = (_player.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(direction, transform.forward);

                if (angle < FieldOfViewAngle * 0.5f)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(transform.position + Vector3.up, direction, out raycastHit, DetectionRadius))
                    {
                        if (raycastHit.collider.gameObject == _player)
                        {
                            _playerInSight = true;
                            _currentState = State.Chasing;
                            _agent.isStopped = false;
                            Debug.Log($"{gameObject.name} detected player. Switching to Chasing state.");
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Chases the player to get within attack range.
    /// </summary>
    private void ChasePlayer()
    {
        if (!CanChase)
        {
            _currentState = State.Patrolling;
            _agent.isStopped = false;
            Debug.Log($"{gameObject.name} cannot chase. Returning to Patrolling state.");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer <= AttackRange)
        {
            // Within attack range, switch to attacking
            _currentState = State.Attacking;
            _agent.isStopped = true;
            Debug.Log($"{gameObject.name} is within attack range. Switching to Attacking state.");
        }
        else
        {
            // Move towards player
            _agent.isStopped = false;
            _agent.SetDestination(_player.transform.position);
        }
    }

    /// <summary>
    /// Handles the attacking behavior.
    /// </summary>
    private void Attack()
    {
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
        {
            _animator.SetTrigger("Shoot");
        }
        if (!CanShoot)
        {
            Debug.Log($"{gameObject.name} cannot shoot. Attack aborted.");
            return;
        }

        // Check if player is still within attack range
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        if (distanceToPlayer > AttackRange)
        {
            // Player moved out of range, start chasing
            _currentState = State.Chasing;
            _agent.isStopped = false;
            Debug.Log($"{gameObject.name} lost attack range. Switching to Chasing state.");
            return;
        }

        // Rotate towards the player
        Vector3 direction = (_player.transform.position - transform.position).normalized;
        direction.y = 0; // Keep only horizontal rotation
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        _attackTimer += Time.deltaTime;

        if (_currentAmmo > 0)
        {
            if (_attackTimer >= AttackInterval)
            {
                Shoot();
                _attackTimer = 0f;
            }
        }
        else
        {
            // Start reloading
            _currentState = State.Reloading;
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Shoots a projectile towards the player.
    /// </summary>
    private void Shoot()
    {
        GameObject projectileInstance = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Initialize the projectile
            projectileScript.Initialize(
                shooterObject: this.gameObject,
                hitSelf: false,
                layer: LayerMask.NameToLayer("Projectile")
            );

            // Assign target tags
            projectileScript.targetTags = new string[] { "Player" };
        }
        _currentAmmo--;
        Debug.Log($"{gameObject.name} fired a shot. Ammo remaining: {_currentAmmo}");
    }

    /// <summary>
    /// Reloads the enemy's ammo after a delay.
    /// </summary>
    private IEnumerator Reload()
    {
        Debug.Log($"{gameObject.name} is reloading.");
        // While reloading, enemy should not move
        _agent.isStopped = true;
        yield return new WaitForSeconds(ReloadTime);
        _currentAmmo = AmmoCapacity;
        Debug.Log($"{gameObject.name} reloaded. Ammo refilled to {_currentAmmo}");
        // After reloading, decide whether to attack or chase
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
        if (distanceToPlayer <= AttackRange)
        {
            _currentState = State.Attacking;
        }
        else
        {
            _currentState = State.Chasing;
            _agent.isStopped = false;
        }
    }

    /// <summary>
    /// Applies knockback to the enemy.
    /// </summary>
    /// <param name="force">The force to apply.</param>
    public void ApplyKnockback(Vector3 force)
    {
        _rigidbody.AddForce(force, ForceMode.Impulse);
        _currentState = State.Stunned;
        Debug.Log($"{gameObject.name} received knockback. Switching to Stunned state.");
        // Optionally, handle stun duration and transition back to previous state
    }

    /// <summary>
    /// Inflicts damage to the enemy.
    /// </summary>
    /// <param name="damage">Amount of damage.</param>
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {_currentHealth}");
        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            _currentState = State.Pain;
            Debug.Log($"{gameObject.name} in Pain state.");
            // Optionally, handle pain animation and transition back to previous state
        }
    }

    /// <summary>
    /// Applies a stun effect to the enemy for a specified duration.
    /// </summary>
    /// <param name="duration">Duration of the stun in seconds.</param>
    public void ApplyStun(float duration)
    {
        if (_currentState != State.Stunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    /// <summary>
    /// Coroutine to handle the stun effect over time.
    /// </summary>
    /// <param name="duration">Duration of the stun.</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator StunCoroutine(float duration)
    {
        _currentState = State.Stunned;
        _agent.isStopped = true;
        _agent.enabled = false;

       

        Debug.Log($"{gameObject.name} is stunned for {duration} seconds.");
        // Optionally, play stun animation if available
        yield return new WaitForSeconds(duration);
        if (CheckForGround())
        {
            RecoverFromStun();
        }
    }
    
    /// <summary>
    /// Recovers the enemy from stun.
    /// </summary>
    private void RecoverFromStun()
    {
        _agent.enabled = true;
        _agent.isStopped = false;
        // Reset velocity 
        _agent.velocity = Vector3.zero; 
        _rigidbody.velocity = Vector3.zero; 
        _currentState = State.Patrolling;
        Debug.Log($"{gameObject.name} recovered from stun. Returning to Patrolling state.");
    }
    
    /// <summary>
    /// Handles enemy death.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        // Optionally, play death animation before destroying
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when the enemy enters a trigger collider.
    /// </summary>
    /// <param name="other">The collider entered.</param>
    private void OnTriggerEnter(Collider other)
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
        if (!CanFriendlyFire)
        {
            return;
        }

        Debug.Log($"{gameObject.name} hit by friendly fire. Damage: {damage}");
        TakeDamage(damage);
        // Optionally, change target to the enemy that shot
    }

    /// <summary>
    /// Draws debug visuals in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (ShowFieldOfView)
        {
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRadius);

            // Draw field of view lines
            Vector3 leftBoundary = Quaternion.Euler(0, -FieldOfViewAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, FieldOfViewAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary * DetectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary * DetectionRadius);

            // Optionally, draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
