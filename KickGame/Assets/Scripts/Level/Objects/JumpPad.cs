using UnityEngine;

public class JumpPad : LevelObjects
{
    public float jumpForce = 10f;

    public void ActivateJumpPad(GameObject player)
    {
        Debug.Log($"{objectName} activated!");
        // Add jumping logic, e.g., applying force to player
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public override void Interact(GameObject instigator)
    {
        base.Interact(instigator);
        ActivateJumpPad(instigator);
    }
}