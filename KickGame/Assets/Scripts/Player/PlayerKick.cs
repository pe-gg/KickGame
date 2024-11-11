using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

/// <summary>
/// Handles kicking actions associated with the Mouse1 input.
/// </summary>
public class PlayerKick : MonoBehaviour
{
    #region Variables

    [Header("Kick Settings")]
    [SerializeField] private float kickRange = 2f;
    [SerializeField] private float kickForce = 10f;
    [SerializeField] private float kickCooldownSeconds = 0.5f;

    [Header("Divekick Settings")]
    [SerializeField] private float diveKickSpeed = 20f;
    [SerializeField] private float diveKickGravityDisableDuration = 0.5f;
    [SerializeField] private float diveKickAcceptanceAngle = 30f; // Angle in degrees
    [SerializeField] private float diveKickAcceptanceDistance = 10f; // Max distance to lock on
    [SerializeField] private float maxDiveKickUpwardsAngle = -10f; // Max upwards angle for divekick
    [SerializeField] private float diveKickCooldownSeconds = 1f;
    [SerializeField] private Collider diveKickCollider;

    private bool isKicking = false;
    private bool isDiveKicking = false;
    private float lastKickTime = -Mathf.Infinity;
    private float lastDiveKickTime = -Mathf.Infinity;
    private Vector3 diveKickDirection;

    private Camera cam;
    private PlayerState state;
    private Rigidbody rb;
    private PlayerMovement move;
    private ViewmodelAnimator anim;

    private IKickable lockedOnKickable;

    #endregion

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        state = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody>();
        move = GetComponent<PlayerMovement>();
        anim = GetComponent<ViewmodelAnimator>();

        if (diveKickCollider != null)
            diveKickCollider.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by PlayerInputManager. If the player is in the air, divekick. Otherwise, ground kick.
    /// </summary>
    public void Kick()
    {
        // If on cooldown, return
        if (Time.time < lastKickTime + kickCooldownSeconds)
            return;

        // If in air, try to divekick
        if (state.currentState == PlayerState.PState.JUMPING)
        {
            DiveKick();
        }
        else
        {
            GroundKick();
            lastKickTime = Time.time;
        }
    }

    /// <summary>
    /// Performs a ground kick action.
    /// </summary>
    private void GroundKick()
    {
        if (isKicking)
            return;

        isKicking = true;
        anim.GroundKick();

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, kickRange))
        {
            IKickable kickable = hit.transform.GetComponent<IKickable>();
            if (kickable != null)
            {
                kickable.OnKick(gameObject);
                Debug.Log("Kicked kickable object!");
            }
            else
            {
                // If it's not kickable, apply force if it has a rigidbody
                Rigidbody rbHit = hit.transform.GetComponent<Rigidbody>();
                if (rbHit != null)
                {
                    rbHit.AddForce(cam.transform.forward * kickForce, ForceMode.Impulse);
                    Debug.Log("Kick success!");
                }
            }
        }

        StartCoroutine(KickCooldown());
    }

    /// <summary>
    /// Initiates the divekick sequence.
    /// </summary>
    private void DiveKick()
    {
        if (isDiveKicking)
            return;

        if (Time.time < lastDiveKickTime + diveKickCooldownSeconds)
            return;

        // Check the player's view angle. If the player is looking upwards beyond maxDiveKickUpwardsAngle, prevent divekick.
        float camAngle = cam.transform.eulerAngles.x;
        if (camAngle > 180f)
            camAngle -= 360f; // Convert angle to -180 to 180 range

        if (camAngle < maxDiveKickUpwardsAngle)
        {
            Debug.Log("Cannot divekick upwards beyond allowed angle.");
            return;
        }

        // Now, check for kickable objects in the player's view direction.
        IKickable targetKickable = null;
        Vector3 targetPosition = Vector3.zero;
        float closestAngle = Mathf.Infinity;

        Collider[] colliders = Physics.OverlapSphere(transform.position, diveKickAcceptanceDistance);
        foreach (Collider collider in colliders)
        {
            IKickable kickable = collider.GetComponent<IKickable>();
            if (kickable != null)
            {
                Vector3 directionToKickable = (collider.transform.position - cam.transform.position).normalized;
                float angle = Vector3.Angle(cam.transform.forward, directionToKickable);

                if (angle <= diveKickAcceptanceAngle && angle < closestAngle)
                {
                    // Also check if the object is not above player's Y position
                    if (collider.transform.position.y <= transform.position.y)
                    {
                        targetKickable = kickable;
                        targetPosition = collider.transform.position;
                        closestAngle = angle;
                    }
                }
            }
        }

        if (targetKickable != null)
        {
            // Lock on to this kickable object
            lockedOnKickable = targetKickable;

            // Set the diveKickDirection towards the kickable object
            diveKickDirection = (targetPosition - transform.position).normalized;

            // Optionally rotate the camera to face the target (Bonus)
            // cam.transform.LookAt(targetPosition);
        }
        else
        {
            // No kickable object found, divekick in direction of camera
            diveKickDirection = cam.transform.forward;
        }

        StartCoroutine(DiveKickCoroutine());
        lastDiveKickTime = Time.time;
    }

    /// <summary>
    /// Coroutine that handles the divekick movement and timing.
    /// </summary>
    private IEnumerator DiveKickCoroutine()
    {
        isDiveKicking = true;

        // Disable gravity
        rb.useGravity = false;

        // Activate divekick collider
        if (diveKickCollider != null)
            diveKickCollider.gameObject.SetActive(true);

        anim.DiveKickStart();

        // Lock movement input
        move.LockMovement(true);

        // For the duration of gravity disable
        float duration = diveKickGravityDisableDuration;
        float elapsed = 0f;

        // Initial thrust
        rb.velocity = diveKickDirection * diveKickSpeed;

        while (elapsed < duration)
        {
            rb.velocity = diveKickDirection * diveKickSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Re-enable gravity
        rb.useGravity = true;

        // Continue moving until divekick ends or max duration reached
        float maxDiveKickDuration = 2f;
        float totalElapsed = 0f;

        while (isDiveKicking && totalElapsed < maxDiveKickDuration)
        {
            rb.velocity = diveKickDirection * diveKickSpeed;
            totalElapsed += Time.deltaTime;
            yield return null;
        }

        // Divekick ended
        EndDiveKick();
    }

    /// <summary>
    /// Ends the divekick sequence and resets variables.
    /// </summary>
    private void EndDiveKick()
    {
        if (diveKickCollider != null)
            diveKickCollider.gameObject.SetActive(false);

        anim.DiveKickEnd();

        move.LockMovement(false);

        isDiveKicking = false;
    }

    /// <summary>
    /// Handles collision with kickable objects during divekick.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isDiveKicking)
        {
            IKickable kickable = other.GetComponent<IKickable>();
            if (kickable != null)
            {
                kickable.OnKick(gameObject);

                // Bounce upwards
                rb.velocity = new Vector3(rb.velocity.x, diveKickSpeed, rb.velocity.z);

                // Optionally, end divekick
                isDiveKicking = false;
                EndDiveKick();
            }
        }
    }

    /// <summary>
    /// Handles collision with other objects during divekick.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (isDiveKicking)
        {
            // End divekick if hitting the ground or other obstacles
            isDiveKicking = false;
            EndDiveKick();
        }
    }

    /// <summary>
    /// Cooldown coroutine for ground kick.
    /// </summary>
    private IEnumerator KickCooldown()
    {
        yield return new WaitForSeconds(kickCooldownSeconds);
        isKicking = false;
    }

    public bool IsKicking()
    {
        return isKicking;
    }
}
