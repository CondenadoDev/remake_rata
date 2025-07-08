// IConfigurable.cs
using UnityEngine;

namespace UISystem.Core
{
    public interface IConfigurable<T> where T : ScriptableObject
    {
        T Configuration { get; set; }
        void ApplyConfiguration(T config);
        void ResetToDefault();
    }
}