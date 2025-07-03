using UnityEngine;
using UnityEngine.UI;
using DungeonSystem;
using DungeonSystem.Core;
using DungeonSystem.Utils;

namespace DungeonSystem.Runtime
{
    /// <summary>
    /// Controlador para generar dungeons en runtime con UI
    /// </summary>
    public class RuntimeDungeonController : MonoBehaviour
    {
        [Header("Dungeon Manager")]
        [SerializeField] private DungeonManager dungeonManager;
        
        [Header("UI Elements")]
        [SerializeField] private Button generateButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button newSeedButton;
        [SerializeField] private Text seedText;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject loadingPanel;
        
        [Header("Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool validateAfterGeneration = true;
        
        private bool isGenerating = false;

        void Start()
        {
            // Buscar DungeonManager si no está asignado
            if (dungeonManager == null)
                dungeonManager = FindObjectOfType<DungeonManager>();
            
            // Configurar botones
            SetupUI();
            
            // Deshabilitar auto-generación
            if (dungeonManager != null)
                dungeonManager.autoGenerateOnStart = false;
        }

        void SetupUI()
        {
            // Configurar botón de generar
            if (generateButton != null)
            {
                generateButton.onClick.RemoveAllListeners();
                generateButton.onClick.AddListener(GenerateDungeon);
            }
            
            // Configurar botón de limpiar
            if (clearButton != null)
            {
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(ClearDungeon);
            }
            
            // Configurar botón de nueva semilla
            if (newSeedButton != null)
            {
                newSeedButton.onClick.RemoveAllListeners();
                newSeedButton.onClick.AddListener(GenerateNewSeed);
            }
            
            UpdateUI();
        }

        public void GenerateDungeon()
        {
            if (isGenerating || dungeonManager == null) return;
            
            StartCoroutine(GenerateDungeonCoroutine());
        }

        private System.Collections.IEnumerator GenerateDungeonCoroutine()
        {
            isGenerating = true;
            ShowLoading(true);
            UpdateStatus("Limpiando dungeon anterior...");
            
            // Limpiar dungeon anterior
            dungeonManager.ClearDungeon();
            yield return null;
            
            UpdateStatus("Generando estructura del mapa...");
            dungeonManager.GenerateMapStructure();
            yield return null;
            
            UpdateStatus("Seleccionando punto de inicio...");
            dungeonManager.SelectStartingPoint();
            yield return null;
            
            UpdateStatus("Configurando progresión inicial...");
            dungeonManager.SetupInitialProgression();
            yield return null;
            
            UpdateStatus("Poblando con entidades...");
            dungeonManager.PopulateWithEntities();
            yield return null;
            
            UpdateStatus("Renderizando dungeon...");
            dungeonManager.RenderDungeon();
            yield return null;
            
            // Validar el dungeon generado
            if (validateAfterGeneration)
            {
                UpdateStatus("Validando dungeon...");
                ValidateDungeon();
                yield return null;
            }
            
            UpdateStatus("¡Dungeon generado exitosamente!");
            ShowLoading(false);
            isGenerating = false;
            
            UpdateUI();
        }

        private void ValidateDungeon()
        {
            var dungeonData = GetDungeonData();
            if (dungeonData == null) return;
            
            // Validar conectividad
            bool allConnected = dungeonData.AreAllRoomsConnected();
            if (!allConnected)
            {
                Debug.LogError("¡ERROR! No todas las habitaciones están conectadas!");
                UpdateStatus("¡ERROR! Habitaciones desconectadas detectadas");
            }
            else
            {
                Debug.Log($"✓ Todas las {dungeonData.rooms.Count} habitaciones están conectadas");
            }
            
            // Validar con el sistema de validación completo
            var validationResult = DungeonValidator.ValidateDungeon(dungeonData);
            
            if (!validationResult.isValid)
            {
                Debug.LogError($"Dungeon inválido: {string.Join(", ", validationResult.errors)}");
                UpdateStatus($"Dungeon inválido: {validationResult.errors[0]}");
            }
            else
            {
                Debug.Log($"Dungeon válido - Completabilidad: {validationResult.completabilityScore:P0}, Balance: {validationResult.balanceScore:P0}");
                
                if (validationResult.warnings.Count > 0)
                {
                    Debug.LogWarning($"Advertencias: {string.Join(", ", validationResult.warnings)}");
                }
            }
        }

        public void ClearDungeon()
        {
            if (dungeonManager == null) return;
            
            dungeonManager.ClearDungeon();
            UpdateStatus("Dungeon limpiado");
            UpdateUI();
        }

        public void GenerateNewSeed()
        {
            if (dungeonManager == null || dungeonManager.generationSettings == null) return;
            
            dungeonManager.generationSettings.seed = Random.Range(0, 999999);
            UpdateUI();
            UpdateStatus($"Nueva semilla: {dungeonManager.generationSettings.seed}");
        }

        private void UpdateUI()
        {
            // Actualizar texto de semilla
            if (seedText != null && dungeonManager != null && dungeonManager.generationSettings != null)
            {
                seedText.text = $"Seed: {dungeonManager.generationSettings.seed}";
            }
            
            // Habilitar/deshabilitar botones según el estado
            if (generateButton != null)
                generateButton.interactable = !isGenerating;
                
            if (clearButton != null)
                clearButton.interactable = !isGenerating && HasDungeon();
                
            if (newSeedButton != null)
                newSeedButton.interactable = !isGenerating;
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
                
            if (showDebugInfo)
                Debug.Log($"[RuntimeDungeonController] {message}");
        }

        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(show);
        }

        private bool HasDungeon()
        {
            var dungeonData = GetDungeonData();
            return dungeonData != null && dungeonData.rooms.Count > 0;
        }

        private DungeonData GetDungeonData()
        {
            if (dungeonManager == null) return null;
            
            // Usar reflexión para obtener dungeonData (es privado)
            var fieldInfo = typeof(DungeonManager).GetField("dungeonData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fieldInfo != null)
                return fieldInfo.GetValue(dungeonManager) as DungeonData;
                
            return null;
        }

        // Método para crear UI automáticamente si no existe
        [ContextMenu("Create Runtime UI")]
        private void CreateRuntimeUI()
        {
            // Buscar Canvas o crear uno
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Crear panel de control
            GameObject panel = new GameObject("Dungeon Control Panel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(10, -10);
            panelRect.sizeDelta = new Vector2(250, 200);
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.8f);

            // Layout vertical
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;

            // Crear botón Generate
            generateButton = CreateButton("Generate Dungeon", panel.transform);
            
            // Crear botón Clear
            clearButton = CreateButton("Clear Dungeon", panel.transform);
            
            // Crear botón New Seed
            newSeedButton = CreateButton("New Random Seed", panel.transform);
            
            // Crear texto de semilla
            GameObject seedObj = new GameObject("Seed Text");
            seedObj.transform.SetParent(panel.transform, false);
            seedText = seedObj.AddComponent<Text>();
            seedText.text = "Seed: 0";
            seedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            seedText.color = Color.white;
            
            // Crear texto de estado
            GameObject statusObj = new GameObject("Status Text");
            statusObj.transform.SetParent(panel.transform, false);
            statusText = statusObj.AddComponent<Text>();
            statusText.text = "Ready";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.color = Color.green;
            
            // Panel de carga
            loadingPanel = new GameObject("Loading Panel");
            loadingPanel.transform.SetParent(canvas.transform, false);
            RectTransform loadingRect = loadingPanel.AddComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;
            
            Image loadingImg = loadingPanel.AddComponent<Image>();
            loadingImg.color = new Color(0, 0, 0, 0.5f);
            
            GameObject loadingText = new GameObject("Loading Text");
            loadingText.transform.SetParent(loadingPanel.transform, false);
            Text loadText = loadingText.AddComponent<Text>();
            loadText.text = "Generating...";
            loadText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            loadText.color = Color.white;
            loadText.fontSize = 24;
            loadText.alignment = TextAnchor.MiddleCenter;
            RectTransform loadTextRect = loadingText.GetComponent<RectTransform>();
            loadTextRect.anchorMin = Vector2.zero;
            loadTextRect.anchorMax = Vector2.one;
            loadTextRect.offsetMin = Vector2.zero;
            loadTextRect.offsetMax = Vector2.zero;
            
            loadingPanel.SetActive(false);
            
            Debug.Log("Runtime UI created successfully!");
        }

        private Button CreateButton(string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(text);
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonImg = buttonObj.AddComponent<Image>();
            buttonImg.color = new Color(0.2f, 0.2f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30;
            
            return button;
        }
    }
}