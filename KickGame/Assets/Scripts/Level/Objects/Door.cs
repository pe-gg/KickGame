using UnityEngine;

public class Door : LevelObjects
{
    [Header("Door Settings")]
    public float openAngle = 90f;             
    public float openSpeed = 2f;
    public float closeDelay = 2f;            
    public bool autoClose = true;              
    public bool canClose = true;              

    private bool isOpening = false;            
    private bool isClosing = false;               
    private Quaternion closedRotation;          
    private Quaternion openRotation;           
    private float openTimer = 0f;

    public GameObject Pivot;
    private void Start()
    {
        // Save the closed and open rotation for the door
        closedRotation = Pivot.transform.rotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;
    }

    public void OpenDoor()
    {
        if (!isOpening && !isClosing)
        {
            Debug.Log($"{objectName} is opening.");
            isOpening = true;
            isClosing = false;
        }
    }

    public void CloseDoor()
    {
        if(!canClose) return;
        if (!isClosing && !isOpening)
        {
            Debug.Log($"{objectName} is closing.");
            isClosing = true;
            isOpening = false;
        }
    }

    private void Update()
    {
        if (isOpening)
        {
            // Rotate door towards open position over time
            Pivot.transform.rotation = Quaternion.Slerp(Pivot.transform.rotation, openRotation, Time.deltaTime * openSpeed);

            // Check if door is fully open
            if (Quaternion.Angle(Pivot.transform.rotation, openRotation) < 0.1f)
            {
                Pivot.transform.rotation = openRotation;
                isOpening = false;

                if (autoClose)
                {
                    openTimer = closeDelay;  // Start close delay timer
                }
            }
        }
        else if (isClosing)
        {
            // Rotate door towards closed position over time
            Pivot.transform.rotation = Quaternion.Slerp(Pivot.transform.rotation, closedRotation, Time.deltaTime * openSpeed);

            // Check if door is fully closed
            if (Quaternion.Angle(Pivot.transform.rotation, closedRotation) < 0.1f)
            {
                Pivot.transform.rotation = closedRotation;
                isClosing = false;
            }
        }

        // Handle auto-closing if enabled
        if (autoClose && !isOpening && openTimer > 0f)
        {
            openTimer -= Time.deltaTime;
            if (openTimer <= 0f)
            {
                CloseDoor();
            }
        }
    }

    public override void Interact(GameObject instigator)
    {
        base.Interact(instigator);
        PlayerKick KickScript;
        KickScript = instigator.GetComponent<PlayerKick>();
        if(!KickScript) return;
        
        if (true) // KickScript.IsKicking()
        {
            if (!isOpening && Pivot.transform.rotation == closedRotation)
            {
                OpenDoor();
            }
            else if (!isClosing && Pivot.transform.rotation == openRotation)
            {
                CloseDoor();
            }
        }
    }
}
