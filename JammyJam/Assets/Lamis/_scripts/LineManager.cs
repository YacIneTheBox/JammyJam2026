using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class LineManager : MonoBehaviour
{
    public enum PlayerEndBehaviour
    {
        Nothing,
        HideAndDisable,
        DisableGameObject
    }
    [Header("Smoothing")]
    [Tooltip("If disabled, NPCs are moved without Rigidbody interpolation. This prevents activation dash/glitch artifacts.")]
    public bool useNpcInterpolation = false;

    [Header("Path")]
    [Tooltip("Assign the Path GameObject containing p1, p2, p3, etc. as direct children.")]
    public Transform pathParent;

    [Tooltip("Optional fallback if you do not want to use pathParent.")]
    public Transform[] fallbackWaypoints;

    [Tooltip("If true, inactive path children will also be used.")]
    public bool includeInactivePathPoints = false;

    [Header("Line Generation")]
    [Tooltip("Distance between two consecutive line slots.")]
    public float spacing = 2f;

    [Tooltip("Belt movement speed. Should usually match your old GhostSlot scroll speed.")]
    public float scrollSpeed = 3f;

    [Tooltip("Requested player slot index. NPCs before this index are behind the player.")]
    public int playerStartIndex = 5;

    [Tooltip("If true, invalid player indexes are clamped.")]
    public bool autoClampPlayerIndex = true;

    [Tooltip("Optional distance offset before the first visible slot.")]
    public float startOffset = 0f;

    [Tooltip("Entities despawn when they reach totalLength minus this margin.")]
    public float endDespawnMargin = 0.1f;

    [Header("Player Start Mode")]
    [Tooltip("If true, the player slot is anchored at the beginning of the visible line. NPCs before the player are placed behind the start.")]
    public bool playerStartsAtLineBeginning = true;

    [Tooltip("Optional waypoint where the player should start. Overrides Start Offset if assigned.")]
    public Transform playerStartWaypoint;

    [Tooltip("If true, slots placed before distance 0 are kept inactive until they enter the path.")]
    public bool activateSlotsOnlyWhenOnPath = true;

    [Tooltip("Maximum allowed player index when autoClampPlayerIndex is enabled. Prevents spawning too many hidden behind-slots.")]
    public int maxPlayerIndex = 100;

    [Header("Player End")]
    [Tooltip("What happens when the player slot reaches the end of the belt.")]
    public PlayerEndBehaviour playerEndBehaviour = PlayerEndBehaviour.HideAndDisable;

    [Header("References")]
    [Tooltip("Prefab used for NPC Homunculi.")]
    public GameObject npcPrefab;

    [Tooltip("Player controller reference. If empty, LineManager will try to find it.")]
    public PlayerController playerController;

    [Tooltip("Ghost slot reference. Usually the GhostSlot component on GhostTarget.")]
    public GhostSlot ghostSlot;

    [Tooltip("Optional parent container for spawned NPCs. If empty, one will be created.")]
    public Transform npcContainer;

    [Header("Placement")]
    [Tooltip("If true, the player GameObject is placed at its slot position at start.")]
    public bool placePlayerAtStart = true;

    [Tooltip("If true, spawned NPCs and the ghost slot rotate to follow the path direction.")]
    public bool rotateEntitiesWithPath = false;

    [Tooltip("Additional rotation offset if your sprite faces a different direction.")]
    public float rotationOffset = 0f;

    [Header("Events")]
    public UnityEvent onPlayerSlotReachedEnd;

    [Header("End Of Line Validation")]
    [Tooltip("Max distance between the player and their ghost slot to count as 'present' at the end. If 0, uses snapRadius.")]
    public float endPresenceRadius = 1.5f;

    [Header("Debug Read Only")]
    [SerializeField] private float totalLength;
    [SerializeField] private int slotCount;
    [SerializeField] private int actualPlayerIndex;
    [SerializeField] private float runtimeStartOffset;
    [SerializeField] private float beltTravel;
    [SerializeField] private float playerSlotDistance;
    [SerializeField] private float previousPlayerSlotDistance;
    [SerializeField] private bool playerSlotReachedEnd;

    private class LineSlotInstance
    {
        public int index;
        public float initialDistance;
        public bool isPlayer;
        public bool isActive = true;
        public bool pendingActivation = false;

        public GameObject gameObject;
        public Transform transform;
        public Rigidbody2D rb;
        public LineEntity entity;
    }

    private readonly List<Vector2> pathPoints = new List<Vector2>();
    private readonly List<Transform> pathTransforms = new List<Transform>();
    private readonly List<float> cumulativeDistances = new List<float>();
    private readonly List<LineSlotInstance> lineSlots = new List<LineSlotInstance>();

    private LineSlotInstance playerSlot;

    private bool initialized;
    private bool initializationFailed;

    private void Start()
    {
        Initialize();
    }

    private void FixedUpdate()
    {
        if (!initialized)
        {
            Initialize();

            if (!initialized)
                return;
        }

        AdvanceLine();
    }

    public void Initialize()
    {
        if (initialized || initializationFailed)
            return;

        if (!BuildPath())
        {
            initializationFailed = true;
            return;
        }

        if (!CalculateSlots())
        {
            initializationFailed = true;
            return;
        }

        FindReferences();
        CreateDefaultContainer();

        if (!ValidateReferences())
        {
            initializationFailed = true;
            return;
        }

        PrepareGhostSlot();
        SpawnLine();

        initialized = true;
    }

    private bool BuildPath()
    {
        pathPoints.Clear();
        pathTransforms.Clear();
        cumulativeDistances.Clear();
        totalLength = 0f;

        if (pathParent != null)
        {
            for (int i = 0; i < pathParent.childCount; i++)
            {
                Transform child = pathParent.GetChild(i);

                if (child == null)
                    continue;

                if (!includeInactivePathPoints && !child.gameObject.activeSelf)
                    continue;

                pathPoints.Add(child.position);
                pathTransforms.Add(child);
            }
        }
        else if (fallbackWaypoints != null && fallbackWaypoints.Length > 0)
        {
            foreach (Transform waypoint in fallbackWaypoints)
            {
                if (waypoint == null)
                    continue;

                if (!includeInactivePathPoints && !waypoint.gameObject.activeSelf)
                    continue;

                pathPoints.Add(waypoint.position);
                pathTransforms.Add(waypoint);
            }
        }

        if (pathPoints.Count < 2)
        {
            Debug.LogError("[LineManager] Path needs at least two points. Assign Path Parent or Fallback Waypoints.");
            return false;
        }

        cumulativeDistances.Add(0f);

        for (int i = 1; i < pathPoints.Count; i++)
        {
            float segmentLength = Vector2.Distance(pathPoints[i - 1], pathPoints[i]);
            totalLength += segmentLength;
            cumulativeDistances.Add(totalLength);
        }

        if (totalLength <= 0.0001f)
        {
            Debug.LogError("[LineManager] Path length is zero. Waypoints are likely all at the same position.");
            return false;
        }

        return true;
    }

    private float GetEffectiveStartOffset()
    {
        if (playerStartsAtLineBeginning && playerStartWaypoint != null)
        {
            for (int i = 0; i < pathTransforms.Count; i++)
            {
                if (pathTransforms[i] == playerStartWaypoint)
                    return cumulativeDistances[i];
            }

            Debug.LogWarning("[LineManager] Player Start Waypoint is not one of the path waypoints. Using Start Offset instead.");
        }

        return Mathf.Max(0f, startOffset);
    }

    private bool CalculateSlots()
    {
        if (spacing <= 0f)
        {
            Debug.LogError("[LineManager] Spacing must be greater than zero.");
            return false;
        }

        runtimeStartOffset = GetEffectiveStartOffset();

        float clampedEndMargin = Mathf.Max(0f, endDespawnMargin);

        if (playerStartsAtLineBeginning)
        {
            float lengthAhead = totalLength - runtimeStartOffset - clampedEndMargin;
            lengthAhead = Mathf.Max(0f, lengthAhead);

            int aheadSlots = Mathf.FloorToInt(lengthAhead / spacing);

            if (aheadSlots < 1)
            {
                Debug.LogWarning("[LineManager] There is not enough path length ahead of the player for an NPC. At least one ahead-slot will be forced, but it may despawn immediately.");
                aheadSlots = 1;
            }

            int minPlayerIndex = 1;
            int maxIndex = Mathf.Max(minPlayerIndex, maxPlayerIndex);

            actualPlayerIndex = playerStartIndex;

            if (autoClampPlayerIndex)
            {
                if (actualPlayerIndex < minPlayerIndex || actualPlayerIndex > maxIndex)
                {
                    int clampedIndex = Mathf.Clamp(actualPlayerIndex, minPlayerIndex, maxIndex);

                    Debug.LogWarning(
                        $"[LineManager] Player index {actualPlayerIndex} is outside the configured range. " +
                        $"Clamping to {clampedIndex}."
                    );

                    actualPlayerIndex = clampedIndex;
                }
            }
            else
            {
                if (actualPlayerIndex < minPlayerIndex)
                {
                    Debug.LogError("[LineManager] Player index must be at least 1 if you want NPCs behind the player.");
                    return false;
                }

                if (actualPlayerIndex > maxIndex)
                {
                    Debug.LogWarning(
                        $"[LineManager] Player index {actualPlayerIndex} is higher than Max Player Index {maxIndex}. " +
                        "This may spawn many hidden behind-slots."
                    );
                }
            }

            slotCount = actualPlayerIndex + 1 + aheadSlots;
        }
        else
        {
            float usableLength = totalLength - runtimeStartOffset - clampedEndMargin;
            usableLength = Mathf.Max(0f, usableLength);

            slotCount = Mathf.FloorToInt(usableLength / spacing) + 1;
            slotCount = Mathf.Max(1, slotCount);

            if (slotCount < 3)
            {
                Debug.LogWarning("[LineManager] Not enough slots to place NPCs before and after the player. Increase path length or reduce spacing.");
            }

            int minPlayerIndex = slotCount >= 3 ? 1 : 0;
            int maxPlayerIndexForSlots = slotCount >= 3 ? slotCount - 2 : slotCount - 1;

            actualPlayerIndex = playerStartIndex;

            if (autoClampPlayerIndex)
            {
                if (actualPlayerIndex < minPlayerIndex || actualPlayerIndex > maxPlayerIndexForSlots)
                {
                    int clampedIndex = Mathf.Clamp(actualPlayerIndex, minPlayerIndex, maxPlayerIndexForSlots);

                    Debug.LogWarning(
                        $"[LineManager] Player index {actualPlayerIndex} is invalid for {slotCount} slots. " +
                        $"Clamping to {clampedIndex}."
                    );

                    actualPlayerIndex = clampedIndex;
                }
            }
            else
            {
                if (actualPlayerIndex < minPlayerIndex || actualPlayerIndex > maxPlayerIndexForSlots)
                {
                    Debug.LogError(
                        $"[LineManager] Player index {actualPlayerIndex} is invalid for {slotCount} slots. " +
                        $"Valid range is {minPlayerIndex} to {maxPlayerIndexForSlots}."
                    );

                    return false;
                }
            }
        }

        return true;
    }

    private void FindReferences()
    {
        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();

        if (ghostSlot == null && playerController != null)
            ghostSlot = playerController.ghostSlot;

        if (ghostSlot == null)
            ghostSlot = Object.FindFirstObjectByType<GhostSlot>();
    }

    private void CreateDefaultContainer()
    {
        if (npcContainer != null)
            return;

        GameObject container = new GameObject("NPC_Line");
        container.transform.SetParent(transform, false);
        npcContainer = container.transform;
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (npcPrefab == null)
        {
            Debug.LogError("[LineManager] NPC Prefab is missing.");
            valid = false;
        }

        if (playerController == null)
        {
            Debug.LogError("[LineManager] PlayerController reference is missing.");
            valid = false;
        }

        if (ghostSlot == null)
        {
            Debug.LogError("[LineManager] GhostSlot reference is missing.");
            valid = false;
        }

        return valid;
    }

    private void PrepareGhostSlot()
    {
        if (ghostSlot == null)
            return;

        ghostSlot.SetLineDriven(true);

        if (playerController != null && playerController.ghostSlot != ghostSlot)
            playerController.ghostSlot = ghostSlot;
    }

    private void SpawnLine()
    {
        lineSlots.Clear();
        playerSlot = null;

        beltTravel = 0f;
        playerSlotReachedEnd = false;

        float clampedEndMargin = Mathf.Max(0f, endDespawnMargin);
        float despawnDistance = totalLength - clampedEndMargin;

        for (int i = 0; i < slotCount; i++)
        {
            float initialDistance;

            if (playerStartsAtLineBeginning)
            {
                initialDistance = runtimeStartOffset + (i - actualPlayerIndex) * spacing;
            }
            else
            {
                initialDistance = runtimeStartOffset + i * spacing;
            }

            if (initialDistance > despawnDistance)
                initialDistance = despawnDistance;

            SamplePath(initialDistance, out Vector2 position, out Vector2 direction);

            if (i == actualPlayerIndex)
            {
                CreatePlayerSlot(i, initialDistance, position, direction);
            }
            else
            {
                CreateNpcSlot(i, initialDistance, position, direction);
            }
        }

        previousPlayerSlotDistance = playerSlotDistance;
    }

    private void CreatePlayerSlot(int index, float initialDistance, Vector2 position, Vector2 direction)
    {
        SetupPlayerEntity(index);

        if (placePlayerAtStart && playerController != null)
        {
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            PlaceRigidbodyOrTransform(playerRb, playerController.transform, position);
        }

        if (ghostSlot != null)
        {
            ghostSlot.SetState(position, direction * GetSpeed());
        }

        playerSlot = new LineSlotInstance
        {
            index = index,
            initialDistance = initialDistance,
            isPlayer = true,
            isActive = true,
            pendingActivation = false,
            gameObject = ghostSlot != null ? ghostSlot.gameObject : null,
            transform = ghostSlot != null ? ghostSlot.transform : null,
            rb = null,
            entity = playerController != null ? playerController.GetComponent<LineEntity>() : null
        };

        playerSlotDistance = initialDistance;
        lineSlots.Add(playerSlot);
    }

    private void CreateNpcSlot(int index, float initialDistance, Vector2 position, Vector2 direction)
    {
        GameObject npc = Instantiate(npcPrefab, npcContainer);
        npc.name = $"NPC_{index}";

        Rigidbody2D rb = npc.GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = npc.AddComponent<Rigidbody2D>();

        ConfigureNpcRigidbody(rb);

        Collider2D[] colliders = npc.GetComponentsInChildren<Collider2D>();

        if (colliders != null && colliders.Length > 0)
        {
            foreach (Collider2D col in colliders)
            {
                if (col != null)
                    col.isTrigger = false;
            }
        }
        else
        {
            Debug.LogWarning($"[LineManager] NPC {index} has no Collider2D. It will not block the player.");
        }

        LineEntity entity = npc.GetComponent<LineEntity>();

        if (entity == null)
            entity = npc.GetComponentInChildren<LineEntity>();

        if (entity == null)
            entity = npc.AddComponent<LineEntity>();

        entity.Initialize(index, LineEntity.EntityType.NPC);

        bool startsBeforePath = initialDistance < 0f;
        bool pendingActivation = activateSlotsOnlyWhenOnPath && startsBeforePath;

        Vector2 placementPosition = position;
        Vector2 placementDirection = direction;

        // If this NPC is waiting to enter the path, keep it parked at the path start while inactive.
        // This prevents it from visually dashing in from an extrapolated off-path position.
        if (pendingActivation)
        {
            SamplePath(0f, out placementPosition, out placementDirection);
        }

        PlaceRigidbodyOrTransform(rb, npc.transform, placementPosition);

        if (rotateEntitiesWithPath)
        {
            float angle = GetAngle(placementDirection);
            rb.rotation = angle;
        }

        if (pendingActivation)
            npc.SetActive(false);

        LineSlotInstance slot = new LineSlotInstance
        {
            index = index,
            initialDistance = initialDistance,
            isPlayer = false,
            isActive = true,
            pendingActivation = pendingActivation,
            gameObject = npc,
            transform = npc.transform,
            rb = rb,
            entity = entity
        };

        lineSlots.Add(slot);
    }

    private void SetupPlayerEntity(int index)
    {
        if (playerController == null)
            return;

        LineEntity entity = playerController.GetComponent<LineEntity>();

        if (entity == null)
            entity = playerController.gameObject.AddComponent<LineEntity>();

        entity.Initialize(index, LineEntity.EntityType.Player);
    }

    private void ConfigureNpcRigidbody(Rigidbody2D rb)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.simulated = true;

        rb.interpolation = useNpcInterpolation
            ? RigidbodyInterpolation2D.Interpolate
            : RigidbodyInterpolation2D.None;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
    }



    private void PlaceRigidbodyOrTransform(Rigidbody2D rb, Transform fallbackTransform, Vector2 position)
    {
        if (rb != null)
            rb.position = position;
        else if (fallbackTransform != null)
            fallbackTransform.position = position;
    }

    private void AdvanceLine()
    {
        float speed = GetSpeed();

        previousPlayerSlotDistance = playerSlotDistance;
        beltTravel += speed * Time.fixedDeltaTime;

        float clampedEndMargin = Mathf.Max(0f, endDespawnMargin);
        float despawnDistance = totalLength - clampedEndMargin;

        for (int i = 0; i < lineSlots.Count; i++)
        {
            LineSlotInstance slot = lineSlots[i];

            if (slot == null || !slot.isActive || slot.isPlayer)
                continue;

            float distance = slot.initialDistance + beltTravel;

            bool justActivated = false;

            if (slot.pendingActivation)
            {
                // If it somehow skipped past the despawn point while inactive, just remove it.
                if (distance >= despawnDistance)
                {
                    slot.isActive = false;

                    if (slot.gameObject != null)
                        slot.gameObject.SetActive(false);

                    continue;
                }

                if (distance >= 0f)
                {
                    slot.pendingActivation = false;
                    justActivated = true;

                    if (slot.gameObject != null)
                        slot.gameObject.SetActive(true);
                }
                else
                {
                    continue;
                }
            }

            if (distance >= despawnDistance)
            {
                slot.isActive = false;

                if (slot.gameObject != null)
                    slot.gameObject.SetActive(false);

                continue;
            }

            SamplePath(distance, out Vector2 position, out Vector2 direction);

            if (slot.rb != null)
            {
                if (justActivated)
                {
                    // Hard teleport on activation.
                    // This removes the interpolation dash from the old off-path position.
                    slot.rb.interpolation = RigidbodyInterpolation2D.None;
                    slot.rb.position = position;
                    slot.transform.position = position;

                    if (rotateEntitiesWithPath)
                        slot.rb.rotation = GetAngle(direction);

                    slot.rb.interpolation = useNpcInterpolation
                        ? RigidbodyInterpolation2D.Interpolate
                        : RigidbodyInterpolation2D.None;
                }
                else
                {
                    slot.rb.MovePosition(position);

                    if (rotateEntitiesWithPath)
                        slot.rb.MoveRotation(GetAngle(direction));
                }
            }
            else if (slot.transform != null)
            {
                slot.transform.position = position;

                if (rotateEntitiesWithPath)
                    slot.transform.rotation = Quaternion.Euler(0f, 0f, GetAngle(direction));
            }
        }

        if (playerSlot != null)
        {
            float distance = playerSlot.initialDistance + beltTravel;
            bool reachedEnd = distance >= despawnDistance;

            if (reachedEnd)
                distance = despawnDistance;

            SamplePath(distance, out Vector2 position, out Vector2 direction);

            Vector2 velocity = reachedEnd ? Vector2.zero : direction * speed;

            if (ghostSlot != null)
                ghostSlot.SetState(position, velocity);

            playerSlotDistance = distance;

            if (reachedEnd && !playerSlotReachedEnd)
            {
                playerSlotReachedEnd = true;

                if (IsPlayerPresentAtSlot())
                {
                    Debug.Log("[LineManager] Player present at the end of the line. Win.");
                    GameManager.Instance.TriggerWin();
                }
                else
                {
                    Debug.LogWarning("[LineManager] Player slot reached the end empty. Loss.");
                    GameManager.Instance.TriggerLoss(LossReason.SlotReachedEndEmpty);
                }
            }
        }
    }

    private void HandlePlayerEnd()
    {
        if (playerController == null)
            return;

        switch (playerEndBehaviour)
        {
            case PlayerEndBehaviour.Nothing:
                break;

            case PlayerEndBehaviour.DisableGameObject:
                playerController.gameObject.SetActive(false);
                break;

            case PlayerEndBehaviour.HideAndDisable:
            default:
                playerController.enabled = false;

                Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();

                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.simulated = false;
                }

                Collider2D[] colliders = playerController.GetComponentsInChildren<Collider2D>();

                if (colliders != null)
                {
                    foreach (Collider2D col in colliders)
                    {
                        if (col != null)
                            col.enabled = false;
                    }
                }

                SpriteRenderer[] sprites = playerController.GetComponentsInChildren<SpriteRenderer>();

                if (sprites != null)
                {
                    foreach (SpriteRenderer sr in sprites)
                    {
                        if (sr != null)
                            sr.enabled = false;
                    }
                }
                break;
        }
    }

    private float GetSpeed()
    {
        return Mathf.Max(0f, scrollSpeed);
    }

    private float GetAngle(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector2.right;

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
    }

    public int GetPlayerLineIndex()
    {
        return actualPlayerIndex;
    }

    public float GetPlayerSlotDistance()
    {
        return playerSlotDistance;
    }

    public float GetPreviousPlayerSlotDistance()
    {
        return previousPlayerSlotDistance;
    }

    public bool HasPlayerSlotCrossedDistance(float distance)
    {
        return initialized && previousPlayerSlotDistance < distance && playerSlotDistance >= distance;
    }

    public bool IsPlayerSlotAtEnd()
    {
        return playerSlotReachedEnd;
    }

    public float GetTotalLength()
    {
        return totalLength;
    }

    public void SamplePathDistance(float distance, out Vector2 position, out Vector2 direction)
    {
        SamplePath(distance, out position, out direction);
    }

    private void SamplePath(float distance, out Vector2 position, out Vector2 direction)
    {
        position = Vector2.zero;
        direction = Vector2.right;

        if (pathPoints.Count == 0)
            return;

        if (pathPoints.Count == 1)
        {
            position = pathPoints[0];
            return;
        }

        // Extrapolate backwards for slots that exist before the beginning of the path.
        if (distance < 0f)
        {
            if (GetFirstValidSegment(out Vector2 a, out Vector2 b))
            {
                direction = (b - a).normalized;

                if (direction.sqrMagnitude <= 0.0001f)
                    direction = Vector2.right;

                position = a + direction * distance;
            }
            else
            {
                position = pathPoints[0];
                direction = Vector2.right;
            }

            return;
        }

        distance = Mathf.Clamp(distance, 0f, totalLength);

        int segmentIndex = FindSegmentIndex(distance);

        Vector2 a2 = pathPoints[segmentIndex];
        Vector2 b2 = pathPoints[segmentIndex + 1];

        float segmentStart = cumulativeDistances[segmentIndex];
        float segmentLength = cumulativeDistances[segmentIndex + 1] - segmentStart;

        Vector2 segmentDirection = b2 - a2;

        if (segmentDirection.sqrMagnitude <= 0.0001f)
        {
            // If this segment is zero-length, try to find the next valid segment.
            for (int i = segmentIndex + 1; i < pathPoints.Count - 1; i++)
            {
                Vector2 nextDirection = pathPoints[i + 1] - pathPoints[i];

                if (nextDirection.sqrMagnitude > 0.0001f)
                {
                    segmentIndex = i;
                    a2 = pathPoints[i];
                    b2 = pathPoints[i + 1];
                    segmentStart = cumulativeDistances[i];
                    segmentLength = cumulativeDistances[i + 1] - segmentStart;
                    segmentDirection = nextDirection;
                    break;
                }
            }
        }

        direction = segmentDirection.normalized;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector2.right;

        float t = segmentLength > 0f
            ? Mathf.Clamp01((distance - segmentStart) / segmentLength)
            : 0f;

        position = Vector2.Lerp(a2, b2, t);
    }

    private bool GetFirstValidSegment(out Vector2 a, out Vector2 b)
    {
        a = Vector2.zero;
        b = Vector2.zero;

        if (pathPoints.Count < 2)
            return false;

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector2 dir = pathPoints[i + 1] - pathPoints[i];

            if (dir.sqrMagnitude > 0.0001f)
            {
                a = pathPoints[i];
                b = pathPoints[i + 1];
                return true;
            }
        }

        a = pathPoints[0];
        b = pathPoints[0] + Vector2.right;
        return true;
    }

    private int FindSegmentIndex(float distance)
    {
        int low = 0;
        int high = cumulativeDistances.Count - 1;

        while (low < high - 1)
        {
            int mid = (low + high) / 2;

            if (cumulativeDistances[mid] <= distance)
                low = mid;
            else
                high = mid;
        }

        return Mathf.Clamp(low, 0, cumulativeDistances.Count - 2);
    }

    private bool IsPlayerPresentAtSlot()
    {
        if (playerController == null)
            return false;

        if (!playerController.IsOnBelt)
            return false;

        if (playerController.ghostSlot == null)
            return false;

        float radius = endPresenceRadius > 0f ? endPresenceRadius : playerController.snapRadius;

        float distanceToSlot = Vector2.Distance(
            playerController.transform.position,
            playerController.ghostSlot.transform.position
        );

        return distanceToSlot <= radius;
    }

    private void OnValidate()
    {
        if (spacing < 0.01f)
            spacing = 0.01f;

        if (endDespawnMargin < 0f)
            endDespawnMargin = 0f;

        if (startOffset < 0f)
            startOffset = 0f;

        if (maxPlayerIndex < 1)
            maxPlayerIndex = 1;
    }

    private void OnDrawGizmosSelected()
    {
        if (pathParent == null && (fallbackWaypoints == null || fallbackWaypoints.Length == 0))
            return;

        List<Vector2> previewPoints = new List<Vector2>();

        if (pathParent != null)
        {
            for (int i = 0; i < pathParent.childCount; i++)
            {
                Transform child = pathParent.GetChild(i);

                if (child == null)
                    continue;

                if (!includeInactivePathPoints && !child.gameObject.activeSelf)
                    continue;

                previewPoints.Add(child.position);
            }
        }
        else
        {
            foreach (Transform waypoint in fallbackWaypoints)
            {
                if (waypoint == null)
                    continue;

                if (!includeInactivePathPoints && !waypoint.gameObject.activeSelf)
                    continue;

                previewPoints.Add(waypoint.position);
            }
        }

        if (previewPoints.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < previewPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(previewPoints[i], previewPoints[i + 1]);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(previewPoints[0], 0.15f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(previewPoints[previewPoints.Count - 1], 0.15f);
    }
} 