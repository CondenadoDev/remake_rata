// Assets/DungeonSystem/Settings/SpawnSettings.cs
using System.Collections.Generic;
using UnityEngine;
using DungeonSystem.Core;      // Para RoomType
using DungeonSystem.Settings;  // Para SpawnRuleSet, ItemSpawnData, etc.

namespace DungeonSystem.Settings
{
    /// <summary>
    /// Configuración global de spawns para el generador de mazmorras.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpawnSettings",
        menuName = "Dungeon System/Settings/Spawn Settings",
        order = 3)]
    public class SpawnSettings : ScriptableObject
    {
        // ───────────────────────────────────────────────────────
        [Header("General Spawn Rules")]
        public SpawnRuleSet generalRules = new SpawnRuleSet();

        // ───────────────────────────────────────────────────────
        [Header("Starting Room Settings")]
        public bool allowItemsInStartingRoom  = true;
        public bool allowEnemiesInStartingRoom = false;

        // ───────────────────────────────────────────────────────
        [Header("Item Spawning")]
        public List<ItemSpawnData> itemSpawns = new();

        // ───────────────────────────────────────────────────────
        [Header("Enemy Spawning")]
        public List<EnemySpawnData> enemySpawns = new();

        // ───────────────────────────────────────────────────────
        [Header("Special-Room Config")]
        public RoomTypeSpawnConfig[] roomTypeConfigs = System.Array.Empty<RoomTypeSpawnConfig>();

        // ───────────────────────────────────────────────────────
        [Header("Debug")]
        public bool  showSpawnGizmos = true;
        public Color itemSpawnColor  = Color.green;
        public Color enemySpawnColor = Color.red;
        public Color playerSpawnColor = Color.blue;
    }
}