using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Attache Outline.cs from QuickOutline to given object to highlight it
///     Or generate highlight location mark to highlight a location
/// </summary>
public static class HighlightUtils
{
    public static void HighlightObject(GameObject gameObject, Color? color = null)
    {
        // Try to get outline
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        // Highlight setting
        outline.enabled = true;
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineWidth = 10;
        outline.OutlineColor = color ?? Color.blue;
    }

    public static void UnhighlightObject(GameObject gameObject)
    {
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    // Resume a previously disabled highlight
    public static void ResumeObjectHighlight(GameObject gameObject)
    {
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = true;
    }

    // Get the highlighted color. Return Color.clear if not highlighted
    public static Color GetHighlightColor(GameObject gameObject)
    {
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null || outline.enabled == false)
            return (Color.clear);
        else
            return (outline.OutlineColor);
    }
}
