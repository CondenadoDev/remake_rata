using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class AutoUIPanel
{
    public string panelName;
    public string configTypeName;
    public List<AutoUIControl> controls = new List<AutoUIControl>();
}

[System.Serializable]
public class AutoUIControl
{
    public string fieldName;
    public string displayName;
    public UIControlType controlType;
    public object configObject;
    public FieldInfo fieldInfo;
    public float minValue, maxValue;
    public string[] options;
    public int order;
}