using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColorController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ColorSpriteSet sprites = new ColorSpriteSet();

    public ColorId CurrentColor { get; private set; } = ColorId.Default;

    public event System.Action<ColorId> OnColorChanged;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ApplyColor(CurrentColor);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.eKey.wasPressedThisFrame)
            CycleColor();
    }

    public void CycleColor()
    {
        CurrentColor = GetNextColor(CurrentColor);
        ApplyColor(CurrentColor);

        if (OnColorChanged != null)
            OnColorChanged.Invoke(CurrentColor);
    }

    public void SetColor(ColorId colorId)
    {
        CurrentColor = colorId;
        ApplyColor(colorId);

        if (OnColorChanged != null)
            OnColorChanged.Invoke(CurrentColor);
    }

    private ColorId GetNextColor(ColorId colorId)
    {
        switch (colorId)
        {
            case ColorId.Default:
                return ColorId.Red;

            case ColorId.Red:
                return ColorId.Green;

            case ColorId.Green:
                return ColorId.Blue;

            case ColorId.Blue:
                return ColorId.Brown;

            case ColorId.Brown:
            default:
                return ColorId.Default;
        }
    }

    private void ApplyColor(ColorId colorId)
    {
        if (spriteRenderer == null)
            return;

        Sprite sprite = sprites.GetSprite(colorId);

        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }
}