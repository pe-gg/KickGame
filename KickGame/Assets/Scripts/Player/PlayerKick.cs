using System.Collections;
using Player;
using UnityEngine;

/// <summary>
/// Handles kicking actions associated with the Mouse1 input, including ground kicks and dive kicks.
/// </summary>
public class PlayerKick : MonoBehaviour
{
    #region Variables

    [Header("Kick Settings")]
    [SerializeField] private float kickRange = 2f;
    [SerializeField] private float kickForce = 10f;
    [SerializeField] private float kickCooldownSeconds = 0.2f; // Reduced for responsiveness

    [Header("Divekick Settings")]
    [SerializeField] private float diveKickInitialForce = 20f;
    [SerializeField] private float diveKickAcceptanceAngle = 30f; // Angle in degrees
    [SerializeField] private float diveKickAcceptanceDistance = 10f; // Max distance to lock on
    [SerializeField] private float maxDiveKickUpwardsAngle = -10f; // Max upwards angle for divekick
    [SerializeField] private float diveKickCooldownSeconds = 1f;
    [SerializeField] private Collider diveKickCollider;
    [SerializeField] private AnimationCurve diveKickSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float maxDiveKickDuration = 2f;
    [SerializeField] private float inputResponsiveness = 2f;

    [Header("Initial Upward Force Settings")]
    [SerializeField] private float initialUpwardForce = 15f; // Adjust as needed
    [SerializeField] private float initialUpwardForceDuration = 0.2f; // Duration to apply upward force

    [Header("Visual and Audio Effects")]
    [SerializeField] private ParticleSystem diveKickParticles;
    [SerializeField] private AudioClip diveKickSound;
    [SerializeField] private AudioClip groundKickSound;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float cameraShakeIntensity = 0.1f;
    [SerializeField] private float cameraShakeDuration = 0.2f;
    
    private bool isKicking = false;
    private bool isDiveKicking = false;
    private float lastKickTime = -Mathf.Infinity;
    private float lastDiveKickTime = -Mathf.Infinity;
    private Vector3 diveKickDirection;

    private bool LimitDiveKick = false;

    private Camera cam;
    private PlayerState state;
    private Rigidbody rb;
    private PlayerMovement move;
    private ViewmodelAnimator anim;
    private Animator animator;
    private AudioSource audioSource;

