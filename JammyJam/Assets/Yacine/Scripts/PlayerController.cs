using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement & Inertie")]
    public float moveSpeed = 5f;
    public float acceleration = 30f;
    public float deceleration = 40f;

    [Header("Magnétisme (Snap)")]
    [Tooltip("Distance à laquelle le joueur est aspiré vers sa place")]
    public float snapRadius = 1.5f;
    [Tooltip("Vitesse d'aspiration")]
    public float snapSpeed = 8f;

    [Header("Dash")]
    [Tooltip("Speed multiplier during the dash")]
    public float dashSpeed = 20f;
    [Tooltip("How long the dash lasts in seconds")]
    public float dashDuration = 0.15f;
    [Tooltip("Time to wait before dashing again")]
    public float dashCooldown = 0.5f;

    [Header("Références")]
    public GhostSlot ghostSlot;
    public InputAction moveAction;
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 playerVelocity;
    private Vector2 lastMoveInput = new Vector2(0, -1); // Regarde vers le bas par défaut
    private bool isOnBelt = false;

    // Dash state variables
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

    public bool IsOnBelt => isOnBelt;
    public bool IsPerfectlySnapped { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // --- DASH INPUT (SPACEBAR) ---
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isDashing && dashCooldownTimer <= 0f)
            {
                StartDash();
            }
        }
        // -----------------------------

        // Mise à jour des animations
        if (animator != null)
        {
            bool isMoving = moveInput.magnitude > 0.01f || isDashing;

            // Don't play the normal walk animation while dashing
            animator.SetBool("isWalking", isMoving && !isDashing);

            // Optional: If you have a dash animation trigger, uncomment this:
            // if (isDashing && dashTimer >= dashDuration - Time.deltaTime) animator.SetTrigger("dash");

            if (isMoving)
            {
                lastMoveInput = isDashing ? dashDirection : moveInput;
                animator.SetFloat("moveX", lastMoveInput.x);
                animator.SetFloat("moveY", lastMoveInput.y);
            }
            else
            {
                // Maintient la direction du regard pour l'idle quand il s'arrête
                animator.SetFloat("moveX", lastMoveInput.x);
                animator.SetFloat("moveY", lastMoveInput.y);
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Dash in current move direction, or last moved direction if standing still
        if (moveInput.magnitude > 0.1f)
            dashDirection = moveInput.normalized;
        else
            dashDirection = lastMoveInput.normalized;

        // Break camouflage immediately when dashing
        IsPerfectlySnapped = false;
    }

    void FixedUpdate()
    {
        // --- DASH LOGIC ---
        if (isDashing)
        {
            // Override velocity completely with dash speed
            playerVelocity = dashDirection * dashSpeed;
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        // --- NORMAL MOVEMENT ---
        else
        {
            // 1. Gestion des inputs et de la vélocité propre du joueur
            if (moveInput.magnitude > 0.01f)
            {
                Vector2 targetVelocity = moveInput * moveSpeed;
                playerVelocity = Vector2.MoveTowards(playerVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

                // Si le joueur touche aux commandes, il est d'office une anomalie
                IsPerfectlySnapped = false;
            }
            else
            {
                playerVelocity = Vector2.MoveTowards(playerVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);

                // 2. Mécanique de Snap (Magnétisme fluide)
                if (isOnBelt && ghostSlot != null)
                {
                    Vector2 vectorToGhost = ghostSlot.transform.position - transform.position;
                    float distanceToGhost = vectorToGhost.magnitude;

                    if (distanceToGhost <= snapRadius)
                    {
                        playerVelocity = vectorToGhost * (snapSpeed * 2f);
                        if (playerVelocity.magnitude > snapSpeed)
                        {
                            playerVelocity = playerVelocity.normalized * snapSpeed;
                        }
                    }

                    // Le joueur est camouflé s'il a lâché les commandes et qu'il est très proche du centre
                    IsPerfectlySnapped = (distanceToGhost <= 0.1f);
                }
                else
                {
                    IsPerfectlySnapped = false;
                }
            }
        }

        // 3. Application de la vélocité finale
        Vector2 finalVelocity = playerVelocity;

        // Add belt velocity (conveyor movement)
        if (isOnBelt && ghostSlot != null)
        {
            finalVelocity += ghostSlot.CurrentVelocity;
        }

        rb.linearVelocity = finalVelocity;
    }

    public void SetOnBelt(bool state)
    {
        isOnBelt = state;
    }
}