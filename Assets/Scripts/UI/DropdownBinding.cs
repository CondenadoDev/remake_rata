using System;
using TMPro;
using UnityEngine.UI;
using UISystem.Binding;

public class DropdownBinding : DataBinding
{
    private TMP_Dropdown dropdown;
    private Dropdown legacyDropdown;
        
    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        legacyDropdown = GetComponent<Dropdown>();
            
        if (dropdown != null && bindingMode != BindingMode.OneWay)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        else if (legacyDropdown != null && bindingMode != BindingMode.OneWay)
        {
            legacyDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    private void OnDropdownValueChanged(int value)
    {
        if (updateTrigger == UpdateTrigger.OnValueChanged)
        {
            UpdateSource();
        }
    }

    public override void UpdateSource()
    {
        if (bindingMode == BindingMode.OneWay) return;
            
        int value = dropdown != null ? dropdown.value : legacyDropdown.value;
        SetSourceValue(value);
    }

    public override void UpdateTarget()
    {
        var value = GetSourceValue();
        if (value == null) return;
            
        int index = Convert.ToInt32(value);
        if (dropdown != null)
            dropdown.value = index;
        else if (legacyDropdown != null)
            legacyDropdown.value = index;
    }
}