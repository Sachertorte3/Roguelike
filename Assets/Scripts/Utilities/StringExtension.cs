using UnityEngine;

public static class StringExtension
{
    public static string SetColored(this string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }
}