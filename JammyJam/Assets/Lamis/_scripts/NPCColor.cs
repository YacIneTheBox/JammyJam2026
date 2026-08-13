using UnityEngine;

[RequireComponent(typeof(LineEntity))]
public class NPCCOLOR : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer[] additionalSpriteRenderers;
    public ColorTintSet tints = new ColorTintSet();

    private LineEntity lineEntity;
    private PatternManager cachedPatternManager;

    private void Awake()
    {
        lineEntity = GetComponent<LineEntity>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (cachedPatternManager == PatternManager.Instance)
            return;

        Unsubscribe();

        cachedPatternManager = PatternManager.Instance;

        if (cachedPatternManager != null)
            cachedPatternManager.OnPatternStateChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (cachedPatternManager != null)
            cachedPatternManager.OnPatternStateChanged -= Refresh;

        cachedPatternManager = null;
    }

    public void Refresh()
    {
        if (lineEntity == null)
            lineEntity = GetComponent<LineEntity>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ColorId color = ColorId.Default;

        if (PatternManager.Instance != null && lineEntity != null)
            color = PatternManager.Instance.GetExpectedColor(lineEntity.lineIndex);

        ApplyColor(color);
    }

    public void ApplyColor(ColorId colorId)
    {
        Color color = tints.GetColor(colorId);

        if (spriteRenderer != null)
            spriteRenderer.color = color;

        if (additionalSpriteRenderers != null)
        {
            foreach (SpriteRenderer sr in additionalSpriteRenderers)
            {
                if (sr != null)
                    sr.color = color;
            }
        }
    }
}