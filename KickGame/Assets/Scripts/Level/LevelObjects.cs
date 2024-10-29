using UnityEngine;

public class LevelObjects : MonoBehaviour
{
    // Common properties for level objects
    public string objectName;
    public bool isActive = true;

    virtual public void Interact(GameObject instigator)
    {
    }

    virtual public void WorldInteraction(GameObject instigator)
    {
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{objectName} collided with the player.");
            Interact(collision.gameObject);  
        }
        else
        {
            WorldInteraction(collision.gameObject);
        }
    }
}
