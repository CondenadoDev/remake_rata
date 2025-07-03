// DungeonPortalInteractable.cs
//---------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DungeonSystem;
using DungeonSystem.Spawning;   // para SpawnSystem.OnSpawningComplete

namespace DungeonSystem.Interaction
{
    /// <summary>
    /// Interactable portal that generates a dungeon on demand,
    /// teleports the player and reports spawn statistics.
    /// </summary>
    public class DungeonPortalInteractable : MonoBehaviour
    {
        #region Inspector ----------------------------------------------------

        [Header("Interaction Settings")]
        [SerializeField] float  interactionRange      = 3f;
        [SerializeField] KeyCode interactionKey       = KeyCode.E;
        [SerializeField] bool   randomizeSeedOnInteract = true;

        [Header("Dungeon Generation")]
        [SerializeField]
        public DungeonManager dungeonManager;
        [SerializeField] Transform      playerTeleportPoint;
        [SerializeField] bool teleportPlayer = true;
        
        [Header("Simple UI")]
        [SerializeField] GameObject interactionPromptUI;
        [SerializeField] Text      promptText;

        [Header("Debug")]
        [SerializeField] bool enableDebugLogs = true;

        #endregion

        #region Private state ------------------------------------------------

        Transform playerTransform;
        bool      playerInRange = false;
        bool      isGenerating  = false;

        #endregion

        // ------------------------------------------------------------------
        #region Unity lifecycle ----------------------------------------------

        void Awake()
        {
            // Garantizar que NO modificamos el asset original de settings
            if (dungeonManager && dungeonManager.generationSettings)
            {
                dungeonManager.generationSettings =
                    Instantiate(dungeonManager.generationSettings);
            }
        }

        /// <summary>
        /// Initializes references and prepares the portal UI.
        /// </summary>
        void Start()
        {
            DebugLog("Portal Start()");

            // Obtener referencias si faltan
            if (!dungeonManager)
            {
                dungeonManager = FindFirstObjectByType<DungeonManager>();
                DebugLog($"DungeonManager found: {dungeonManager}");
            }
            if (dungeonManager) dungeonManager.autoGenerateOnStart = false;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerTransform = player.transform;
            else        DebugLog("WARNING: no GameObject tagged 'Player'");

            if (interactionPromptUI) interactionPromptUI.SetActive(false);
            if (promptText)
                promptText.text = $"Press [{interactionKey}] to enter dungeon";

            // Escuchar fin de spawns
            SpawnSystem.OnSpawningComplete += HandleSpawnComplete;
        }

        void OnDestroy()
        {
            SpawnSystem.OnSpawningComplete -= HandleSpawnComplete;
        }

        void Update()
        {
            if (!playerTransform || isGenerating) return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool  prev = playerInRange;
            playerInRange = dist <= interactionRange;

            if (playerInRange != prev) OnPlayerRangeChanged();

            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                DebugLog($"[{interactionKey}] pressed");
                OnInteract();
            }
        }

        #endregion
        // ------------------------------------------------------------------
        #region Interaction logic -------------------------------------------

        void OnPlayerRangeChanged()
        {
            if (interactionPromptUI)
                interactionPromptUI.SetActive(playerInRange && !isGenerating);
        }

        void OnInteract()
        {
            if (isGenerating) return;
            if (!dungeonManager)
            {
                DebugLog("ERROR: DungeonManager missing");
                return;
            }
            StartCoroutine(GenerateDungeonCoroutine());
        }
        /// <summary>
        /// Ejecuta <paramref name="action"/> dentro de un try/catch y loguea
        /// el paso con la etiqueta <paramref name="label"/>.
        /// No contiene ningún 'yield', por lo que el compilador lo acepta.
        /// </summary>
        void SafeRun(string label, System.Action action)
        {
            try
            {
                DebugLog($"→ {label}");
                action?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ERROR during «{label}»:\n{e}");
            }
        }

