using UnityEngine;
using UnityEngine.Events;

public class ScannerCheckpoint : PlayerSlotCheckpoint
{
    public PlayerController playerController;
    public PlayerColorController playerColorController;

    [Header("Scanner Rules")]
    public bool requireOnBelt = true;
    public bool requireNearSlot = true;

    [Tooltip("Maximum distance allowed between the player and their ghost slot. If zero, PlayerController.snapRadius is used.")]
    public float presenceRadius = 1.5f;

    [Header("Events (optional, for VFX / audio)")]
    public UnityEvent onSuccess;
    public UnityEvent onFail;

    protected override void Start()
    {
        base.Start();

        if (playerController == null)
            playerController = Object.FindAnyObjectByType<PlayerController>();

        if (playerColorController == null && playerController != null)
            playerColorController = playerController.GetComponent<PlayerColorController>();

        if (playerColorController == null)
            playerColorController = Object.FindAnyObjectByType<PlayerColorController>();

        if (presenceRadius <= 0f && playerController != null)
            presenceRadius = playerController.snapRadius;
    }

    protected override void OnPlayerSlotCrossed()
    {
        Debug.Log($"[ScannerCheckpoint] Player slot crossed scanner at distance {checkpointDistance}.");

        if (PatternManager.Instance == null)
        {
            Debug.LogWarning("[ScannerCheckpoint] PatternManager missing. Scan ignored.");
            return;
        }

        // Do not kill the player before the pattern has been revealed.
        if (!PatternManager.Instance.IsPatternRevealed || !PatternManager.Instance.HasActivePattern())
        {
            Debug.Log("[ScannerCheckpoint] Pattern not revealed yet. Scan ignored.");
            return;
        }

        if (playerController == null || playerColorController == null)
        {
            Fail(LossReason.External, "Player references missing.");
            return;
        }

        // 1. Presence check -> Empty Slot
        if (requireOnBelt && !playerController.IsOnBelt)
        {
            Fail(LossReason.ScannerEmptySlot, "Player is off the conveyor belt.");
            return;
        }

        if (requireNearSlot)
        {
            if (playerController.ghostSlot == null)
            {
                Fail(LossReason.ScannerEmptySlot, "Ghost slot missing.");
                return;
            }

            float distanceToSlot = Vector2.Distance(
                playerController.transform.position,
                playerController.ghostSlot.transform.position
            );

            if (distanceToSlot > presenceRadius)
            {
                Fail(LossReason.ScannerEmptySlot, $"Player too far from slot ({distanceToSlot:F2} > {presenceRadius}).");
                return;
            }
        }

        // 2. Color check -> Mismatch
        int lineIndex = lineManager != null
            ? lineManager.GetPlayerLineIndex()
            : GetLineIndexFromEntity();

        if (lineIndex < 0)
        {
            Fail(LossReason.ScannerEmptySlot, "Invalid player line index.");
            return;
        }

        ColorId expected = PatternManager.Instance.GetExpectedColor(lineIndex);
        ColorId current = playerColorController.CurrentColor;

        if (expected == current)
        {
            Pass($"Scanner passed. Expected {expected}, current {current}.");
        }
        else
        {
            Fail(LossReason.ScannerColorMismatch, $"Scanner failed. Expected {expected}, current {current}.");
        }
    }

    private int GetLineIndexFromEntity()
    {
        if (playerController == null)
            return -1;

        LineEntity entity = playerController.GetComponent<LineEntity>();
        return entity != null ? entity.lineIndex : -1;
    }

    private void Pass(string message)
    {
        Debug.Log($"[ScannerCheckpoint] PASS: {message}");

        if (onSuccess != null)
            onSuccess.Invoke();
    }

    private void Fail(LossReason reason, string message)
    {
        Debug.LogWarning($"[ScannerCheckpoint] FAIL: {message}");

        if (onFail != null)
            onFail.Invoke();

        GameManager.Instance.TriggerLoss(reason);
    }
}