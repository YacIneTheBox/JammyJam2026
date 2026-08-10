using UnityEngine;

public class GhostSlot : MonoBehaviour
{
    [Header("Configuration du Tapis")]
    public Transform[] waypoints;
    public float scrollSpeed = 3f;
    
    private int currentWaypointIndex = 0;
    
    // Le joueur aura besoin de lire cette vélocité
    public Vector2 CurrentVelocity { get; private set; }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        
        // Calcul de la direction vers le prochain point
        Vector2 direction = (targetWaypoint.position - transform.position).normalized;
        
        // Calcul et stockage de la vélocité
        CurrentVelocity = direction * scrollSpeed;
        
        // Déplacement du fantôme
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, scrollSpeed * Time.fixedDeltaTime);

        // Si on a atteint le waypoint actuel, on passe au suivant
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                currentWaypointIndex++;
            }
            else
            {
                // Fin du niveau ou boucle
                CurrentVelocity = Vector2.zero; 
            }
        }
    }
}