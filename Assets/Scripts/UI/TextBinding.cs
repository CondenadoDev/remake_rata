using System;
using TMPro;
using UISystem.Binding;
using UnityEngine;
using UnityEngine.UI;

 public class TextBinding : DataBinding
 {
     private TextMeshProUGUI textComponent;
     private InputField inputField;
     private TMP_InputField tmpInputField;

     private void Awake()
     {
         textComponent = GetComponent<TextMeshProUGUI>();
         inputField = GetComponent<InputField>();
         tmpInputField = GetComponent<TMP_InputField>();

         if (tmpInputField != null && bindingMode == BindingMode.TwoWay)
         {
             tmpInputField.onValueChanged.AddListener(OnInputValueChanged);
         }
         else if (inputField != null && bindingMode == BindingMode.TwoWay)
         {
             inputField.onValueChanged.AddListener(OnInputValueChanged);
         }
     }

     private void OnInputValueChanged(string value)
     {
         if (updateTrigger == UpdateTrigger.OnValueChanged)
         {
             UpdateSource();
         }
     }

     public override void UpdateSource()
     {
         if (bindingMode == BindingMode.OneWay) return;

         string value = null;
         if (tmpInputField != null)
             value = tmpInputField.text;
         else if (inputField != null)
             value = inputField.text;

         if (value != null)
         {
             SetSourceValue(ConvertValue(value, propertyInfo.PropertyType));
         }
     }

     public override void UpdateTarget()
     {
         var value = GetSourceValue();
         if (value == null) return;

         string text = value.ToString();

         if (textComponent != null)
             textComponent.text = text;
         if (tmpInputField != null)
             tmpInputField.text = text;
         if (inputField != null)
             inputField.text = text;
     }

     private object ConvertValue(string value, Type targetType)
     {
         if (targetType == typeof(string))
             return value;
         if (targetType == typeof(int))
             return int.Parse(value);
         if (targetType == typeof(float))
             return float.Parse(value);
         if (targetType == typeof(bool))
             return bool.Parse(value);

         return value;
     }
 }