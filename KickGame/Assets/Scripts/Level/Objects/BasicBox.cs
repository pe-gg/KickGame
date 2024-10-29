using UnityEngine;

public class BasicBox : LevelObjects
{
    public override void Interact(GameObject instigator)
    {
        base.Interact(instigator);
        Debug.Log($"{objectName} is a basic box.");
        // Basic box-specific interaction logic, if any
    }
}