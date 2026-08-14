using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PatternManager : MonoBehaviour
{
    public static PatternManager Instance { get; private set; }

    [Header("Designer Patterns")]
    public List<ColorPattern> patterns = new List<ColorPattern>();

    [Tooltip("Pattern used at startup if Randomize Start Pattern is false.")]
    public int startingPatternIndex = 0;

    // --- NOUVELLE OPTION POUR L'ALÉATOIRE ---
    [Tooltip("If true, a random pattern will be chosen at startup.")]
    public bool randomizeStartPattern = true; 

    [Tooltip("For testing only. In normal gameplay this should be false, because a Pattern Checkpoint reveals the pattern.")]
    public bool startRevealed = false;

    public event Action OnPatternStateChanged;

    public event Action OnPatternAlert;

    public void TriggerPatternAlert()
    {
        if (OnPatternAlert != null)
            OnPatternAlert.Invoke();
    }

    [Header("Debug Read Only")]
    [SerializeField] private int currentPatternIndex;
    [SerializeField] private bool patternRevealed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (patterns != null && patterns.Count > 0)
        {
            // --- C'EST ICI QUE LA MAGIE OPÈRE ---
            if (randomizeStartPattern)
            {
                // Sélectionne un index aléatoire entre 0 (inclus) et patterns.Count (exclu)
                currentPatternIndex = UnityEngine.Random.Range(0, patterns.Count);
            }
            else
            {
                currentPatternIndex = Mathf.Clamp(startingPatternIndex, 0, patterns.Count - 1);
            }
        }
        else
        {
            currentPatternIndex = 0;
        }

        patternRevealed = startRevealed;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsPatternRevealed => patternRevealed;

    public int CurrentPatternIndex => currentPatternIndex;

    public ColorPattern ActivePattern
    {
        get
        {
            if (patterns == null || patterns.Count == 0)
                return null;

            int index = Mathf.Clamp(currentPatternIndex, 0, patterns.Count - 1);
            return patterns[index];
        }
    }

    public bool HasActivePattern()
    {
        ColorPattern pattern = ActivePattern;
        return pattern != null && pattern.colors != null && pattern.colors.Count > 0;
    }

    public void RevealCurrentPattern()
    {
        patternRevealed = true;
        InvokePatternStateChanged();
    }

    public void AdvanceAndRevealPattern()
    {
        if (patterns != null && patterns.Count > 0)
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Count;

        patternRevealed = true;
        InvokePatternStateChanged();
    }

    public void SetPatternIndex(int index)
    {
        if (patterns == null || patterns.Count == 0)
            return;

        currentPatternIndex = Mathf.Clamp(index, 0, patterns.Count - 1);
        patternRevealed = true;

        InvokePatternStateChanged();
    }

    public ColorId GetExpectedColor(int lineIndex)
    {
        if (!patternRevealed || lineIndex < 0)
            return ColorId.Default;

        ColorPattern pattern = ActivePattern;

        if (pattern == null || pattern.colors == null || pattern.colors.Count == 0)
            return ColorId.Default;

        int index = lineIndex % pattern.colors.Count;

        if (index < 0)
            index += pattern.colors.Count;

        return pattern.colors[index];
    }

    private void InvokePatternStateChanged()
    {
        if (OnPatternStateChanged != null)
            OnPatternStateChanged.Invoke();
    }
}