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

    [Header("Events")]
    public UnityEvent onSuccess;
    public UnityEvent onFail;

    protected override void Start()
    {
        base.Start();

        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();

        if (playerColorController == null && playerController != null)
            playerColorController = playerController.GetComponent<PlayerColorController>();

        if (playerColorController == null)
            playerColorController = Object.FindFirstObjectByType<PlayerColorController>();

        if (presenceRadius <= 0f && playerController != null)
            presenceRadius = playerController.snapRadius;
    }

    protected override void OnPlayerSlotCrossed()
    {
        if (PatternManager.Instance == null)
        {
            Fail("PatternManager is missing.");
            return;
        }

        if (!PatternManager.Instance.IsPatternRevealed)
        {
            Fail("Pattern has not been revealed yet.");
            return;
        }

        if (!PatternManager.Instance.HasActivePattern())
        {
            Fail("No active pattern is configured.");
            return;
        }

        if (playerController == null)
        {
            Fail("PlayerController is missing.");
            return;
        }

        if (playerColorController == null)
        {
            Fail("PlayerColorController is missing.");
            return;
        }

        if (requireOnBelt && !playerController.IsOnBelt)
        {
            Fail("Player is off the conveyor belt.");
            return;
        }

        if (requireNearSlot)
        {
            if (playerController.ghostSlot == null)
            {
                Fail("Player ghost slot is missing.");
                return;
            }

            float distanceToSlot = Vector2.Distance(
                playerController.transform.position,
                playerController.ghostSlot.transform.position
            );

            if (distanceToSlot > presenceRadius)
            {
                Fail($"Player is too far from their slot. Distance: {distanceToSlot}. Radius: {presenceRadius}.");
                return;
            }
        }

        int lineIndex;

        if (lineManager != null)
        {
            lineIndex = lineManager.GetPlayerLineIndex();
        }
        else
        {
            LineEntity playerEntity = playerController.GetComponent<LineEntity>();
            lineIndex = playerEntity != null ? playerEntity.lineIndex : -1;
        }

        if (lineIndex < 0)
        {
            Fail("Player line index is invalid.");
            return;
        }

        ColorId expectedColor = PatternManager.Instance.GetExpectedColor(lineIndex);
        ColorId currentColor = playerColorController.CurrentColor;

        if (expectedColor == currentColor)
        {
            Pass($"Scanner passed. Expected: {expectedColor}, Current: {currentColor}.");
        }
        else
        {
            Fail($"Scanner failed. Expected: {expectedColor}, Current: {currentColor}.");
        }
    }

    private void Pass(string message)
    {
        Debug.Log($"[ScannerCheckpoint] {message}");

        if (onSuccess != null)
            onSuccess.Invoke();
    }

    private void Fail(string reason)
    {
        Debug.LogWarning($"[ScannerCheckpoint] {reason}");

        if (onFail != null)
            onFail.Invoke();
    }
}
