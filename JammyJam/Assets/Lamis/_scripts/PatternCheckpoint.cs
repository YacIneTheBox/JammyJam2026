using UnityEngine;

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

    protected override void OnPlayerSlotCrossed()
    {
        if (PatternManager.Instance == null)
        {
            Debug.LogWarning("[PatternCheckpoint] PatternManager.Instance is missing.");
            return;
        }

        switch (action)
        {
            case PatternCheckpointAction.RevealCurrentPattern:
                PatternManager.Instance.RevealCurrentPattern();
                Debug.Log($"[PatternCheckpoint] Pattern revealed at distance {checkpointDistance}.");
                break;

            case PatternCheckpointAction.AdvanceToNextPattern:
                PatternManager.Instance.AdvanceAndRevealPattern();
                Debug.Log($"[PatternCheckpoint] Pattern advanced to index {PatternManager.Instance.CurrentPatternIndex} at distance {checkpointDistance}.");
                break;

            case PatternCheckpointAction.SetSpecificPattern:
                PatternManager.Instance.SetPatternIndex(specificPatternIndex);
                Debug.Log($"[PatternCheckpoint] Pattern set to index {specificPatternIndex} at distance {checkpointDistance}.");
                break;
        }
    }
}