using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISystem.Configuration
{
    [CreateAssetMenu(fileName = "UIGenerationConfig", menuName = "UI System/Generation Config")]
    public class UIGenerationConfig : ScriptableObject
    {
        public List<PanelConfig> panels = new List<PanelConfig>();
        public UITheme theme;
        public NavigationConfig navigation;
    }

    [Serializable]
    public class PanelConfig
    {
        public string panelId;
        public string displayName;
        public PanelType panelType;
        public bool startHidden = true;
        public PanelAnimation showAnimation = PanelAnimation.FadeScale;
        public PanelAnimation hideAnimation = PanelAnimation.FadeScale;
        public List<ElementConfig> elements = new List<ElementConfig>();
    }

    [Serializable]
    public class ElementConfig
    {
        public string elementId;
        public string displayText;
        public ElementType elementType;

        [Header("Binding")] public string bindingPath;
        public BindingMode bindingMode = BindingMode.TwoWay;

        [Header("Type-Specific")] public string actionTarget; // For buttons
        public float minValue = 0; // For sliders
        public float maxValue = 1;
        public bool wholeNumbers = false;
        public List<string> options; // For dropdowns
        public string placeholder; // For input fields
    }

    [Serializable]
    public class NavigationConfig
    {
        public bool enableKeyboardNavigation = true;
        public bool enableGamepadNavigation = true;
        public bool wrapAround = true;
        public KeyCode submitKey = KeyCode.Return;
        public KeyCode cancelKey = KeyCode.Escape;
    }

    [Serializable]
    public class UITheme
    {
        public Color primaryColor = new Color(0.2f, 0.6f, 1f);
        public Color secondaryColor = new Color(0.3f, 0.3f, 0.3f);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        public Color textColor = Color.white;
        public Font defaultFont;
        public float defaultFontSize = 14f;
    }
}