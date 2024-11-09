using UnityEngine;
using UnityEngine.VFX;

public class ExplodingBox : BasicBox
{
    public float kickForce = 10f;         
    public float explosionForce = 500f;  
    public float explosionRadius = 5f;

    public VisualEffect explosionEffect;  // Reference to the VFX component jw

    private bool isKicked = false;  


    public void Start()
    {
        if (explosionEffect != null)
        {
            explosionEffect.Stop(); 
        }
    }

    public void Kick(Vector3 kickDirection)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force forward and up
            Vector3 finalKickDirection = kickDirection + Vector3.up;
            rb.AddForce(finalKickDirection * kickForce, ForceMode.Impulse);
            isKicked = true;
            Debug.Log($"{objectName} was kicked!");
        }
    }

    public void Explode()
    {
        Debug.Log($"{objectName} exploded!");

        // Trigger the explosion VFX
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        // Explosion logic here
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                EnemyAI en = nearbyObject.GetComponent<EnemyAI>(); //this doesn't work! replace later with interface check
                if (en != null)
                {
                    en.ApplyStun(3f);
                }
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Destroy the box after exploding
        Destroy(gameObject,0.1f);
    }

    public override void Interact(GameObject instigator)
    {
        base.Interact(instigator);
        if (!isKicked)
        {
            Debug.Log("KICKING FORWARD");
            Vector3 kickDirection = instigator.transform.forward;
            Kick(kickDirection);
        }
        else
        {
            // If already kicked, ignore further interactions
            Debug.Log($"{objectName} is ready to explode on the next collision.");
        }
    }

    public override void WorldInteraction(GameObject instigator)
    {
        base.WorldInteraction(instigator);
        if (isKicked)
        {
            Explode();
        }
    }
}