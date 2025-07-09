using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UIOptionAttribute : Attribute
{
    public string displayName;
    public UIControlType controlType;
    public float minValue;
    public float maxValue;
    public string[] options;
    public string category;
    public string tooltip;
    public int order;

    // Para Sliders
    public UIOptionAttribute(string displayName, UIControlType controlType, float minValue, float maxValue, string category = "", int order = 0)
    {
        this.displayName = displayName;
        this.controlType = controlType;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.category = category;
        this.order = order;
    }

    // Para Toggles y Labels
    public UIOptionAttribute(string displayName, UIControlType controlType, string category = "", int order = 0)
    {
        this.displayName = displayName;
        this.controlType = controlType;
        this.category = category;
        this.order = order;
    }

    // Para Dropdowns
    public UIOptionAttribute(string displayName, string[] options, string category = "", int order = 0)
    {
        this.displayName = displayName;
        this.controlType = UIControlType.Dropdown;
        this.options = options;
        this.category = category;
        this.order = order;
    }
}

public enum UIControlType
{
    Toggle,
    Slider,
    Dropdown,
    Label,
    Button
}