    private IKickable lockedOnKickable;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        state = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody>();
        move = GetComponent<PlayerMovement>();
        anim = GetComponent<ViewmodelAnimator>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (diveKickCollider != null)
            diveKickCollider.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDiveKicking)
        {
            IKickable kickable = other.GetComponent<IKickable>();
            if (kickable != null)
            {
                kickable.OnKick(gameObject);
                PlayImpactEffects();

                // Apply additional force to the kickable object
                Rigidbody rbKickable = other.GetComponent<Rigidbody>();
                if (rbKickable != null)
                {
                    // Ensure the direction is clamped before applying force
                    Vector3 clampedDirection = ClampDiveKickDirection(diveKickDirection);
                    rbKickable.AddForce(clampedDirection * kickForce, ForceMode.Impulse);
                }

                // Bounce upwards
                rb.velocity = new Vector3(rb.velocity.x, diveKickInitialForce * 0.5f, rb.velocity.z);

                // End divekick
                isDiveKicking = false;
                EndDiveKick();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDiveKicking)
        {
            // Check if collided with the ground or other obstacles
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer != gameObject.layer)
            {
                isDiveKicking = false;
                PlayImpactEffects();
                EndDiveKick();
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called by PlayerInputManager. If the player is in the air, perform a divekick. Otherwise, perform a ground kick.
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
    /// Returns whether the player is currently performing a ground kick.
    /// </summary>
    public bool IsKicking()
    {
        return isKicking;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Performs a ground kick action with physics-based force application.
    /// </summary>
    private void GroundKick()
    {
        if (isKicking)
            return;

        isKicking = true;
        anim.GroundKick();
        animator.SetTrigger("GroundKick"); // Assuming Animator has a GroundKick trigger

        PlayGroundKickEffects();

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

        // Check the player's view angle. Prevent divekick if looking too far upwards.
        float camPitch = cam.transform.eulerAngles.x;
        if (camPitch > 180f)
            camPitch -= 360f; // Convert angle to -180 to 180 range

        if (camPitch < maxDiveKickUpwardsAngle)
        {
            Debug.Log("Cannot divekick upwards beyond allowed angle.");
            LimitDiveKick = true;
            
            diveKickDirection = cam.transform.forward;
        }
        else
        {
            // Find a target kickable object within acceptance parameters
            FindDiveKickTarget();

            // Set diveKickDirection towards the target or forward
            if (lockedOnKickable != null)
            {
                // Cast to Component to access transform
                Component kickableComponent = lockedOnKickable as Component;
                if (kickableComponent != null)
                {
                    diveKickDirection = (kickableComponent.transform.position - transform.position).normalized;
                }
                else
                {
                    // Fallback if casting fails
                    diveKickDirection = cam.transform.forward;
                }
            }
            else
            {
                diveKickDirection = cam.transform.forward;
            }
        }

        // Clamp the diveKickDirection to prevent upward direction
        diveKickDirection = ClampDiveKickDirection(diveKickDirection);

        StartCoroutine(DiveKickCoroutine());
        lastDiveKickTime = Time.time;
    }

    /// <summary>
    /// Clamps the dive kick direction to ensure it's not upwards.
    /// </summary>
    /// <param name="direction">Original dive kick direction.</param>
    /// <returns>Clamped dive kick direction.</returns>
    private Vector3 ClampDiveKickDirection(Vector3 direction)
    {
        Vector3 clampedDir = direction.normalized;

        if (clampedDir.y > maxDiveKickUpwardsAngle)
        {
            clampedDir.y = maxDiveKickUpwardsAngle;
            clampedDir.Normalize();
        }

        return clampedDir;
    }

    /// <summary>
    /// Finds a target kickable object within the acceptance angle and distance.
    /// </summary>
    private void FindDiveKickTarget()
    {
        lockedOnKickable = null;
        float closestAngle = Mathf.Infinity;
        Vector3 targetPosition = Vector3.zero;

        // Define a layer mask for kickable objects to optimize performance
        int kickableLayerMask = LayerMask.GetMask("Kickable"); // Ensure Kickable objects are on this layer

        Collider[] colliders = Physics.OverlapSphere(transform.position, diveKickAcceptanceDistance, kickableLayerMask);
        foreach (Collider collider in colliders)
        {
            IKickable kickable = collider.GetComponent<IKickable>();
            if (kickable != null)
            {
                Vector3 directionToKickable = (collider.transform.position - cam.transform.position).normalized;
                float angle = Vector3.Angle(cam.transform.forward, directionToKickable);

                if (angle <= diveKickAcceptanceAngle && angle < closestAngle)
                {
                    // Ensure the object is not above the player's Y position
                    if (collider.transform.position.y <= transform.position.y)
                    {
                        lockedOnKickable = kickable;
                        targetPosition = collider.transform.position;
                        closestAngle = angle;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Coroutine that handles the divekick movement and timing, including initial upward force.
    /// </summary>
    private IEnumerator DiveKickCoroutine()
    {
        isDiveKicking = true;

        // Play divekick animations and effects
        anim.DiveKickStart();
        animator.SetTrigger("DiveKick"); // Assuming Animator has a DiveKick trigger
        PlayDiveKickEffects();

        // Phase 1: Apply initial upward force
        rb.AddForce(Vector3.up * initialUpwardForce, ForceMode.Impulse);
        yield return new WaitForSeconds(initialUpwardForceDuration);

        // Phase 2: Apply forward dive kick force
        rb.AddForce(diveKickDirection * diveKickInitialForce, ForceMode.Impulse);

        // Activate divekick collider
        if (diveKickCollider != null)
            diveKickCollider.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < maxDiveKickDuration && isDiveKicking)
        {
            float normalizedTime = elapsedTime / maxDiveKickDuration;
            float speedMultiplier = diveKickSpeedCurve.Evaluate(normalizedTime);

            // Apply additional force based on the animation curve
            rb.AddForce(diveKickDirection * diveKickInitialForce * speedMultiplier, ForceMode.Acceleration);

            // Allow player to slightly influence direction
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(horizontal, 0, vertical).normalized;

            if (inputDir.magnitude > 0)
            {
                Vector3 adjustedDirection = Vector3.Lerp(diveKickDirection, (diveKickDirection + inputDir * 0.5f), Time.deltaTime * inputResponsiveness).normalized;
                // Clamp the adjusted direction to prevent upward direction
                diveKickDirection = ClampDiveKickDirection(adjustedDirection);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // End divekick after duration
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
        animator.ResetTrigger("DiveKick"); // Reset the trigger if necessary

        isDiveKicking = false;
    }

    /// <summary>
    /// Handles cooldown for ground kicks.
    /// </summary>
    /// <returns></returns>
    private IEnumerator KickCooldown()
    {
        yield return new WaitForSeconds(kickCooldownSeconds);
        isKicking = false;
    }

    /// <summary>
    /// Plays visual and audio effects for divekick initiation.
    /// </summary>
    private void PlayDiveKickEffects()
    {
        if (diveKickParticles != null)
            diveKickParticles.Play();

        if (audioSource != null && diveKickSound != null)
            audioSource.PlayOneShot(diveKickSound);

        // Implement camera shake
        StartCoroutine(CameraShake(cameraShakeIntensity, cameraShakeDuration));
    }

    /// <summary>
    /// Plays visual and audio effects for ground kick.
    /// </summary>
    private void PlayGroundKickEffects()
    {
        if (audioSource != null && groundKickSound != null)
            audioSource.PlayOneShot(groundKickSound);

        // Implement camera shake
        StartCoroutine(CameraShake(cameraShakeIntensity * 0.5f, cameraShakeDuration * 0.5f));
    }

    /// <summary>
    /// Plays visual and audio effects upon impact.
    /// </summary>
    private void PlayImpactEffects()
    {
        if (audioSource != null && impactSound != null)
            audioSource.PlayOneShot(impactSound);

        if (diveKickParticles != null)
            diveKickParticles.Stop();
    }

    /// <summary>
    /// Coroutine to handle camera shake effect.
    /// </summary>
    /// <param name="intensity">Intensity of the shake.</param>
    /// <param name="duration">Duration of the shake.</param>
    /// <returns></returns>
    private IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            cam.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }

    #endregion
}
