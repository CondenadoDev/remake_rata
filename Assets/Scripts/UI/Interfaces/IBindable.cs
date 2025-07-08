// IBindable.cs
using System;

namespace UISystem.Core
{
    public interface IBindable
    {
        event Action<object> OnValueChanged;
        object GetValue();
        void SetValue(object value);
        Type GetValueType();
    }
}