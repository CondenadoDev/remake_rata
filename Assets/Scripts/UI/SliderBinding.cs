using System;
using UISystem.Binding;
using UnityEngine;
using UnityEngine.UI;

public class SliderBinding : DataBinding
{
    private Slider slider;
        
    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider != null && bindingMode != BindingMode.OneWay)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (updateTrigger == UpdateTrigger.OnValueChanged)
        {
            UpdateSource();
        }
    }

    public override void UpdateSource()
    {
        if (bindingMode == BindingMode.OneWay || slider == null) return;
        SetSourceValue(slider.value);
    }

    public override void UpdateTarget()
    {
        if (slider == null) return;
            
        var value = GetSourceValue();
        if (value != null)
        {
            slider.value = Convert.ToSingle(value);
        }
    }
}