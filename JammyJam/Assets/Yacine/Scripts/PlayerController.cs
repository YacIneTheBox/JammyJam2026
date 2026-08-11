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

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 playerVelocity;
    private bool isOnBelt = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
        rb.freezeRotation = true; 
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // 1. Gestion des inputs et de la vélocité propre du joueur
        if (moveInput.magnitude > 0.01f)
        {
            // Le joueur se déplace activement : on accélère vers la direction voulue
            Vector2 targetVelocity = moveInput * moveSpeed;
            playerVelocity = Vector2.MoveTowards(playerVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Le joueur a lâché les contrôles : on freine naturellement d'abord
            playerVelocity = Vector2.MoveTowards(playerVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            
            // 2. Mécanique de Snap (Magnétisme fluide)
            if (isOnBelt && ghostSlot != null)
            {
                Vector2 vectorToGhost = ghostSlot.transform.position - transform.position;
                float distanceToGhost = vectorToGhost.magnitude;

                if (distanceToGhost <= snapRadius)
                {
                    // La vitesse diminue naturellement en approchant de 0, ce qui empêche le tremblement
                    playerVelocity = vectorToGhost * (snapSpeed * 2f);
                    
                    // On plafonne la vitesse pour ne pas aspirer trop violemment quand il est à la limite du rayon
                    if (playerVelocity.magnitude > snapSpeed)
                    {
                        playerVelocity = playerVelocity.normalized * snapSpeed;
                    }
                }
            }
        }

        // 3. Application de la vélocité finale (Vélocité du joueur + Vélocité du tapis)
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