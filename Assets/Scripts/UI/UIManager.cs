using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UISystem.Panels;
using UISystem.Configuration;
using DG.Tweening;

namespace UISystem.Core
{
    public class UIManager : MonoBehaviour
    {
        public static event Action<string> OnPanelOpened;
        public static event Action<string> OnPanelClosed;
        public static event Action<BaseUIPanel, BaseUIPanel> OnPanelSwitched;
        
        private static UIManager _instance;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                    }
                }

                return _instance;
            }
        }

        [Header("UI Settings")] [SerializeField]
        private Canvas mainCanvas;

        [SerializeField] private Camera uiCamera;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GraphicRaycaster raycaster;

        [Header("Panel Management")] [SerializeField]
        private Transform panelContainer;

        [SerializeField] private bool allowMultiplePanels = false;
        [SerializeField] private float panelTransitionTime = 0.3f;
        [SerializeField] private AudioClip clickSound;

        private Dictionary<string, BaseUIPanel> panels = new Dictionary<string, BaseUIPanel>();
        private Stack<BaseUIPanel> navigationStack = new Stack<BaseUIPanel>();
        private BaseUIPanel currentPanel;

        public event Action<BaseUIPanel> OnPanelShown;
        public event Action<BaseUIPanel> OnPanelHidden;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create main canvas if not assigned
            if (mainCanvas == null)
            {
                GameObject canvasGO = new GameObject("Main Canvas");
                mainCanvas = canvasGO.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 0;

                canvasScaler = canvasGO.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;

                raycaster = canvasGO.AddComponent<GraphicRaycaster>();
                canvasGO.transform.SetParent(transform);
            }

            // Create panel container
            if (panelContainer == null)
            {
                GameObject containerGO = new GameObject("Panel Container");
                containerGO.transform.SetParent(mainCanvas.transform);
                RectTransform rt = containerGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                panelContainer = containerGO.transform;
            }

            // Register existing panels
            RegisterExistingPanels();
        }

        public void RegisterExistingPanels()
        {
            BaseUIPanel[] existingPanels = panelContainer.GetComponentsInChildren<BaseUIPanel>(true);
            foreach (var panel in existingPanels)
            {
                RegisterPanel(panel);
            }
        }

        public void RegisterPanel(BaseUIPanel panel)
        {
            if (panel == null || string.IsNullOrEmpty(panel.PanelId)) return;

            if (panels.ContainsKey(panel.PanelId))
            {
                Debug.LogWarning($"Panel with ID {panel.PanelId} already registered");
                return;
            }

            panels[panel.PanelId] = panel;
            panel.Initialize();

            panel.OnPanelShown += OnPanelShownHandler;
            panel.OnPanelHidden += OnPanelHiddenHandler;
        }

        public void UnregisterPanel(string panelId)
        {
            if (panels.TryGetValue(panelId, out BaseUIPanel panel))
            {
                panel.OnPanelShown -= OnPanelShownHandler;
                panel.OnPanelHidden -= OnPanelHiddenHandler;
                panels.Remove(panelId);
            }
        }

        public void ShowPanel(string panelId, bool addToStack = true)
        {
            if (!panels.TryGetValue(panelId, out BaseUIPanel panel))
            {
                Debug.LogError($"Panel {panelId} not found");
                return;
            }

            // Detecta el panel anterior antes de cambiar
            BaseUIPanel previousPanel = currentPanel;

            // Hide current panel if not allowing multiple
            if (!allowMultiplePanels && currentPanel != null && currentPanel != panel)
            {
                currentPanel.Hide();
            }

            // Add to navigation stack
            if (addToStack && !navigationStack.Contains(panel))
            {
                navigationStack.Push(panel);
            }

            currentPanel = panel;
            panel.Show(panelTransitionTime);

            // --- LLAMA AL EVENTO GLOBAL ---
            OnPanelOpened?.Invoke(panelId);

            // --- LLAMA EVENTO DE CAMBIO DE PANEL ---
            if (previousPanel != null && previousPanel != panel)
            {
                OnPanelSwitched?.Invoke(previousPanel, panel);
            }
        }

        public void HidePanel(string panelId)
        {
            if (panels.TryGetValue(panelId, out BaseUIPanel panel))
            {
                panel.Hide(panelTransitionTime);

                if (navigationStack.Count > 0 && navigationStack.Peek() == panel)
                {
                    navigationStack.Pop();
                }

                // --- LLAMA AL EVENTO GLOBAL ---
                OnPanelClosed?.Invoke(panelId);
            }
        }

        public void HideAllPanels()
        {
            foreach (var panel in panels.Values)
            {
                if (panel.IsVisible)
                {
                    panel.Hide(panelTransitionTime);
                }
            }

            navigationStack.Clear();
            currentPanel = null;
        }

        public void GoBack()
        {
            if (navigationStack.Count <= 1) return;

            // Pop current panel
            var current = navigationStack.Pop();
            current.Hide(panelTransitionTime);

            // Show previous panel
            if (navigationStack.Count > 0)
            {
                var previous = navigationStack.Peek();
                ShowPanel(previous.PanelId, false);
            }
        }

        public T GetPanel<T>(string panelId) where T : class
        {
            if (panels.TryGetValue(panelId, out BaseUIPanel panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// Plays a simple UI click sound through the AudioManager if available.
        /// </summary>
        public void PlayClickSound()
        {
            if (AudioManager.Instance != null && clickSound != null)
            {
                AudioManager.Instance.PlayUISFX(clickSound);
            }
        }

        private void OnPanelShownHandler(BaseUIPanel panel)
        {
            OnPanelShown?.Invoke(panel);
        }

        private void OnPanelHiddenHandler(BaseUIPanel panel)
        {
            if (currentPanel == panel)
            {
                currentPanel = null;
            }

            OnPanelHidden?.Invoke(panel);
        }

        private void Update()
        {
            // Handle back button/escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoBack();
            }
        }
    }
}