        /// <summary>
        /// Executes the full dungeon generation process step by step.
        /// </summary>
        IEnumerator GenerateDungeonCoroutine()
        {
            isGenerating = true;

            // Oculta el prompt mientras genera
            if (interactionPromptUI)
                interactionPromptUI.SetActive(false);

            /* 1️⃣  Semilla aleatoria, fuera de try/catch  */
            if (randomizeSeedOnInteract)
            {
                int seed = Random.Range(int.MinValue, int.MaxValue);
                dungeonManager.SetSeed(seed);         // nuevo método en DungeonManager
                DebugLog($"Seed → {seed}");
            }

            yield return new WaitForSeconds(0.1f);

            /* 2️⃣  Pasos de generación — cada uno con SafeRun + yield fuera */
            SafeRun("Clear",               () => dungeonManager.ClearDungeon());
            yield return new WaitForEndOfFrame();

            SafeRun("Structure",           () => dungeonManager.GenerateMapStructure());
            yield return null;

            SafeRun("Start point",         () => dungeonManager.SelectStartingPoint());
            yield return null;

            SafeRun("Progression",         () => dungeonManager.SetupInitialProgression());
            yield return null;

            SafeRun("Spawns",              () => dungeonManager.PopulateWithEntities());
            yield return null;

            SafeRun("Render",              () => dungeonManager.RenderDungeon());
            yield return null;

            /* 3️⃣  Todo OK → teletransportar */
            DebugLog("✓ Generation complete");
            if (teleportPlayer)
                TeleportPlayerToDungeon();

            isGenerating = false;

            // Reactivar el prompt si el jugador sigue cerca
            if (playerInRange && interactionPromptUI)
                interactionPromptUI.SetActive(true);
        }


        #endregion
        // ------------------------------------------------------------------
        #region Teleport -----------------------------------------------------

        /// <summary>
        /// Moves the player to the generated dungeon entrance or starting room.
        /// </summary>
        void TeleportPlayerToDungeon()
        {
            if (!playerTransform || dungeonManager.DungeonData == null)
            {
                DebugLog("Teleport aborted: no player or dungeon data");
                return;
            }

            Vector3 pos;

            /* Custom point */
            if (playerTeleportPoint)
            {
                pos = playerTeleportPoint.position;
            }
            /* Door marked as entrance */
            else if (dungeonManager.DungeonData.doors.Find(d => d.isEntrance) is
                     { } door)
            {
                pos = new Vector3(door.position.x, 1f, door.position.y);
                float offset = 3f;
                switch (door.orientation)
                {
                    case DungeonSystem.Core.DoorOrientation.Horizontal:
                        pos.z += offset; break;
                    case DungeonSystem.Core.DoorOrientation.Vertical:
                        pos.x += offset; break;
                }
            }
            /* Fallback → centro de startingRoom o del mapa */
            else if (dungeonManager.DungeonData.startingRoom is { } room)
            {
                pos = new Vector3(room.centerPoint.x, 1f, room.centerPoint.y);
            }
            else
            {
                pos = new Vector3(dungeonManager.DungeonData.width  / 2f,
                                  1f,
                                  dungeonManager.DungeonData.height / 2f);
            }

            // Offset global del DungeonManager
            pos += dungeonManager.transform.position;
            playerTransform.position = pos;
            DebugLog($"Player teleported to {pos}");
        }

        #endregion
        // ------------------------------------------------------------------
        #region Spawn event callback -----------------------------------------

        void HandleSpawnComplete(int items, int enemies)
        {
            DebugLog($"[REPORT] Spawns → Items:{items} · Enemies:{enemies}");
        }

        #endregion
        // ------------------------------------------------------------------
        #region Gizmos & Logs ------------------------------------------------

        void DebugLog(string msg)
        {
            if (enableDebugLogs) Debug.Log($"[DungeonPortal] {msg}");
        }

        void OnDrawGizmos()
        {
            Gizmos.color = (playerInRange ? Color.green : Color.yellow);
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (playerTeleportPoint)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(playerTeleportPoint.position, 0.5f);
                Gizmos.DrawLine(transform.position, playerTeleportPoint.position);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, interactionRange);
        }

        #endregion
    }
}
