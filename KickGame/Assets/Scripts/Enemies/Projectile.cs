using UnityEngine;

/// <summary>
/// Controls the projectile behavior with improved collision handling and optional self-hit.
/// </summary>
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 25;
    public Rigidbody rb;
    public float lifeTime = 5f; // Time before the projectile is destroyed automatically
    public string[] targetTags; // Tags of objects that the projectile can damage
    public GameObject shooter; // Reference to the shooter who fired the projectile
    public bool canHitSelf = false; // Determines if the projectile can hit the shooter

    void Start()
    {
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifeTime); // Destroy the projectile after a set time
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider hitInfo)
    {
        GameObject hitObject = hitInfo.gameObject;

        // Ignore collision with the shooter if canHitSelf is false
        if (!canHitSelf && hitObject == shooter)
            return;

        // Check if the hit object has one of the target tags
        foreach (string tag in targetTags)
        {
            if (hitObject.CompareTag(tag))
            {
                if (tag == "Player")
                {
                    Debug.LogWarning("TODO:  Player hit ");
                    // Handle player damage
                    PlayerHealth player = hitObject.GetComponent<PlayerHealth>(); // I changed this from PlayerController to PlayerHealth
                    if (player != null)
                    {
                        player.TakeDamage(damage);
                    }
                }
                else if (tag == "Enemy")
                {
                    Debug.Log("Projectile hit an Enemy.");
                    EnemyAI enemy = hitObject.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        if (hitObject == shooter)
                        {
                            // Handle self-hit logic
                            enemy.TakeDamage(damage);
                            Debug.Log("Enemy hit itself.");
                        }
                        else
                        {
                            enemy.FriendlyFire(damage);
                        }
                    }
                }
                else if (tag == "Kickable")
                {
                    ExplodingBox box = hitObject.GetComponent<ExplodingBox>();
                    if(box != null)
                    {
                        Debug.Log("BOX HIT");
                        box.Explode();
                    }
                }
                else
                {
                    Debug.Log($"Projectile hit an object with tag {tag}.");
                }

                Destroy(gameObject); // Destroy the projectile upon hitting a target
                return;
            }
        }

        // Optionally, you can destroy the projectile if it hits any other object
        // Destroy(gameObject);
    }

    /// <summary>
    /// Sets the shooter and initializes the projectile settings.
    /// </summary>
    /// <param name="shooterObject">The GameObject that fired the projectile.</param>
    /// <param name="hitSelf">Determines if the projectile can hit the shooter.</param>
    /// <param name="layer">Layer to set for the projectile.</param>
    public void Initialize(GameObject shooterObject, bool hitSelf, int layer)
    {
        shooter = shooterObject;
        canHitSelf = hitSelf;
        gameObject.layer = layer;

        // Ignore collisions with the projectile's own layer
        Physics.IgnoreLayerCollision(layer, layer);

        // Optionally, ignore collision between the projectile and the shooter
        if (!canHitSelf && shooter != null)
        {
            Collider shooterCollider = shooter.GetComponent<Collider>();
            Collider projectileCollider = GetComponent<Collider>();
            if (shooterCollider != null && projectileCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, shooterCollider);
            }
        }
    }
}
