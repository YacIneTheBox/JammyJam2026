using UnityEngine;
using System.Collections;

public class PatternCheckpoint : PlayerSlotCheckpoint
{
    public enum PatternCheckpointAction
    {
        RevealCurrentPattern,
        AdvanceToNextPattern,
        SetSpecificPattern
    }

    public PatternCheckpointAction action = PatternCheckpointAction.RevealCurrentPattern;

    [Tooltip("Used only if Action is SetSpecificPattern.")]
    public int specificPatternIndex = 0;

    [Header("Alert")]
    [Tooltip("Seconds between the alert and the actual color change.")]
    public float revealDelay = 2f;

    [Tooltip("If true, logs the player's new expected color after the change.")]
    public bool logExpectedPlayerColor = true;

    private Coroutine revealCoroutine;

    protected override void OnPlayerSlotCrossed()
    {
        if (PatternManager.Instance == null)
        {
            Debug.LogWarning("[PatternCheckpoint] PatternManager.Instance is missing.");
            return;
        }

        // 1) Warn the player NOW
        PatternManager.Instance.TriggerPatternAlert();
        Debug.Log($"[PatternCheckpoint] Alert! Pattern changes in {revealDelay} seconds.");

        // 2) Change the colors AFTER the delay
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealAfterDelay());
    }

    private IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(revealDelay);

        switch (action)
        {
            case PatternCheckpointAction.RevealCurrentPattern:
                PatternManager.Instance.RevealCurrentPattern();
                Debug.Log($"[PatternCheckpoint] Pattern revealed. Index: {PatternManager.Instance.CurrentPatternIndex}.");
                break;

            case PatternCheckpointAction.AdvanceToNextPattern:
                PatternManager.Instance.AdvanceAndRevealPattern();
                Debug.Log($"[PatternCheckpoint] Pattern advanced to index {PatternManager.Instance.CurrentPatternIndex}.");
                break;

            case PatternCheckpointAction.SetSpecificPattern:
                PatternManager.Instance.SetPatternIndex(specificPatternIndex);
                Debug.Log($"[PatternCheckpoint] Pattern set to index {PatternManager.Instance.CurrentPatternIndex}.");
                break;
        }

        LogExpectedPlayerColor();

        revealCoroutine = null;
    }

    private void LogExpectedPlayerColor()
    {
        if (!logExpectedPlayerColor)
            return;

        if (PatternManager.Instance == null || lineManager == null)
            return;

        int playerIndex = lineManager.GetPlayerLineIndex();
        ColorId expectedColor = PatternManager.Instance.GetExpectedColor(playerIndex);

        Debug.Log($"[PatternCheckpoint] Player index {playerIndex} expected color is now {expectedColor}.");
    }
}