using UnityEngine;
using UnityEngine.InputSystem; // Obligatoire pour le nouveau système

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 5f;
    public GhostSlot ghostSlot; 
    
    [Header("Configuration Input")]
    // Cela créera une interface directement dans l'inspecteur d'Unity
    public InputAction moveAction; 

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isOnBelt = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
        rb.freezeRotation = true; 
    }

    // Le New Input System nécessite d'activer et désactiver l'action manuellement
    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    void Update()
    {
        // Lit la valeur du vecteur 2D (WASD, Flèches ou Joystick)
        moveInput = moveAction.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 finalVelocity = moveInput * moveSpeed;

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