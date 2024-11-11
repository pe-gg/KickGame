using UnityEngine;

namespace Player
{
    public class KickableObject : MonoBehaviour, IKickable
    {
        public void OnKick(GameObject kicker)
        {
            // Handle what happens when this object is kicked
            Debug.Log($"{gameObject.name} was kicked by {kicker.name}!");

            // Example: Apply force
            EnemyAI en = GetComponent<EnemyAI>();
            if (en != null)
            {
                en.ApplyStun(2f);
                Vector3 forceDirection = (transform.position - kicker.transform.position).normalized;
                en.ApplyKnockback(forceDirection * 10f);
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = (transform.position - kicker.transform.position).normalized;
                rb.AddForce(forceDirection * 10f, ForceMode.Impulse);
            }
        }
    }
}