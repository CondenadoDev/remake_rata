using UnityEngine;
using DungeonSystem;
using System.Collections;

namespace DungeonSystem.Interaction
{
    /// <summary>
    /// Versión simplificada del generador interactivo
    /// </summary>
    public class SimpleDungeonGenerator : MonoBehaviour
    {
        [Header("Basic Settings")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private bool generateNewSeedEachTime = true;
        
        [Header("References")]
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private Transform playerSpawnPoint;
        
        [Header("Simple UI")]
        [SerializeField] private GameObject promptCanvas;
        [SerializeField] private UnityEngine.UI.Text promptText;
        [SerializeField] private GameObject loadingCanvas;
        
        [Header("Effects")]
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip generateSound;
        
        private Transform player;
        private bool playerInRange = false;
        private bool isGenerating = false;
        
        void Start()
        {
            // Buscar componentes
            if (!dungeonManager)
                dungeonManager = FindObjectOfType<DungeonManager>();
                
            if (!player)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj) player = playerObj.transform;
            }
            
            // Configurar UI
            if (promptCanvas) promptCanvas.SetActive(false);
            if (loadingCanvas) loadingCanvas.SetActive(false);
            if (glowEffect) glowEffect.SetActive(false);
            
            // Configurar texto
            if (promptText)
                promptText.text = $"Press [{interactionKey}] to generate dungeon";
                
            // Asegurar que no se genere automáticamente
            if (dungeonManager)
                dungeonManager.autoGenerateOnStart = false;
        }
        
        void Update()
        {
            if (!player || isGenerating) return;
            
            // Verificar distancia
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            // Actualizar visual
            if (playerInRange != wasInRange)
            {
                if (promptCanvas) promptCanvas.SetActive(playerInRange);
                if (glowEffect) glowEffect.SetActive(playerInRange);
            }
            
            // Detectar input
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                GenerateNewDungeon();
            }
        }
        
        public void GenerateNewDungeon()
        {
            if (isGenerating || !dungeonManager) return;
            
            StartCoroutine(GenerateCoroutine());
        }
        
        IEnumerator GenerateCoroutine()
        {
            isGenerating = true;
            
            // Ocultar prompt
            if (promptCanvas) promptCanvas.SetActive(false);
            
            // Mostrar loading
            if (loadingCanvas) loadingCanvas.SetActive(true);
            
            // Sonido
            if (audioSource && generateSound)
                audioSource.PlayOneShot(generateSound);
            
            // Generar nueva semilla si está activado
            if (generateNewSeedEachTime)
            {
                int newSeed = Random.Range(0, 999999);
                dungeonManager.generationSettings.seed = newSeed;
                Debug.Log($"Generating with new seed: {newSeed}");
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Generar dungeon
            dungeonManager.GenerateCompleteDungeon();
            
            yield return new WaitForSeconds(0.5f);
            
            // Teletransportar jugador
            TeleportPlayer();
            
            // Ocultar loading
            if (loadingCanvas) loadingCanvas.SetActive(false);
            
            isGenerating = false;
            
            // Mostrar prompt de nuevo si el jugador sigue cerca
            if (playerInRange && promptCanvas)
                promptCanvas.SetActive(true);
        }
        
        void TeleportPlayer()
        {
            if (!player || dungeonManager.DungeonData == null) return;
            
            Vector3 spawnPos;
            
            // Opción 1: Usar punto de spawn custom
            if (playerSpawnPoint)
            {
                spawnPos = playerSpawnPoint.position;
            }
            // Opción 2: Buscar entrada del dungeon
            else
            {
                var entrance = dungeonManager.DungeonData.doors.Find(d => d.isEntrance);
                if (entrance != null)
                {
                    // Spawn fuera de la entrada
                    spawnPos = new Vector3(entrance.position.x, 1f, entrance.position.y + 3);
                }
                else if (dungeonManager.DungeonData.startingRoom != null)
                {
                    // Spawn en habitación inicial
                    var start = dungeonManager.DungeonData.startingRoom.centerPoint;
                    spawnPos = new Vector3(start.x, 1f, start.y);
                }
                else
                {
                    // Centro del mapa
                    spawnPos = new Vector3(50, 1f, 50);
                }
            }
            
            player.position = spawnPos;
        }
        
        void OnDrawGizmos()
        {
            // Mostrar rango
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Línea al spawn point
            if (playerSpawnPoint)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, playerSpawnPoint.position);
                Gizmos.DrawWireCube(playerSpawnPoint.position, Vector3.one);
            }
        }
    }
}