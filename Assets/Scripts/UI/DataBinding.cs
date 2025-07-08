// DataBinding.cs
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystem.Core;
using UnityEngine.UIElements;

namespace UISystem.Binding
{
    public abstract class DataBinding : MonoBehaviour, IDataBinding
    {
        [SerializeField] public string propertyPath;
        [SerializeField] public BindingMode bindingMode = BindingMode.TwoWay;
        [SerializeField] protected UpdateTrigger updateTrigger = UpdateTrigger.OnValueChanged;

        protected object source;
        protected PropertyInfo propertyInfo;
        protected bool isUpdating;

        public event Action<object> OnValueChanged;

        public virtual void Bind(object source, string propertyPath)
        {
            Unbind();

            this.source = source;
            this.propertyPath = propertyPath;

            if (source == null || string.IsNullOrEmpty(propertyPath))
                return;

            // Get property info
            propertyInfo = source.GetType().GetProperty(propertyPath);
            if (propertyInfo == null)
            {
                Debug.LogError($"Property {propertyPath} not found on {source.GetType().Name}");
                return;
            }

            // Initial update
            UpdateTarget();

            // Subscribe to source changes if available
            if (source is INotifyPropertyChanged notifySource)
            {
                notifySource.PropertyChanged += OnSourcePropertyChanged;
            }
        }

        public virtual void Unbind()
        {
            if (source is INotifyPropertyChanged notifySource)
            {
                notifySource.PropertyChanged -= OnSourcePropertyChanged;
            }

            source = null;
            propertyInfo = null;
        }

        public abstract void UpdateSource();
        public abstract void UpdateTarget();

        protected virtual void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyPath && !isUpdating)
            {
                UpdateTarget();
            }
        }

        protected void SetSourceValue(object value)
        {
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                isUpdating = true;
                propertyInfo.SetValue(source, value);
                OnValueChanged?.Invoke(value);
                isUpdating = false;
            }
        }

        protected object GetSourceValue()
        {
            return propertyInfo?.GetValue(source);
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}