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
    
    [Header("Références")]
    public GhostSlot ghostSlot; 
    public InputAction moveAction; 
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 playerVelocity;
    private Vector2 lastMoveInput = new Vector2(0, -1); // Regarde vers le bas par défaut
    private bool isOnBelt = false;
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

        // Mise à jour des animations
        if (animator != null)
        {
            bool isMoving = moveInput.magnitude > 0.01f;
            
            // Dit à l'Animator si on marche ou si on est idle
            animator.SetBool("isWalking", isMoving);

            if (isMoving)
            {
                lastMoveInput = moveInput;
                animator.SetFloat("moveX", moveInput.x);
                animator.SetFloat("moveY", moveInput.y);
            }
            else
            {
                // Maintient la direction du regard pour l'idle quand il s'arrête
                animator.SetFloat("moveX", lastMoveInput.x);
                animator.SetFloat("moveY", lastMoveInput.y);
            }
        }
    }

    void FixedUpdate()
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

        // 3. Application de la vélocité finale
        Vector2 finalVelocity = playerVelocity;
        
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