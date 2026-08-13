using System.Collections.Generic;
using UnityEngine;

public class SuspicionManager : MonoBehaviour
{
    public static SuspicionManager Instance;
    public static bool HasInstance => Instance != null;

    [Header("Settings")]
    [Tooltip("How fast suspicion drops per second when the player is safe")]
    public float decayRate = 0.8f;

    private readonly Dictionary<int, float> sources = new Dictionary<int, float>();
    private float displayedSuspicion;

    public float Suspicion => displayedSuspicion;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ReportSuspicion(int sourceId, float value)
    {
        float clamped = Mathf.Clamp01(value);
        sources[sourceId] = clamped;

        float max = 0f;
        foreach (float v in sources.Values)
            if (v > max) max = v;

        if (max > displayedSuspicion)
            displayedSuspicion = max;

        if (displayedSuspicion >= 1f)
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.TriggerLoss(LossReason.CameraCaught);
        }
    }

    public void RemoveSource(int sourceId)
    {
        sources.Remove(sourceId);
    }

    public void ResetSuspicion()
    {
        sources.Clear();
        displayedSuspicion = 0f;
    }

    private void Update()
    {
        float max = 0f;
        foreach (float v in sources.Values)
            if (v > max) max = v;

        if (max < displayedSuspicion)
            displayedSuspicion = Mathf.Max(max, displayedSuspicion - decayRate * Time.deltaTime);
    }
}