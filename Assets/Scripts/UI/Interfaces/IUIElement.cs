// IUIElement.cs
using UnityEngine;

namespace UISystem.Core
{
    public interface IUIElement
    {
        GameObject GameObject { get; }
        void Initialize();
        void Show(float duration = 0.3f);
        void Hide(float duration = 0.3f);
        void SetInteractable(bool interactable);
    }
}