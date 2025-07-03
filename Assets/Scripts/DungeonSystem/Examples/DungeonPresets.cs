// Examples/DungeonPresets.cs
using UnityEngine;
using DungeonSystem.Settings;
using DungeonSystem.Core;
using System.Collections.Generic;

namespace DungeonSystem.Examples
{
    public static class DungeonPresets
    {
        // Preset para Metroidvania
        public static GenerationSettings CreateMetroidvaniaSettings()
        {
            var settings = ScriptableObject.CreateInstance<GenerationSettings>();
            settings.dungeonWidth = 80;
            settings.dungeonHeight = 60;
            settings.minRoomSize = 6;
            settings.maxRoomSize = 15;
            settings.corridorWidth = 2;
            settings.treasureRoomChance = 0.15f;
            settings.guardRoomChance = 0.25f;
            settings.laboratoryChance = 0.1f;
            settings.bossRoomChance = 0.05f;
            settings.seed = Random.Range(0, 999999);
            return settings;
        }

        public static SpawnSettings CreateMetroidvaniaSpawning()
        {
            var settings = ScriptableObject.CreateInstance<SpawnSettings>();
            settings.generalRules = new SpawnRuleSet
            {
                maxSpawnsPerRoom = 8,
                spawnDensity = 0.4f,
                minDistanceBetweenSpawns = 2f,
                smallRoomMultiplier = 0.6f,
                mediumRoomMultiplier = 1f,
                largeRoomMultiplier = 1.3f
            };

            settings.allowItemsInStartingRoom = true;
            settings.allowEnemiesInStartingRoom = false;

            settings.itemSpawns = new List<ItemSpawnData>
            {
                CreateItemSpawn("key_red", 0.8f, new[] { RoomType.GuardRoom, RoomType.Laboratory }),
                CreateItemSpawn("key_blue", 0.8f, new[] { RoomType.TreasureRoom }),
                CreateItemSpawn("power_up", 1f, new[] { RoomType.TreasureRoom, RoomType.BossRoom }),
                CreateItemSpawn("health_tank", 0.6f, new[] { RoomType.TreasureRoom, RoomType.LargeRoom }),
                CreateItemSpawn("missile_pack", 0.5f, new[] { RoomType.MediumRoom, RoomType.LargeRoom })
            };

            settings.enemySpawns = new List<EnemySpawnData>
            {
                CreateEnemySpawn("drone", 1f, 1f, new[] { RoomType.SmallRoom, RoomType.Corridor }),
                CreateEnemySpawn("guard", 0.8f, 2f, new[] { RoomType.GuardRoom, RoomType.MediumRoom }),
                CreateEnemySpawn("boss", 1f, 5f, new[] { RoomType.BossRoom }, true)
            };

            return settings;
        }

        public static GenerationSettings CreateDungeonCrawlerSettings()
        {
            var settings = ScriptableObject.CreateInstance<GenerationSettings>();
            settings.dungeonWidth = 100;
            settings.dungeonHeight = 100;
            settings.minRoomSize = 8;
            settings.maxRoomSize = 25;
            settings.corridorWidth = 3;
            settings.treasureRoomChance = 0.2f;
            settings.guardRoomChance = 0.3f;
            settings.laboratoryChance = 0.05f;
            settings.bossRoomChance = 0.1f;
            settings.seed = Random.Range(0, 999999);
            return settings;
        }

        public static SpawnSettings CreateDungeonCrawlerSpawning()
        {
            var settings = ScriptableObject.CreateInstance<SpawnSettings>();
            settings.generalRules = new SpawnRuleSet
            {
                maxSpawnsPerRoom = 12,
                spawnDensity = 0.6f,
                minDistanceBetweenSpawns = 1.5f,
                smallRoomMultiplier = 0.8f,
                mediumRoomMultiplier = 1.2f,
                largeRoomMultiplier = 1.8f
            };

            settings.allowItemsInStartingRoom = false;
            settings.allowEnemiesInStartingRoom = false;

            settings.itemSpawns = new List<ItemSpawnData>
            {
                CreateItemSpawn("sword", 0.7f, new[] { RoomType.TreasureRoom, RoomType.GuardRoom }),
                CreateItemSpawn("shield", 0.6f, new[] { RoomType.TreasureRoom, RoomType.GuardRoom }),
                CreateItemSpawn("potion", 0.9f, new[] { RoomType.Laboratory, RoomType.MediumRoom }),
                CreateItemSpawn("gold", 1f, new[] { RoomType.TreasureRoom, RoomType.LargeRoom }),
                CreateItemSpawn("magic_scroll", 0.4f, new[] { RoomType.Laboratory, RoomType.TreasureRoom })
            };

            settings.enemySpawns = new List<EnemySpawnData>
            {
                CreateEnemySpawn("skeleton", 1f, 1f, new[] { RoomType.SmallRoom, RoomType.MediumRoom }),
                CreateEnemySpawn("orc", 0.8f, 2f, new[] { RoomType.GuardRoom, RoomType.LargeRoom }),
                CreateEnemySpawn("wizard", 0.5f, 3f, new[] { RoomType.Laboratory, RoomType.TreasureRoom }),
                CreateEnemySpawn("dragon", 1f, 8f, new[] { RoomType.BossRoom }, true)
            };

            return settings;
        }

