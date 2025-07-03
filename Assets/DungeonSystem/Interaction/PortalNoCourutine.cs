using UnityEngine;
using DungeonSystem;

namespace DungeonSystem.Interaction
{
    /// <summary>
    /// Portal que NO usa corrutinas para evitar congelamiento
    /// </summary>
    public class PortalNoCoroutine : MonoBehaviour
    {
        [Header("Settings")]
        public KeyCode interactionKey = KeyCode.E;
        public float interactionRange = 3f;
        public bool randomizeSeed = true;
        
        [Header("References")]
        public DungeonManager dungeonManager;
        
        [Header("UI Simple")]
        public GameObject promptUI;
        
        private Transform player;
        private bool playerInRange = false;
        private bool isGenerating = false;
        
        void Start()
        {
            Debug.Log("[Portal] Start - NO COROUTINES VERSION");
            
            // Buscar DungeonManager
            if (dungeonManager == null)
                dungeonManager = FindFirstObjectByType<DungeonManager>();
                
            if (dungeonManager != null)
                dungeonManager.autoGenerateOnStart = false;
            
            // Buscar jugador
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) player = playerObj.transform;
            
            // Ocultar UI
            if (promptUI) promptUI.SetActive(false);
        }
        
        void Update()
        {
            if (!player || isGenerating) return;
            
            // Verificar distancia
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;
            
            // Actualizar UI
            if (playerInRange != wasInRange)
            {
                if (promptUI) promptUI.SetActive(playerInRange);
            }
            
            // Detectar E
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                Debug.Log("[Portal] E pressed - Generating WITHOUT coroutine");
                StartCoroutine(GenerateDungeonDirect());
            }
        }
        
        System.Collections.IEnumerator GenerateDungeonDirect()
        {
            if (isGenerating || dungeonManager == null) yield break;

            isGenerating = true;

            Debug.Log("[Portal] === DIRECT GENERATION START ===");

            // Ocultar UI
            if (promptUI) promptUI.SetActive(false);

            // Nueva semilla
            if (randomizeSeed)
            {
                int seed = Random.Range(0, 999999);
                dungeonManager.generationSettings.seed = seed;
                Debug.Log($"[Portal] New seed: {seed}");
            }

            // Generar TODO de una vez (como el SafeGenerator)
            Debug.Log("[Portal] Calling GenerateCompleteDungeon async...");
            yield return StartCoroutine(dungeonManager.GenerateCompleteDungeonAsync());

            Debug.Log("[Portal] Generation complete!");

            // Teleport
            TeleportPlayer();

            isGenerating = false;

            // Reactivar UI si sigue cerca
            if (playerInRange && promptUI)
                promptUI.SetActive(true);
        }
        
        void TeleportPlayer()
        {
            if (!player || dungeonManager.DungeonData == null) return;
            
            Vector3 spawnPos = dungeonManager.transform.position;
            
            // Buscar entrada
            var entrance = dungeonManager.DungeonData.doors.Find(d => d.isEntrance);
            if (entrance != null)
            {
                spawnPos += new Vector3(entrance.position.x, 1, entrance.position.y + 3);
            }
            else if (dungeonManager.DungeonData.startingRoom != null)
            {
                var start = dungeonManager.DungeonData.startingRoom.centerPoint;
                spawnPos += new Vector3(start.x, 1, start.y);
            }
            else
            {
                spawnPos += new Vector3(50, 1, 50);
            }
            
            player.position = spawnPos;
            Debug.Log($"[Portal] Player teleported to {spawnPos}");
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}