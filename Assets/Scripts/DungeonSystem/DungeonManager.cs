using UnityEngine;
using DungeonSystem.Core;
using DungeonSystem.Settings;
using DungeonSystem.Spawning;
using DungeonSystem.Rendering;
using DungeonSystem.Progression;
using UnityEngine.Serialization;

namespace DungeonSystem
{
    public class DungeonManager : MonoBehaviour
    {
        //──────────────────────────────────────── Settings
        [Header("Settings")]
        public GenerationSettings  generationSettings;
        [FormerlySerializedAs("renderSettings")] public RenderSettingsDungeon    renderSettingsDungeon;
        public SpawnSettings       spawnSettings;
        public StartingPointCriteria startingPointCriteria;

        //──────────────────────────────────────── Runtime
        [Header("Runtime Data")]
        [SerializeField] private DungeonData dungeonData;
        private BSPNode rootNode;

        //──────────────────────────────────────── Systems
        [Header("Systems")]
        [SerializeField] private SpawnSystem     spawnSystem;
        [SerializeField] private DungeonRenderer dungeonRenderer;

        //──────────────────────────────────────── Debug
        public bool autoGenerateOnStart = true;
        public bool showDebugInfo       = true;

        //──────────────────────────────────────── Properties
        /// <summary>
        /// Acceso público de solo lectura a los datos del dungeon
        /// </summary>
        public DungeonData DungeonData => dungeonData;
        
        /// <summary>
        /// Indica si hay un dungeon generado actualmente
        /// </summary>
        public bool HasDungeon => dungeonData != null && dungeonData.rooms.Count > 0;
        
        /// <summary>
        /// Obtiene el número total de habitaciones
        /// </summary>
        public int RoomCount => dungeonData?.rooms.Count ?? 0;
        
        /// <summary>
        /// Indica si el dungeon está completamente conectado
        /// </summary>
        public bool IsFullyConnected => dungeonData?.AreAllRoomsConnected() ?? false;

        //──────────────────────────────────────── Unity
        private void Start()
        {
            if (autoGenerateOnStart) GenerateCompleteDungeon();
        }

        //──────────────────────────────────────── Public API
        [ContextMenu("Generate Complete Dungeon")]
        public void GenerateCompleteDungeon()
        {
            // Validar settings antes de generar
            if (!ValidateSettings())
            {
                Debug.LogError("Invalid settings - cannot generate dungeon");
                return;
            }

            GenerateMapStructure();
            SelectStartingPoint();
            SetupInitialProgression();
            PopulateWithEntities();
            RenderDungeon();

            // Validación final
            if (!dungeonData.AreAllRoomsConnected())
            {
                Debug.LogError("Generated dungeon has disconnected rooms! Regenerating...");
                GenerateNewSeed();
            }
            else
            {
                Debug.Log($"Dungeon generated successfully with seed: {generationSettings.seed}");
                Debug.Log($"Rooms: {dungeonData.rooms.Count}, Doors: {dungeonData.doors.Count}, Fully connected: {IsFullyConnected}");
            }
        }

        /// <summary>
        /// Coroutine version of <see cref="GenerateCompleteDungeon"/> to avoid
        /// frame stalls when generating at runtime.
        /// </summary>
        public System.Collections.IEnumerator GenerateCompleteDungeonAsync()
        {
            if (!ValidateSettings())
            {
                Debug.LogError("Invalid settings - cannot generate dungeon");
                yield break;
            }

            GenerateMapStructure();
            yield return null;

            SelectStartingPoint();
            yield return null;

            SetupInitialProgression();
            yield return null;

            PopulateWithEntities();
            yield return null;

            RenderDungeon();
            yield return null;

            if (!dungeonData.AreAllRoomsConnected())
            {
                Debug.LogError("Generated dungeon has disconnected rooms! Regenerating...");
                GenerateNewSeed();
            }
            else
            {
                Debug.Log($"Dungeon generated successfully with seed: {generationSettings.seed}");
                Debug.Log($"Rooms: {dungeonData.rooms.Count}, Doors: {dungeonData.doors.Count}, Fully connected: {IsFullyConnected}");
            }
        }

        public void GenerateMapStructure()
        {
            (dungeonData, rootNode) = BSPGenerator.GenerateDungeon(generationSettings);
            RoomConnector.ConnectRooms(rootNode, dungeonData, generationSettings);
        }

        public void SelectStartingPoint()
        {
            if (dungeonData == null)
            {
                Debug.LogError("No dungeon data");
                return;
            }
            StartingPointSystem.SelectStartingRoom(dungeonData,startingPointCriteria);
        }

        public void SetupInitialProgression()
        {
            if (dungeonData?.startingRoom == null)
            {
                Debug.LogError("No starting room");
                return;
            }
            StartingPointSystem.InitializeDoorStates(dungeonData);
        }

