using UnityEngine;

public class GhostSlot : MonoBehaviour
{
    [Header("Line Manager Control")]
    [Tooltip("When true, LineManager controls this ghost slot. Legacy waypoint movement is disabled.")]
    public bool drivenByLineManager = false;

    [Header("Legacy Configuration (only used if not driven by LineManager)")]
    public Transform[] waypoints;
    public float scrollSpeed = 3f;

    private int currentWaypointIndex = 0;

    // PlayerController already reads this velocity.
    public Vector2 CurrentVelocity { get; private set; }

    public void SetLineDriven(bool state)
    {
        drivenByLineManager = state;
    }

    public void SetState(Vector2 position, Vector2 velocity)
    {
        transform.position = position;
        CurrentVelocity = velocity;
    }

    private void FixedUpdate()
    {
        if (drivenByLineManager)
            return;

        if (waypoints == null || waypoints.Length == 0)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        if (currentWaypointIndex >= waypoints.Length)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)targetWaypoint.position - (Vector2)transform.position).normalized;

        CurrentVelocity = direction * scrollSpeed;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetWaypoint.position,
            scrollSpeed * Time.fixedDeltaTime
        );

        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                currentWaypointIndex++;
            }
            else
            {
                CurrentVelocity = Vector2.zero;
            }
        }
    }
}