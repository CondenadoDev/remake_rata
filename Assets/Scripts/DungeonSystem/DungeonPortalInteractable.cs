using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DungeonSystem;
using DungeonSystem.Runtime;

namespace DungeonSystem.Interaction
{
    /// <summary>
    /// Objeto interactuable que permite al jugador generar nuevos dungeons
    /// </summary>
    public class DungeonPortalInteractable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private string interactionPrompt = "Press [E] to enter dungeon";
        [SerializeField] private LayerMask playerLayer = -1;
        
        [Header("Dungeon Generation")]
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private bool randomizeSeedOnInteract = true;
        [SerializeField] private bool showSeedSelectionUI = true;
        [SerializeField] private Transform dungeonSpawnPoint;
        [SerializeField] private Transform playerTeleportPoint;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private Text promptText;
        [SerializeField] private GameObject seedSelectionUI;
        [SerializeField] private InputField seedInputField;
        [SerializeField] private Button generateButton;
        [SerializeField] private Button randomSeedButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text currentSeedText;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip interactionSound;
        [SerializeField] private AudioClip generationSound;
        
        [Header("Loading")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private Text loadingText;
        
        private Transform playerTransform;
        private bool playerInRange = false;
        private bool isGenerating = false;
        private bool uiActive = false;
        
        void Start()
        {
            // Buscar DungeonManager si no está asignado
            if (dungeonManager == null)
                dungeonManager = FindObjectOfType<DungeonManager>();
                
            // Configurar UI
            SetupUI();
            
            // Ocultar UI al inicio
            if (interactionPromptUI) interactionPromptUI.SetActive(false);
            if (seedSelectionUI) seedSelectionUI.SetActive(false);
            if (loadingScreen) loadingScreen.SetActive(false);
            if (highlightEffect) highlightEffect.SetActive(false);
            
            // Buscar jugador
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerTransform = player.transform;
        }
        
        void SetupUI()
        {
            // Configurar botones
            if (generateButton)
                generateButton.onClick.AddListener(OnGenerateButtonClick);
                
            if (randomSeedButton)
                randomSeedButton.onClick.AddListener(OnRandomSeedButtonClick);
                
            if (cancelButton)
                cancelButton.onClick.AddListener(OnCancelButtonClick);
                
            if (seedInputField)
                seedInputField.onEndEdit.AddListener(OnSeedInputChanged);
                
            // Configurar texto del prompt
            if (promptText)
                promptText.text = interactionPrompt.Replace("[E]", $"[{interactionKey}]");
        }
        
        void Update()
        {
            if (!playerTransform || isGenerating) return;
            
            // Verificar distancia al jugador
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            // Actualizar UI de interacción
            if (playerInRange != wasInRange)
            {
                OnPlayerRangeChanged();
            }
            
            // Manejar input de interacción
            if (playerInRange && !uiActive && Input.GetKeyDown(interactionKey))
            {
                OnInteract();
            }
            
            // Cerrar UI con ESC
            if (uiActive && Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelButtonClick();
            }
        }
        
        void OnPlayerRangeChanged()
        {
            if (interactionPromptUI)
                interactionPromptUI.SetActive(playerInRange && !uiActive);
                
            if (highlightEffect)
                highlightEffect.SetActive(playerInRange);
        }
        
        void OnInteract()
        {
            if (audioSource && interactionSound)
                audioSource.PlayOneShot(interactionSound);
                
            if (showSeedSelectionUI)
            {
                ShowSeedSelectionUI();
            }
            else
            {
                // Generar directamente con semilla aleatoria
                if (randomizeSeedOnInteract)
                    dungeonManager.generationSettings.seed = Random.Range(0, 999999);
                    
                StartCoroutine(GenerateDungeonCoroutine());
            }
        }
        
        void ShowSeedSelectionUI()
        {
            uiActive = true;
            
            if (seedSelectionUI)
                seedSelectionUI.SetActive(true);
                
            if (interactionPromptUI)
                interactionPromptUI.SetActive(false);
                
            // Mostrar semilla actual
            UpdateSeedDisplay();
            
            // Pausar el juego (opcional)
            Time.timeScale = 0f;
            
            // Desbloquear cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        void HideSeedSelectionUI()
        {
            uiActive = false;
            
            if (seedSelectionUI)
                seedSelectionUI.SetActive(false);
                
            // Reanudar el juego
            Time.timeScale = 1f;
            
            // Bloquear cursor nuevamente
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void UpdateSeedDisplay()
        {
            if (currentSeedText && dungeonManager && dungeonManager.generationSettings)
            {
                currentSeedText.text = $"Current Seed: {dungeonManager.generationSettings.seed}";
            }
            
            if (seedInputField && dungeonManager && dungeonManager.generationSettings)
            {
                seedInputField.text = dungeonManager.generationSettings.seed.ToString();
            }
        }
        
        void OnGenerateButtonClick()
        {
            HideSeedSelectionUI();
            StartCoroutine(GenerateDungeonCoroutine());
        }
        
        void OnRandomSeedButtonClick()
        {
            if (dungeonManager && dungeonManager.generationSettings)
            {
                dungeonManager.generationSettings.seed = Random.Range(0, 999999);
                UpdateSeedDisplay();
            }
        }
        
        void OnCancelButtonClick()
        {
            HideSeedSelectionUI();
        }
        
        void OnSeedInputChanged(string value)
        {
            if (int.TryParse(value, out int seed))
            {
                if (dungeonManager && dungeonManager.generationSettings)
                {
                    dungeonManager.generationSettings.seed = seed;
                    UpdateSeedDisplay();
                }
            }
        }
        
        IEnumerator GenerateDungeonCoroutine()
        {
            isGenerating = true;
            
            // Mostrar pantalla de carga
            if (loadingScreen)
            {
                loadingScreen.SetActive(true);
                if (loadingBar) loadingBar.value = 0f;
                if (loadingText) loadingText.text = "Preparing...";
            }
            
            // Efectos visuales
            if (activationParticles) activationParticles.Play();
            if (audioSource && generationSound) audioSource.PlayOneShot(generationSound);
            
            yield return new WaitForSeconds(0.5f);
            
            // Limpiar dungeon anterior
            UpdateLoading(0.1f, "Clearing previous dungeon...");
            dungeonManager.ClearDungeon();
            yield return null;
            
            // Generar estructura
            UpdateLoading(0.2f, "Generating structure...");
            dungeonManager.GenerateMapStructure();
            yield return null;
            
            // Seleccionar punto de inicio
            UpdateLoading(0.4f, "Selecting entrance...");
            dungeonManager.SelectStartingPoint();
            yield return null;
            
            // Configurar progresión
            UpdateLoading(0.5f, "Setting up progression...");
            dungeonManager.SetupInitialProgression();
            yield return null;
            
            // Poblar con entidades
            UpdateLoading(0.7f, "Populating dungeon...");
            dungeonManager.PopulateWithEntities();
            yield return null;
            
            // Renderizar
            UpdateLoading(0.9f, "Rendering dungeon...");
            dungeonManager.RenderDungeon();
            yield return null;
            
            UpdateLoading(1f, "Complete!");
            yield return new WaitForSeconds(0.5f);
            
            // Teletransportar al jugador
            TeleportPlayerToDungeon();
            
            // Ocultar pantalla de carga
            if (loadingScreen) loadingScreen.SetActive(false);
            
            isGenerating = false;
            
            Debug.Log($"Dungeon generated with seed: {dungeonManager.generationSettings.seed}");
        }
        
        void UpdateLoading(float progress, string message)
        {
            if (loadingBar) loadingBar.value = progress;
            if (loadingText) loadingText.text = message;
        }
        
        void TeleportPlayerToDungeon()
        {
            if (!playerTransform || !dungeonManager || dungeonManager.DungeonData == null) return;
            
            // Buscar la entrada del dungeon
            var entranceDoor = dungeonManager.DungeonData.doors.Find(d => d.isEntrance);
            
            Vector3 spawnPosition;
            
            if (entranceDoor != null)
            {
                // Spawn fuera de la entrada principal
                float offset = 3f;
                switch (entranceDoor.orientation)
                {
                    case DungeonSystem.Core.DoorOrientation.Horizontal:
                        if (entranceDoor.position.y > dungeonManager.DungeonData.height / 2)
                            offset = -offset; // Entrada por el norte, spawn al sur
                        spawnPosition = new Vector3(entranceDoor.position.x, 1f, entranceDoor.position.y + offset);
                        break;
                    case DungeonSystem.Core.DoorOrientation.Vertical:
                        if (entranceDoor.position.x > dungeonManager.DungeonData.width / 2)
                            offset = -offset; // Entrada por el este, spawn al oeste
                        spawnPosition = new Vector3(entranceDoor.position.x + offset, 1f, entranceDoor.position.y);
                        break;
                    default:
                        spawnPosition = new Vector3(entranceDoor.position.x, 1f, entranceDoor.position.y + 3);
                        break;
                }
            }
            else if (playerTeleportPoint != null)
            {
                // Usar punto de teletransporte personalizado
                spawnPosition = playerTeleportPoint.position;
            }
            else if (dungeonManager.DungeonData.startingRoom != null)
            {
                // Spawn en el centro de la habitación inicial
                var startRoom = dungeonManager.DungeonData.startingRoom;
                spawnPosition = new Vector3(startRoom.centerPoint.x, 1f, startRoom.centerPoint.y);
            }
            else
            {
                // Fallback: centro del dungeon
                spawnPosition = new Vector3(dungeonManager.DungeonData.width / 2f, 1f, dungeonManager.DungeonData.height / 2f);
            }
            
            // Teletransportar al jugador
            if (dungeonSpawnPoint)
                spawnPosition += dungeonSpawnPoint.position;
                
            playerTransform.position = spawnPosition;
            
            // Rotar al jugador hacia la entrada
            if (entranceDoor != null)
            {
                Vector3 lookDirection = new Vector3(entranceDoor.position.x, 0, entranceDoor.position.y) - new Vector3(spawnPosition.x, 0, spawnPosition.z);
                if (lookDirection != Vector3.zero)
                    playerTransform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            Debug.Log($"Player teleported to dungeon entrance at {spawnPosition}");
        }
        
        void OnDrawGizmos()
        {
            // Dibujar rango de interacción
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Dibujar punto de spawn del dungeon
            if (dungeonSpawnPoint)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(dungeonSpawnPoint.position, Vector3.one * 2f);
                Gizmos.DrawLine(transform.position, dungeonSpawnPoint.position);
            }
            
            // Dibujar punto de teletransporte del jugador
            if (playerTeleportPoint)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerTeleportPoint.position, 0.5f);
                Gizmos.DrawRay(playerTeleportPoint.position, playerTeleportPoint.forward * 2f);
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Mostrar más detalles cuando está seleccionado
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, interactionRange);
        }
    }
}