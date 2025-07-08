 using System.Collections.Generic;
 using TMPro;
 using UISystem.Binding;
 using UISystem.Configuration;
 using UISystem.Core;
 using UISystem.Panels;
 using UnityEngine;
 using UnityEngine.UI;

 public class UIGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private UIGenerationConfig generationConfig;
        [SerializeField] private Transform targetContainer;
        [SerializeField] private bool generateOnStart = true;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject panelPrefab;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject sliderPrefab;
        [SerializeField] private GameObject togglePrefab;
        [SerializeField] private GameObject dropdownPrefab;
        [SerializeField] private GameObject inputFieldPrefab;
        [SerializeField] private GameObject textPrefab;
        
        [Header("Layout")]
        [SerializeField] private float elementSpacing = 10f;
        [SerializeField] private RectOffset padding; 
        
        private Dictionary<string, GameObject> generatedPanels = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateUI();
            }
            if (padding == null)
                padding = new RectOffset(20, 20, 20, 20);
        }

        public void GenerateUI()
        {
            if (generationConfig == null)
            {
                Debug.LogError("No generation config assigned");
                return;
            }
            
            // Clear existing generated UI
            ClearGeneratedUI();
            
            // Generate panels
            foreach (var panelConfig in generationConfig.panels)
            {
                GeneratePanel(panelConfig);
            }
            
            // Register all panels with UIManager
            UIManager.Instance.RegisterExistingPanels();
            
            // Save generated layout
            SaveGeneratedLayout();
        }

        private void GeneratePanel(PanelConfig config)
        {
            // Create panel
            GameObject panelGO = Instantiate(panelPrefab, targetContainer);
            panelGO.name = config.panelId;
            
            // Setup panel component
            BaseUIPanel panelComponent = AddPanelComponent(panelGO, config.panelType, config.panelId);
            if (panelComponent != null)
            {
                panelComponent.panelId = config.panelId;
                panelComponent.startHidden = config.startHidden;
                panelComponent.showAnimation = config.showAnimation;
                panelComponent.hideAnimation = config.hideAnimation;
            }
            
            // Setup layout
            VerticalLayoutGroup layout = panelGO.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = panelGO.AddComponent<VerticalLayoutGroup>();
            }
            layout.spacing = elementSpacing;
            layout.padding = padding;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            // Generate elements
            foreach (var element in config.elements)
            {
                GenerateElement(element, panelGO.transform);
            }
            
            generatedPanels[config.panelId] = panelGO;
        }

        private BaseUIPanel AddPanelComponent(GameObject panelGO, PanelType type, string panelId)
        {
            switch (type)
            {
                case PanelType.Menu:
                    return panelGO.AddComponent<MenuPanel>();
                case PanelType.Settings:
                    switch (panelId)
                    {
                        case "AudioOptions":
                            return panelGO.AddComponent<AudioSettingsPanel>();
                        case "GraphicsOptions":
                            return panelGO.AddComponent<GraphicsSettingsPanel>();
                        case "ControlsOptions":
                            return panelGO.AddComponent<InputSettingsPanel>();
                        case "GameplayOptions":
                            return panelGO.AddComponent<GameplaySettingsPanel>();
                        case "PlayerOptions":
                            return panelGO.AddComponent<PlayerSettingsPanel>();
                        default:
                            return panelGO.AddComponent<GraphicsSettingsPanel>();
                    }
                case PanelType.HUD:
                    return panelGO.AddComponent<HUDPanel>();
                default:
                    return panelGO.AddComponent<GenericPanel>();
            }
        }

        private void GenerateElement(ElementConfig config, Transform parent)
        {
            GameObject elementGO = null;
            
            switch (config.elementType)
            {
                case ElementType.Button:
                    elementGO = Instantiate(buttonPrefab, parent);
                    SetupButton(elementGO, config);
                    break;
                    
                case ElementType.Slider:
                    elementGO = Instantiate(sliderPrefab, parent);
                    SetupSlider(elementGO, config);
                    break;
                    
                case ElementType.Toggle:
                    elementGO = Instantiate(togglePrefab, parent);
                    SetupToggle(elementGO, config);
                    break;
                    
                case ElementType.Dropdown:
                    elementGO = Instantiate(dropdownPrefab, parent);
                    SetupDropdown(elementGO, config);
                    break;
                    
                case ElementType.InputField:
                    elementGO = Instantiate(inputFieldPrefab, parent);
                    SetupInputField(elementGO, config);
                    break;
                    
                case ElementType.Text:
                    elementGO = Instantiate(textPrefab, parent);
                    SetupText(elementGO, config);
                    break;
            }
            
            if (elementGO != null)
            {
                elementGO.name = config.elementId;
                
                // Add binding if specified
                if (!string.IsNullOrEmpty(config.bindingPath))
                {
                    AddBinding(elementGO, config);
                }
            }
        }

        private void SetupButton(GameObject buttonGO, ElementConfig config)
        {
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text != null)
            {
                text.text = config.displayText;
            }
            
            // Add click handler if specified
            if (!string.IsNullOrEmpty(config.actionTarget))
            {
                button.onClick.AddListener(() => 
                {
                    UIManager.Instance.ShowPanel(config.actionTarget);
                });
            }
        }

        private void SetupSlider(GameObject sliderGO, ElementConfig config)
        {
            Slider slider = sliderGO.GetComponent<Slider>();
            TextMeshProUGUI label = sliderGO.GetComponentInChildren<TextMeshProUGUI>();
            
            if (label != null)
            {
                label.text = config.displayText;
            }
            
            slider.minValue = config.minValue;
            slider.maxValue = config.maxValue;
            slider.wholeNumbers = config.wholeNumbers;
            
            // Add value display
            GameObject valueText = new GameObject("Value");
            valueText.transform.SetParent(sliderGO.transform);
            TextMeshProUGUI valueDisplay = valueText.AddComponent<TextMeshProUGUI>();
            valueDisplay.text = slider.value.ToString();
            
            slider.onValueChanged.AddListener((value) => 
            {
                valueDisplay.text = config.wholeNumbers ? value.ToString("0") : value.ToString("0.00");
            });
        }

        private void SetupToggle(GameObject toggleGO, ElementConfig config)
        {
            Toggle toggle = toggleGO.GetComponent<Toggle>();
            TextMeshProUGUI label = toggleGO.GetComponentInChildren<TextMeshProUGUI>();
            
            if (label != null)
            {
                label.text = config.displayText;
            }
        }

        private void SetupDropdown(GameObject dropdownGO, ElementConfig config)
        {
            TMP_Dropdown dropdown = dropdownGO.GetComponent<TMP_Dropdown>();
            TextMeshProUGUI label = dropdownGO.GetComponentInChildren<TextMeshProUGUI>();
            
            if (label != null)
            {
                label.text = config.displayText;
            }
            
            if (config.options != null && config.options.Count > 0)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(config.options);
            }
        }

        private void SetupInputField(GameObject inputGO, ElementConfig config)
        {
            TMP_InputField input = inputGO.GetComponent<TMP_InputField>();
            TextMeshProUGUI label = inputGO.GetComponentInChildren<TextMeshProUGUI>();
            
            if (label != null)
            {
                label.text = config.displayText;
            }
            
            if (!string.IsNullOrEmpty(config.placeholder))
            {
                input.placeholder.GetComponent<TextMeshProUGUI>().text = config.placeholder;
            }
        }

        private void SetupText(GameObject textGO, ElementConfig config)
        {
            TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
            text.text = config.displayText;
        }

        private void AddBinding(GameObject elementGO, ElementConfig config)
        {
            DataBinding binding = null;
            
            switch (config.elementType)
            {
                case ElementType.Slider:
                    binding = elementGO.AddComponent<SliderBinding>();
                    break;
                case ElementType.Toggle:
                    binding = elementGO.AddComponent<ToggleBinding>();
                    break;
                case ElementType.Dropdown:
                    binding = elementGO.AddComponent<DropdownBinding>();
                    break;
                case ElementType.InputField:
                case ElementType.Text:
                    binding = elementGO.AddComponent<TextBinding>();
                    break;
            }
            
            if (binding != null)
            {
                binding.propertyPath = config.bindingPath;
                binding.bindingMode = config.bindingMode;
            }
        }

        public void ClearGeneratedUI()
        {
            foreach (var panel in generatedPanels.Values)
            {
                if (panel != null)
                {
                    DestroyImmediate(panel);
                }
            }
            generatedPanels.Clear();
        }

        private void SaveGeneratedLayout()
        {
            #if UNITY_EDITOR
            // Save the generated layout as a prefab variant
            string path = "Assets/GeneratedUI/";
            if (!UnityEditor.AssetDatabase.IsValidFolder(path))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "GeneratedUI");
            }
            
            foreach (var kvp in generatedPanels)
            {
                string prefabPath = $"{path}{kvp.Key}.prefab";
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(kvp.Value, prefabPath);
            }
            
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
    }