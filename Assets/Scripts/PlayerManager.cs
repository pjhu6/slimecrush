using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Debug")]
    public bool isDebug = false;

    [Header("Movement Parameters")]
    public float moveSpeed = 5f;
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 0.75f; 
    public float wallBounceMultipler = 0.5f;
    public float dashSpeed = 7f;
    public float dashDuration = 0.2f;
    public float apexVelocityThreshold = 0.2f;
    public float apexHoldDuration = 0.2f;
    public float crouchDuration = 0.2f;
    public float dashAnimationBufferDuration = 0.2f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip landingSound;
    public AudioClip deathSound;
    public AudioClip victorySound;
    public AudioClip grappleSound;
    private AudioSource audioSource;

    [Header("Grappling Hook")]
    public float hookMaxDistance = 10f;
    public float hookPullSpeed = 15f;
    public LayerMask grappleLayer;
    public LineRenderer hookLine;

    [Header("Camera")]
    public Camera mainCamera;

    [Header("Grapple State")]
    [SerializeField]
    private bool isGrappling;
    [SerializeField]
    private bool isGrappleAvailable;
    [SerializeField]
    private bool hookAttached;
    [SerializeField]
    private Vector2 hookPoint;
    [SerializeField]
    private Vector2 hookVisualPoint;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2); 
    
    private Rigidbody2D rb;

    [Header("Player State")]
    [SerializeField]
    private Vector2 velocity;
    private float inputAxis;

    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private bool isJumping;
    [SerializeField]
    private bool isDashing;
    [SerializeField]
    private bool isDashingAnimation;
    [SerializeField]
    private bool isDashAvailable;
    [SerializeField]
    private bool facingRight;
    

    [SerializeField]
    private bool isCrouched;
    [SerializeField]
    private int apexState;

    [Header("Victory State")]
    [SerializeField] private bool isVictoryStarted = false;

    private SpriteRenderer[] spriteRenderers;
    private Dictionary<string, SpriteRenderer> spriteMap;

    private Coroutine crouchCoroutine;
    private Coroutine apexCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        spriteMap = new Dictionary<string, SpriteRenderer>
        {
            { "Default", spriteRenderers[0] },
            { "Crouch", spriteRenderers[1] },
            { "Airborne", spriteRenderers[2] },
            { "Apex", spriteRenderers[3] },
            { "Dash", spriteRenderers[4]}
        };

        audioSource = GetComponent<AudioSource>();

        // When debug, set y position to 101
        if (isDebug)
        {
            transform.position = new Vector2(transform.position.x, 101f);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Victory)
        {
            if (!isVictoryStarted)
            {
                isVictoryStarted = true;
                StartCoroutine(VictorySequence());
            }
            return;
        }

        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        bool wasGrounded = isGrounded;
        isGrounded = rb.Raycast(Vector2.down);

        if (isGrounded)
        {
            isGrappleAvailable = true;
            if (!wasGrounded)
            {
                HandleLanding();
            }
            GroundedMovement();
        }
        else
        {
            AirMovement();
        }

        HandleGrapple();

        ApplyGravity();
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing 
        && GameManager.Instance.CurrentState != GameState.Victory)
        {
            return;
        }

        // Update sprite based on current state
        UpdateSprite();
        UpdateGrappleLine();
    }

    private void UpdateGrappleLine()
    {
        if (!isGrappling)
        {
            hookLine.positionCount = 0;
            return;
        }

        hookLine.positionCount = 2;

        hookLine.SetPosition(0, rb.position);
        hookLine.SetPosition(1, hookVisualPoint);
    }

    private void HandleGrapple()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartGrapple();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseGrapple();
        }

        // Release grapple
        if (isGrappling && hookAttached && Input.GetButtonDown("Jump"))
        {
            ReleaseGrapple();
            velocity.y = jumpForce * 0.75f;
        }

        if (isGrappling && hookAttached)
        {
            GrappleMovement();
        }
    }

    private void HorizontalMovement()
    {
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = inputAxis * moveSpeed;

        // Check for running into a wall
        if (IsHittingWall())
        {
            velocity.x = 0f;
        }

        // Rotate sprite based when moving left/right
        if (velocity.x > 0f)
        {
            facingRight = true;
            transform.eulerAngles = Vector3.zero;
        } else if (velocity.x < 0f)
        {
            facingRight = false;
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        if (hookAttached)
        {
            return;
        }

        // Make sure y velocity is negative while on the ground (since gravity is always pulling down)
        velocity.y = Mathf.Max(velocity.y, 0f);

        // Move left/right and jump can only be performed on the ground
        HorizontalMovement();
        HandleJump();

        // Cap horizontal velocity on the ground (prevent moving to fast after dash)
        velocity.x = Mathf.Clamp(velocity.x, -moveSpeed, moveSpeed);
    }

    private void AirMovement()
    {
        if (hookAttached)
        {
            return;
        }

        HorizontalMovement();

        if (!isDashing && Input.GetButtonDown("Dash")) // Replace "Dash" with your input name
        {
            StartCoroutine(Dash(dashSpeed, dashDuration));
        }

        if (!isDashing)
        {
            // Bounce off walls in air.
            // We'll take care of bouncing during dash in Dash coroutine.
            if (IsHittingWall())
            {
                BounceOffWall();
            }
        }

        // 2. Apex Trigger
        // We trigger this if we are "slow" (within threshold) AND we haven't been marked as falling yet.
        // This catches the exact transition frame or the floaty frames at the top.
        if (isJumping && !isGrounded &&
            velocity.y < apexVelocityThreshold && 
            velocity.y > 0f &&
            apexState == 0 && 
            !isCrouched) 
        {
            Debug.Log("Apex reached, velocity: " + velocity.y);
            // TODO fix apex
            StartCoroutine(ApexCoroutine());
        }
    }

    private void HandleLanding()
    {
        // Landed on the ground
        apexState = 0;
        audioSource.PlayOneShot(landingSound);
        Debug.Log("Landed");
        ShowCrouchForDuration(crouchDuration);
    }

    private void HandleJump()
    {
        // y velocity always negative, except right after jump
        isJumping = velocity.y > 0f;

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            isJumping = true;
            isDashAvailable = true;  // Get new dash upon jump
            audioSource.PlayOneShot(jumpSound);
        }
    }

    private void ApplyGravity()
    {
        if (isDashing || hookAttached)
        {
            // Disable gravity while dashing or grappling
            return;
        }

        // Jump higher (less gravity) if jump button is held down longer
        bool isFalling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = isFalling ? 2f : 1f;

        velocity.y += gravity * multiplier * Time.deltaTime;

        // Terminal velocity
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    // Update based on time, not frames, for accurate physics
    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing 
        && GameManager.Instance.CurrentState != GameState.Victory)
        {
            return;
        }

        Vector2 position = rb.position;
        position += velocity * Time.fixedDeltaTime;

        rb.MovePosition(position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset vertical velocity on collision
        if (collision.gameObject.layer != LayerMask.NameToLayer("Item"))
        {
            if (transform.DotTest(collision.transform, Vector2.up))
            {
                velocity.y = 0f;
            }
        }

        // Check for collision with objects tagged as "Poison"
        if (collision.gameObject.CompareTag("Poison"))
        {
            HandleDeath();
        }


        // TODO move win detection to platform, not player. Each win platform will detect if their win
        // Has been detected already and which level to complete
        // Win game when above last platform
        // if (collision.gameObject.CompareTag("WinPlatform"))
        // {
        //     Debug.Log("Collided with WinPlatform");
        //     if (transform.DotTest(collision.transform, Vector2.down) 
        //     && rb.Raycast(Vector2.down))  // check is grounded as well
        //     {
        //         GameManager.Instance.Win();
        //     }
        // }
    }

    private void HandleDeath()
    {
        Debug.Log("Dead :(");
        audioSource.PlayOneShot(deathSound);
        // TODO make this a coroutine with animation, gameover at the end of animation
        GameManager.Instance.GameOver();
    }

    private bool IsHittingWall()
    {
        // Check for wall collision in the direction of horizontal movement
        return rb.Raycast(Vector2.right * Mathf.Sign(velocity.x));
    }

    private void OnDrawGizmos()
    {
        if (rb == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(rb.position, 0.05f);

        // Draw grapple point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hookPoint, 0.05f);
    }

    private IEnumerator Dash(float speed, float duration)
    {
        if (!isDashAvailable)
        {
            yield break; // Dash not available, probably used in this jump already
        }

        audioSource.PlayOneShot(dashSound);
        isDashing = true;
        isDashingAnimation = true;
        isDashAvailable = false;

        // Start coroutine to end dash animation after duration
        StartCoroutine(EndDashAnimationAfterDuration(dashAnimationBufferDuration));

        velocity.y = 0f;

        // Temporarily disable gravity
        float originalGravity = gravity;
        float dashEndTime = Time.time + duration;
        while (Time.time < dashEndTime)
        {
            if (IsHittingWall())
            {
                // Stop dash if hitting a wall
                Debug.Log("Dash interrupted by wall");
                BounceOffWall();
                break;
            }

            if (isGrounded)
            {
                // Stop dash if grounded
                Debug.Log("Dash ended on ground");
                break;
            }

            velocity.x = speed * (facingRight ? 1 : -1);
            yield return null;
        }

        isDashing = false;
    }

    
    private void BounceOffWall()
    {
        velocity.x = -velocity.x * wallBounceMultipler;
    }

    private void UpdateSprite()
    {
        // Hide all SpriteRenderers initially
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = false;
        }

        if (GameManager.Instance.CurrentState == GameState.Victory)
        {
            spriteMap["Default"].enabled = true;  // Ensure use default sprite on victory
        }
        else if (isGrappling)
        {
            spriteMap["Airborne"].enabled = true; // Show airborne sprite while grappling
        }
        else if (isCrouched)
        {
            spriteMap["Crouch"].enabled = true; // Show crouch sprite
        }
        else if (isDashingAnimation)
        {
            spriteMap["Dash"].enabled = true; // Show dash sprite
        }
        else if (apexState == 2)
        {
            spriteMap["Apex"].enabled = true;
        }
        else if (apexState == 1)
        {
            spriteMap["Default"].enabled = true; // Use default as transition to apex
        }
        else if (isJumping)
        {
            spriteMap["Airborne"].enabled = true; // Airborne sprite
        }
        else
        {
            spriteMap["Default"].enabled = true; // Default sprite
        }
    }

    private void ShowCrouchForDuration(float duration)
    {
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
        }
        crouchCoroutine = StartCoroutine(ShowCrouchCoroutine(duration));
    }

    private IEnumerator ShowCrouchCoroutine(float duration)
    {
        isCrouched = true;
        yield return new WaitForSeconds(duration);
        isCrouched = false;
    }

    private IEnumerator ApexCoroutine()
    {
        // Hack the animation states using apexState integer
        if (isGrounded || isCrouched)
        {
            yield break;
        }
        apexState = 1;

        yield return new WaitForSeconds(apexHoldDuration / 4); // Wait for 1/4 of the duration
        if (isGrounded || isCrouched)
        {
            yield break;
        }
        apexState = 2;

        yield return new WaitForSeconds(apexHoldDuration / 2); // Wait for the next 2/4 (3/4 total)
        if (isGrounded || isCrouched)
        {
            yield break;
        }
        apexState = 1;

        yield return new WaitForSeconds(apexHoldDuration / 4); // Wait for the final 1/4
        if (isGrounded || isCrouched)
        {
            yield break;
        }

        apexState = 0;
    }

    private IEnumerator EndDashAnimationAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        isDashingAnimation = false;
    }

    private void TryStartGrapple()
    {
        if (isGrappling || !isGrappleAvailable)
        {
            return;
        }

        
        isGrappleAvailable = false;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = rb.position;
        Vector2 direction = (mouseWorld - origin).normalized;

        origin += direction * 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            hookMaxDistance,
            grappleLayer
        );

        isGrappling = true;

        // Valid grapple: hit something, not a wall, AND hit the bottom of a collider
        if (hit.collider != null &&
            !hit.collider.CompareTag("Wall") &&
            hit.normal.y < -0.5f)
        {
            audioSource.PlayOneShot(grappleSound);
            hookAttached = true;
            hookPoint = hit.point;
            hookVisualPoint = hit.point;

            velocity = Vector2.zero;
            isDashing = false;
        }
        else
        {
            // Missed or hit side/top — still draw the rope, but don’t attach
            hookAttached = false;
            hookVisualPoint = origin + direction * hookMaxDistance;
            StartCoroutine(RetractHook());
        }
    }



    private void GrappleMovement()
    {
        Vector2 toHook = hookPoint - rb.position;
        float distance = toHook.magnitude;

        // Stop when close enough
        if (distance < 0.3f)
        {
            velocity = Vector2.zero;
            return;
        }

        Vector2 pullDir = toHook.normalized;

        velocity = pullDir * hookPullSpeed;

        // Optional: swing control
        float horizontal = Input.GetAxis("Horizontal");
        velocity.x += horizontal * moveSpeed * 0.5f;
    }

    private void ReleaseGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        hookAttached = false;
        hookLine.positionCount = 0;
    }

    private IEnumerator RetractHook(float speed = 20f)
    {
        while (Vector2.Distance(hookVisualPoint, rb.position) > 0.2f)
        {
            hookVisualPoint = Vector2.MoveTowards(
                hookVisualPoint,
                rb.position,
                speed * Time.deltaTime
            );

            yield return null;
        }

        ReleaseGrapple();
    }

    private IEnumerator VictorySequence()
    {
        // Cache original camera size
        float originalSize = mainCamera.orthographicSize;

        // 1. Play victory sound
        audioSource.PlayOneShot(victorySound);

        // 2. Walk right + zoom in until we hit the wall
        while (!IsHittingWall())
        {
            // Camera Zoom
            mainCamera.orthographicSize =
                Mathf.Lerp(mainCamera.orthographicSize, 3f, Time.deltaTime);

            // Move Right
            velocity.y = 0f;
            velocity.x = 2f;

            rb.MovePosition(rb.position + velocity * Time.deltaTime);

            yield return null;
        }

        // Stop movement
        velocity = Vector2.zero;

        // 3. End level coroutine
        yield return StartCoroutine(GameManager.Instance.EndLevelCoroutine());

        // 4. Instantly reset camera size
        mainCamera.orthographicSize = originalSize;
        GameManager.Instance.Resume();
    }
}