        public void PopulateWithEntities()
        {
            if (dungeonData == null || spawnSettings == null)
            {
                Debug.LogError("Missing data or settings");
                return;
            }

            if (spawnSystem == null)
            {
                var go = new GameObject("SpawnSystem");
                go.transform.SetParent(transform);
                spawnSystem = go.AddComponent<SpawnSystem>();
            }

            spawnSystem.Initialize(spawnSettings);
            spawnSystem.PopulateDungeon(dungeonData);
        }

        public void RenderDungeon()
        {
            if (dungeonData == null || renderSettingsDungeon == null)
            {
                Debug.LogError("Missing data or settings");
                return;
            }

            if (dungeonRenderer == null)
            {
                var go = new GameObject("DungeonRenderer");
                go.transform.SetParent(transform);
                dungeonRenderer = go.AddComponent<DungeonRenderer>();
            }

            dungeonRenderer.Initialize(renderSettingsDungeon);
            dungeonRenderer.RenderDungeon(dungeonData);
        }

        public void GenerateNewSeed()
        {
            generationSettings.seed = Random.Range(0,999999);
            GenerateCompleteDungeon();
        }

        public void SetSeed(int seed)
        {
            generationSettings.seed = seed;
            Random.InitState(seed);
        }
        
        public void ClearDungeon()
        {
            spawnSystem?.ClearPreviousSpawns();
            dungeonRenderer?.ClearRendering();
            dungeonData = null;
        }

        /// <summary>
        /// Valida que todos los settings necesarios estén asignados
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;
            
            if (generationSettings == null)
            {
                Debug.LogError("Generation Settings missing!");
                isValid = false;
            }
            
            if (renderSettingsDungeon == null)
            {
                Debug.LogError("Render Settings missing!");
                isValid = false;
            }
            
            if (spawnSettings == null)
            {
                Debug.LogError("Spawn Settings missing!");
                isValid = false;
            }
            
            if (startingPointCriteria == null)
            {
                Debug.LogWarning("Starting Point Criteria missing - using defaults");
                // Crear uno temporal con valores por defecto
                startingPointCriteria = ScriptableObject.CreateInstance<StartingPointCriteria>();
            }
            
            return isValid;
        }

        /// <summary>
        /// Obtiene información de depuración del dungeon actual
        /// </summary>
        public string GetDebugInfo()
        {
            if (!HasDungeon) return "No dungeon generated";
            
            return $"Dungeon Info:\n" +
                   $"- Seed: {generationSettings.seed}\n" +
                   $"- Size: {dungeonData.width}x{dungeonData.height}\n" +
                   $"- Rooms: {dungeonData.rooms.Count}\n" +
                   $"- Doors: {dungeonData.doors.Count}\n" +
                   $"- Corridors: {dungeonData.corridors.Count}\n" +
                   $"- Starting Room: {dungeonData.startingRoom?.centerPoint}\n" +
                   $"- Fully Connected: {IsFullyConnected}\n" +
                   $"- Room Types: {GetRoomTypeDistribution()}";
        }
        
        private string GetRoomTypeDistribution()
        {
            if (dungeonData == null) return "N/A";
            
            string distribution = "";
            foreach (var kvp in dungeonData.roomsByType)
            {
                if (kvp.Value.Count > 0)
                    distribution += $"\n  - {kvp.Key}: {kvp.Value.Count}";
            }
            return distribution;
        }

        //──────────────────────────────────────── Helpers

        //──────────────────────────────────────── Gizmos
        private void OnDrawGizmos()
        {
            if (!showDebugInfo || dungeonData==null || !generationSettings.showGizmos) return;
            
            // Dibujar habitaciones
            foreach (var room in dungeonData.rooms)
            {
                Gizmos.color = room.isStartingRoom ? generationSettings.startingRoomColor : generationSettings.floorColor;
                Vector3 roomCenter = new Vector3(room.centerPoint.x, 0, room.centerPoint.y);
                Vector3 roomSize = new Vector3(room.bounds.width - 1, 0.1f, room.bounds.height - 1);
                Gizmos.DrawCube(roomCenter, roomSize);
                
                // Dibujar número de habitación
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(roomCenter + Vector3.up, $"{room.roomType}\nDist: {room.distanceFromStart}");
                #endif
            }
            
            // Dibujar puertas
            foreach (var door in dungeonData.doors)
            {
                Gizmos.color = generationSettings.doorColor;
                Vector3 doorPos = new Vector3(door.position.x, 0.5f, door.position.y);
                Gizmos.DrawWireCube(doorPos, Vector3.one * 0.8f);
            }
            
            // Dibujar corredores
            Gizmos.color = generationSettings.corridorColor;
            foreach (var corridor in dungeonData.corridors)
            {
                Vector3 corridorPos = new Vector3(corridor.x, 0, corridor.y);
                Gizmos.DrawCube(corridorPos, new Vector3(0.9f, 0.05f, 0.9f));
            }
        }
    }
}