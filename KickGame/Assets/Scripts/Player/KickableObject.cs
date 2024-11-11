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
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = (transform.position - kicker.transform.position).normalized;
                rb.AddForce(forceDirection * 10f, ForceMode.Impulse);
            }
        }
    }
}