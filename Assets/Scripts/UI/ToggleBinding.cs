using System;
using UISystem.Binding;
using UnityEngine;
using UnityEngine.UI;
public class ToggleBinding : DataBinding
{
    private Toggle toggle;
        
    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle != null && bindingMode != BindingMode.OneWay)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        if (updateTrigger == UpdateTrigger.OnValueChanged)
        {
            UpdateSource();
        }
    }

    public override void UpdateSource()
    {
        if (bindingMode == BindingMode.OneWay || toggle == null) return;
        SetSourceValue(toggle.isOn);
    }

    public override void UpdateTarget()
    {
        if (toggle == null) return;
            
        var value = GetSourceValue();
        if (value != null)
        {
            toggle.isOn = Convert.ToBoolean(value);
        }
    }
}