using UnityEngine;

public abstract class PlayerSlotCheckpoint : MonoBehaviour
{
    public LineManager lineManager;

    [Tooltip("Usually the Path object containing p1, p2, p3, etc. If empty, LineManager.pathParent will be used.")]
    public Transform pathParent;

    [Tooltip("Optional waypoint or checkpoint object used to calculate the distance along the belt. If empty, this GameObject transform is used.")]
    public Transform checkpointPoint;

    [Tooltip("If true, uses manualDistance instead of calculating distance from pathParent/checkpointPoint.")]
    public bool useManualDistance = false;

    public float manualDistance = 0f;

    [Tooltip("If true, this checkpoint fires only once.")]
    public bool fireOnce = true;

    [Tooltip("If true, prints checkpoint debug logs.")]
    public bool enableDebugLogs = true;

    [SerializeField] protected float checkpointDistance;
    [SerializeField] protected bool hasFired;

    protected virtual void Start()
    {
        // Important: reset in case the value was left true from a previous play session.
        hasFired = false;

        if (lineManager == null)
            lineManager = Object.FindFirstObjectByType<LineManager>();

        if (pathParent == null && lineManager != null)
            pathParent = lineManager.pathParent;

        checkpointDistance = CalculateDistance();

        if (enableDebugLogs)
        {
            string pathName = pathParent != null ? pathParent.name : "null";

            Debug.Log(
                $"[{GetType().Name}] Initialized. " +
                $"checkpointDistance={checkpointDistance}, " +
                $"useManualDistance={useManualDistance}, " +
                $"pathParent={pathName}"
            );
        }
    }

    protected virtual void FixedUpdate()
    {
        if (lineManager == null || hasFired)
            return;

        if (lineManager.GetTotalLength() <= 0f)
            return;

        float previous = lineManager.GetPreviousPlayerSlotDistance();
        float current = lineManager.GetPlayerSlotDistance();

        // Inclusive crossing check.
        // This also handles cases where the checkpoint is exactly at the starting distance.
        bool crossed = previous <= checkpointDistance && current >= checkpointDistance;

        if (crossed)
        {
            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[{GetType().Name}] Player slot crossed checkpoint. " +
                    $"checkpointDistance={checkpointDistance}, " +
                    $"previous={previous}, " +
                    $"current={current}"
                );
            }

            if (fireOnce)
                hasFired = true;

            OnPlayerSlotCrossed();
        }
    }

    protected abstract void OnPlayerSlotCrossed();

    private float CalculateDistance()
    {
        if (useManualDistance)
            return Mathf.Max(0f, manualDistance);

        Transform point = checkpointPoint != null ? checkpointPoint : transform;

        if (pathParent == null || point == null)
            return 0f;

        int childCount = pathParent.childCount;

        if (childCount <= 0)
            return 0f;

        Vector2[] points = new Vector2[childCount];
        Transform[] children = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = pathParent.GetChild(i);
            children[i] = child;
            points[i] = child.position;
        }

        // If the checkpoint point is one of the path waypoints, use its exact cumulative distance.
        float cumulative = 0f;

        for (int i = 0; i < childCount; i++)
        {
            if (i > 0)
                cumulative += Vector2.Distance(points[i - 1], points[i]);

            if (children[i] == point)
                return cumulative;
        }

        // Otherwise, project the checkpoint point onto the closest path segment.
        if (childCount < 2)
            return 0f;

        float bestDistance = 0f;
        float bestSqrDistance = float.MaxValue;
        cumulative = 0f;

        Vector2 pointPosition = point.position;

        for (int i = 0; i < childCount - 1; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[i + 1];

            Vector2 segment = b - a;
            float segmentLength = segment.magnitude;

            if (segmentLength <= 0.0001f)
                continue;

            float t = Vector2.Dot(pointPosition - a, segment) / (segmentLength * segmentLength);
            t = Mathf.Clamp01(t);

            Vector2 projectedPoint = a + segment * t;
            float sqrDistance = (pointPosition - projectedPoint).sqrMagnitude;

            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                bestDistance = cumulative + segmentLength * t;
            }

            cumulative += segmentLength;
        }

        return bestDistance;
    }
}