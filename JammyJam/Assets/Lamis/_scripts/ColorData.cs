using System;
using System.Collections.Generic;
using UnityEngine;

public enum ColorId
{
    Default,
    Red,
    Green,
    Blue,
    Brown
}

[Serializable]
public class ColorSpriteSet
{
    public Sprite defaultSprite;
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite brownSprite;

    public Sprite GetSprite(ColorId colorId)
    {
        switch (colorId)
        {
            case ColorId.Default:
                return defaultSprite;

            case ColorId.Red:
                return redSprite;

            case ColorId.Green:
                return greenSprite;

            case ColorId.Blue:
                return blueSprite;

            case ColorId.Brown:
                return brownSprite;

            default:
                return defaultSprite;
        }
    }
}

[Serializable]
public class ColorPattern
{
    public string patternName = "Pattern";
    public List<ColorId> colors = new List<ColorId>();
}