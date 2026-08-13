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
public class ColorTintSet
{
    public Color defaultColor = Color.white;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public Color blueColor = Color.blue;
    public Color brownColor = new Color(0.55f, 0.35f, 0.15f, 1f);

    public Color GetColor(ColorId colorId)
    {
        switch (colorId)
        {
            case ColorId.Default:
                return defaultColor;

            case ColorId.Red:
                return redColor;

            case ColorId.Green:
                return greenColor;

            case ColorId.Blue:
                return blueColor;

            case ColorId.Brown:
                return brownColor;

            default:
                return defaultColor;
        }
    }
}

[Serializable]
public class ColorPattern
{
    public string patternName = "Pattern";
    public List<ColorId> colors = new List<ColorId>();
}