        public static GenerationSettings CreateSurvivalHorrorSettings()
        {
            var settings = ScriptableObject.CreateInstance<GenerationSettings>();
            settings.dungeonWidth = 60;
            settings.dungeonHeight = 60;
            settings.minRoomSize = 5;
            settings.maxRoomSize = 12;
            settings.corridorWidth = 2;
            settings.treasureRoomChance = 0.05f;
            settings.guardRoomChance = 0.1f;
            settings.laboratoryChance = 0.2f;
            settings.bossRoomChance = 0.03f;
            settings.seed = Random.Range(0, 999999);
            return settings;
        }

        public static SpawnSettings CreateSurvivalHorrorSpawning()
        {
            var settings = ScriptableObject.CreateInstance<SpawnSettings>();
            settings.generalRules = new SpawnRuleSet
            {
                maxSpawnsPerRoom = 4,
                spawnDensity = 0.2f,
                minDistanceBetweenSpawns = 3f,
                smallRoomMultiplier = 0.5f,
                mediumRoomMultiplier = 0.8f,
                largeRoomMultiplier = 1f
            };

            settings.allowItemsInStartingRoom = true;
            settings.allowEnemiesInStartingRoom = false;

            settings.itemSpawns = new List<ItemSpawnData>
            {
                CreateItemSpawn("flashlight_battery", 0.3f, new[] { RoomType.SmallRoom, RoomType.Laboratory }),
                CreateItemSpawn("med_kit", 0.2f, new[] { RoomType.Laboratory, RoomType.TreasureRoom }),
                CreateItemSpawn("key_card", 0.5f, new[] { RoomType.GuardRoom, RoomType.Laboratory }),
                CreateItemSpawn("ammunition", 0.4f, new[] { RoomType.GuardRoom, RoomType.MediumRoom })
            };

            settings.enemySpawns = new List<EnemySpawnData>
            {
                CreateEnemySpawn("zombie", 0.6f, 2f, new[] { RoomType.SmallRoom, RoomType.MediumRoom }),
                CreateEnemySpawn("stalker", 0.3f, 4f, new[] { RoomType.LargeRoom, RoomType.Corridor }),
                CreateEnemySpawn("nightmare", 1f, 10f, new[] { RoomType.BossRoom }, true)
            };

            return settings;
        }

        private static ItemSpawnData CreateItemSpawn(string id, float weight, RoomType[] allowedRooms)
        {
            return new ItemSpawnData
            {
                itemId = id,
                weight = weight,
                maxPerRoom = 1,
                allowedRoomTypes = new List<RoomType>(allowedRooms).ToArray(),
                minDistanceFromStart = 0,
                maxDistanceFromStart = 999,
                avoidPlayerSpawn = true
            };
        }

        private static EnemySpawnData CreateEnemySpawn(string id, float weight, float difficulty, RoomType[] allowedRooms, bool isBoss = false)
        {
            return new EnemySpawnData
            {
                enemyId = id,
                weight = weight,
                maxPerRoom = isBoss ? 1 : 3,
                difficultyLevel = difficulty,
                isBoss = isBoss,
                allowedRoomTypes = new List<RoomType>(allowedRooms).ToArray(),
                minDistanceFromStart = isBoss ? 4 : 1,
                maxDistanceFromStart = 999,
                minDistanceFromOtherEnemies = isBoss ? 0f : 2f
            };
        }
    }
}
