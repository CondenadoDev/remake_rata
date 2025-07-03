using UnityEngine;
using System.Collections.Generic;
using DungeonSystem.Core;

namespace DungeonSystem.Settings
{
    [System.Serializable]
    public class SpawnRuleSet
    {
        [Header("General Rules")]
        public int maxSpawnsPerRoom = 10;
        [Range(0f, 1f)] public float spawnDensity = 0.3f;
        public float minDistanceBetweenSpawns = 2f;
        
        [Header("Distance from Start Rules")]
        public AnimationCurve difficultyByDistance = AnimationCurve.Linear(0, 0.1f, 1, 1f);
        public AnimationCurve densityByDistance = AnimationCurve.Linear(0, 0.2f, 1, 0.8f);
        
        [Header("Room Size Multipliers")]
        public float smallRoomMultiplier = 0.5f;
        public float mediumRoomMultiplier = 1f;
        public float largeRoomMultiplier = 1.5f;

        public SpawnRuleSet()
        {
            // Curvas por defecto
            difficultyByDistance = AnimationCurve.Linear(0, 0.1f, 1, 1f);
            densityByDistance = AnimationCurve.Linear(0, 0.2f, 1, 0.8f);
        }
    }

    [System.Serializable] 
    public class ItemSpawnData
    {
        [Header("Basic Info")]
        public GameObject prefab;
        public string itemId = "";
        [Range(0f, 2f)] public float weight = 1f;
        public int maxPerRoom = 1;
        
        [Header("Spawn Conditions")]
        public RoomType[] allowedRoomTypes = new RoomType[0];
        public int minDistanceFromStart = 0;
        public int maxDistanceFromStart = 999;
        public bool requiresLineOfSight = false;
        public bool avoidPlayerSpawn = true;

        public List<RoomType> GetAllowedRoomTypes()
        {
            return new List<RoomType>(allowedRoomTypes);
        }
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        [Header("Basic Info")]
        public GameObject prefab;
        public string enemyId = "";
        [Range(0f, 2f)] public float weight = 1f;
        public int maxPerRoom = 1;
        
        [Header("Difficulty")]
        [Range(0.1f, 10f)] public float difficultyLevel = 1f;
        public bool isBoss = false;
        
        [Header("Spawn Conditions")]
        public RoomType[] allowedRoomTypes = new RoomType[0];
        public int minDistanceFromStart = 1; // Nunca en starting room por defecto
        public int maxDistanceFromStart = 999;
        public float minDistanceFromOtherEnemies = 3f;
        public bool requiresGuardPost = false; // Para enemigos que patrullan

        public List<RoomType> GetAllowedRoomTypes()
        {
            return new List<RoomType>(allowedRoomTypes);
        }
    }

    [System.Serializable]
    public class RoomTypeSpawnConfig
    {
        [Header("Room Type")]
        public RoomType roomType;
        
        [Header("Item Rules")]
        public bool guaranteeItem = false;
        public string[] preferredItemIds = new string[0];
        [Range(0f, 3f)] public float itemSpawnMultiplier = 1f;
        
        [Header("Enemy Rules")]
        public bool guaranteeEnemy = false;
        public string[] preferredEnemyIds = new string[0];
        [Range(0f, 3f)] public float enemySpawnMultiplier = 1f;
        
        [Header("Special Rules")]
        public bool isSecure = false; // No spawns, safe room
        public bool isTrap = false;   // Guarantee trap + reward
        public bool requiresKey = false; // Room should be locked

        public List<string> GetPreferredItemIds()
        {
            return new List<string>(preferredItemIds);
        }

        public List<string> GetPreferredEnemyIds()
        {
            return new List<string>(preferredEnemyIds);
        }
    }
}