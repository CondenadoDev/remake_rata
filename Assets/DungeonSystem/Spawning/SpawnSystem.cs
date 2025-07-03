using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonSystem.Core;
using DungeonSystem.Settings;

namespace DungeonSystem.Spawning
{
    public class SpawnSystem : MonoBehaviour
    {
        public static event System.Action<int,int> OnSpawningComplete;

        [Header("Settings")]
        public SpawnSettings spawnSettings;
        
        [Header("Runtime Data")]
        public List<SpawnedEntity> spawnedItems = new List<SpawnedEntity>();
        public List<SpawnedEntity> spawnedEnemies = new List<SpawnedEntity>();
        
        private DungeonData dungeonData;
        private Dictionary<RoomType, RoomTypeSpawnConfig> roomConfigs;

        public void Initialize(SpawnSettings settings)
        {
            spawnSettings = settings;
            BuildRoomConfigDictionary();
        }

        private void BuildRoomConfigDictionary()
        {
            roomConfigs = new Dictionary<RoomType, RoomTypeSpawnConfig>();
            
            if (spawnSettings.roomTypeConfigs != null)
            {
                foreach (var config in spawnSettings.roomTypeConfigs)
                {
                    roomConfigs[config.roomType] = config;
                }
            }
        }

        public void PopulateDungeon(DungeonData data)
        {
            Debug.Log($"[SpawnSystem] → Comienzo de población: {data.rooms.Count} salas");
            dungeonData = data;
            ClearPreviousSpawns();
            
            // Fase 1: Calcular distancias desde starting room
            CalculateRoomDistances();
            
            // Fase 2: Asignar tipos especiales a habitaciones
            AssignSpecialRoomTypes();

            // Fase 3: Spawn items críticos primero (llaves, objetivos)
            SpawnCriticalItems();
            
            // Fase 4: Spawn enemigos por tipo de habitación
            SpawnEnemiesByRoomType();
            Debug.Log($"[SpawnSystem] Fase 4 – Enemigos colocados: {spawnedEnemies.Count}");

            // Fase 5: Spawn items normales por tipo de habitación
            SpawnItemsByRoomType();
            Debug.Log($"[SpawnSystem] Fase 5 – Ítems colocados: {spawnedItems.Count}");

            // Fase 6: Validar y balancear spawns
            ValidateAndBalance();
            
            Debug.Log($"Spawning complete: {spawnedItems.Count} items, {spawnedEnemies.Count} enemies");

            // Notify listeners
            OnSpawningComplete?.Invoke(spawnedItems.Count, spawnedEnemies.Count);
        }

        private void CalculateRoomDistances()
        {
            if (dungeonData.startingRoom == null) return;

            // BFS para calcular distancias
            Queue<Room> queue = new Queue<Room>();
            HashSet<Room> visited = new HashSet<Room>();
            
            dungeonData.startingRoom.distanceFromStart = 0;
            queue.Enqueue(dungeonData.startingRoom);
            visited.Add(dungeonData.startingRoom);

            while (queue.Count > 0)
            {
                Room current = queue.Dequeue();
                
                // Encontrar habitaciones conectadas a través de puertas
                foreach (var door in dungeonData.doors)
                {
                    Room neighbor = null;
                    if (door.roomA == current) neighbor = door.roomB;
                    else if (door.roomB == current) neighbor = door.roomA;
                    
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        neighbor.distanceFromStart = current.distanceFromStart + 1;
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        private void AssignSpecialRoomTypes()
        {
            var availableRooms = dungeonData.rooms.Where(r => r.roomType != RoomType.StartingRoom).ToList();
            
            // Asignar treasure rooms a habitaciones lejanas y grandes
            var treasureCandidates = availableRooms
                .Where(r => r.distanceFromStart >= 3 && r.roomType == RoomType.LargeRoom)
                .OrderByDescending(r => r.distanceFromStart)
                .Take(Mathf.RoundToInt(availableRooms.Count * spawnSettings.generalRules.spawnDensity * 0.2f));
            
            foreach (var room in treasureCandidates)
            {
                room.roomType = RoomType.TreasureRoom;
                dungeonData.roomsByType[RoomType.LargeRoom].Remove(room);
                dungeonData.roomsByType[RoomType.TreasureRoom].Add(room);
            }

            // Asignar guard rooms cerca del inicio
            var guardCandidates = availableRooms
                .Where(r => r.distanceFromStart >= 1 && r.distanceFromStart <= 3 && 
                           r.roomType == RoomType.MediumRoom)
                .Take(Mathf.RoundToInt(availableRooms.Count * 0.3f));
            
            foreach (var room in guardCandidates)
            {
                room.roomType = RoomType.GuardRoom;
                dungeonData.roomsByType[RoomType.MediumRoom].Remove(room);
                dungeonData.roomsByType[RoomType.GuardRoom].Add(room);
            }

            // Asignar boss room a la habitación más lejana y grande
            var bossCandidate = availableRooms
                .Where(r => r.roomType == RoomType.LargeRoom)
                .OrderByDescending(r => r.distanceFromStart)
                .FirstOrDefault();
            
            if (bossCandidate != null)
            {
                bossCandidate.roomType = RoomType.BossRoom;
                dungeonData.roomsByType[RoomType.LargeRoom].Remove(bossCandidate);
                dungeonData.roomsByType[RoomType.BossRoom].Add(bossCandidate);
            }
        }

        private void SpawnCriticalItems()
        {
            // Spawn llaves en orden de distancia (cerca las primeras, lejos las últimas)
            var keyItems = spawnSettings.itemSpawns.Where(item => item.itemId.Contains("key")).ToList();
            var roomsForKeys = dungeonData.rooms
                .Where(r => r.roomType != RoomType.StartingRoom && r.distanceFromStart > 0)
                .OrderBy(r => r.distanceFromStart)
                .ToList();

            for (int i = 0; i < keyItems.Count && i < roomsForKeys.Count; i++)
            {
                var room = roomsForKeys[i * (roomsForKeys.Count / keyItems.Count)];
                SpawnItemInRoom(keyItems[i], room, true);
            }
        }

        private void SpawnEnemiesByRoomType()
        {
            foreach (var roomTypeGroup in dungeonData.roomsByType)
            {
                RoomType roomType = roomTypeGroup.Key;
                List<Room> rooms = roomTypeGroup.Value;

                if (roomType == RoomType.StartingRoom && !spawnSettings.allowEnemiesInStartingRoom)
                    continue;

                foreach (var room in rooms)
                {
                    SpawnEnemiesInRoom(room);
                }
            }
        }

        private void SpawnEnemiesInRoom(Room room)
        {
            // Obtener configuración específica para este tipo de habitación
            roomConfigs.TryGetValue(room.roomType, out RoomTypeSpawnConfig config);
            
            // Filtrar enemigos apropiados para esta habitación
            var appropriateEnemies = spawnSettings.enemySpawns.Where(enemy => 
                enemy.GetAllowedRoomTypes().Contains(room.roomType) &&
                room.distanceFromStart >= enemy.minDistanceFromStart &&
                room.distanceFromStart <= enemy.maxDistanceFromStart
            ).ToList();

            if (appropriateEnemies.Count == 0) return;

            // Calcular número de enemigos basado en tipo de habitación
            int enemyCount = CalculateEnemyCount(room, config);
            
            // Aplicar lógica específica por tipo de habitación
            switch (room.roomType)
            {
                case RoomType.GuardRoom:
                    SpawnGuardRoomEnemies(room, appropriateEnemies, enemyCount);
                    break;
                    
                case RoomType.BossRoom:
                    SpawnBossRoomEnemies(room, appropriateEnemies);
                    break;
                    
                case RoomType.TreasureRoom:
                    SpawnTreasureRoomEnemies(room, appropriateEnemies, enemyCount);
                    break;
                    
                default:
                    SpawnStandardEnemies(room, appropriateEnemies, enemyCount);
                    break;
            }
        }

        private void SpawnGuardRoomEnemies(Room room, List<EnemySpawnData> enemies, int count)
        {
            // Guardias prefieren estar cerca de puertas
            var doorPositions = room.doorPositions;
            var guardEnemies = enemies.Where(e => e.enemyId.Contains("guard") || e.enemyId.Contains("soldier")).ToList();
            
            if (guardEnemies.Count == 0) guardEnemies = enemies;

            for (int i = 0; i < count && i < doorPositions.Count; i++)
            {
                var enemy = SelectWeightedRandom(guardEnemies);
                var spawnPos = FindSpawnPositionNear(room, doorPositions[i], 3f);
                if (spawnPos != GridPosition.zero)
                {
                    SpawnEnemyAtPosition(enemy, room, spawnPos);
                }
            }
        }

        private void SpawnBossRoomEnemies(Room room, List<EnemySpawnData> enemies)
        {
            // Solo un jefe en el centro
            var bosses = enemies.Where(e => e.isBoss).ToList();
            if (bosses.Count == 0) bosses = enemies.OrderByDescending(e => e.difficultyLevel).Take(1).ToList();

            if (bosses.Count > 0)
            {
                var boss = SelectWeightedRandom(bosses);
                SpawnEnemyAtPosition(boss, room, room.centerPoint);
            }
        }

        private void SpawnTreasureRoomEnemies(Room room, List<EnemySpawnData> enemies, int count)
        {
            // Enemigos guardianes distribuidos alrededor del tesoro (centro)
            var guardians = enemies.Where(e => e.enemyId.Contains("guardian") || e.difficultyLevel > 1.5f).ToList();
            if (guardians.Count == 0) guardians = enemies;

            var positions = GetCircularPositions(room.centerPoint, 4f, count);
            foreach (var pos in positions)
            {
                if (IsValidSpawnPosition(room, pos))
                {
                    var enemy = SelectWeightedRandom(guardians);
                    SpawnEnemyAtPosition(enemy, room, pos);
                }
            }
        }

        private void SpawnStandardEnemies(Room room, List<EnemySpawnData> enemies, int count)
        {
            // Distribución aleatoria pero respetando distancias mínimas
            for (int i = 0; i < count; i++)
            {
                var enemy = SelectWeightedRandom(enemies);
                var spawnPos = FindRandomSpawnPosition(room);
                if (spawnPos != GridPosition.zero)
                {
                    SpawnEnemyAtPosition(enemy, room, spawnPos);
                }
            }
        }

        private void SpawnItemsByRoomType()
        {
            foreach (var roomTypeGroup in dungeonData.roomsByType)
            {
                RoomType roomType = roomTypeGroup.Key;
                List<Room> rooms = roomTypeGroup.Value;

                if (roomType == RoomType.StartingRoom && !spawnSettings.allowItemsInStartingRoom)
                    continue;

                foreach (var room in rooms)
                {
                    SpawnItemsInRoom(room);
                }
            }
        }

        private void SpawnItemsInRoom(Room room)
        {
            // Obtener configuración específica
            roomConfigs.TryGetValue(room.roomType, out RoomTypeSpawnConfig config);
            
            // Filtrar items apropiados
            var appropriateItems = spawnSettings.itemSpawns.Where(item => 
                item.allowedRoomTypes.Contains(room.roomType) &&
                room.distanceFromStart >= item.minDistanceFromStart &&
                room.distanceFromStart <= item.maxDistanceFromStart
            ).ToList();

            if (appropriateItems.Count == 0) return;

            // Aplicar lógica específica por tipo de habitación
            switch (room.roomType)
            {
                case RoomType.TreasureRoom:
                    SpawnTreasureItems(room, appropriateItems);
                    break;
                    
                case RoomType.Laboratory:
                    SpawnLaboratoryItems(room, appropriateItems);
                    break;
                    
                case RoomType.GuardRoom:
                    SpawnGuardRoomItems(room, appropriateItems);
                    break;
                    
                default:
                    SpawnStandardItems(room, appropriateItems);
                    break;
            }
        }

        private void SpawnTreasureItems(Room room, List<ItemSpawnData> items)
        {
            // Tesoros valiosos en el centro, items menores alrededor
            var treasures = items.Where(i => i.itemId.Contains("treasure") || i.itemId.Contains("rare")).ToList();
            var regular = items.Where(i => !treasures.Contains(i)).ToList();

            // Tesoro principal en el centro
            if (treasures.Count > 0)
            {
                var mainTreasure = SelectWeightedRandom(treasures);
                SpawnItemAtPosition(mainTreasure, room, room.centerPoint);
            }

            // Items menores en las esquinas
            var corners = GetRoomCorners(room);
            foreach (var corner in corners.Take(regular.Count))
            {
                var item = SelectWeightedRandom(regular);
                SpawnItemAtPosition(item, room, corner);
            }
        }

        private void SpawnLaboratoryItems(Room room, List<ItemSpawnData> items)
        {
            // Pociones, libros, ingredientes distribuidos por los bordes
            var labItems = items.Where(i => 
                i.itemId.Contains("potion") || 
                i.itemId.Contains("book") || 
                i.itemId.Contains("ingredient")
            ).ToList();

            if (labItems.Count == 0) labItems = items;

            var edgePositions = GetRoomEdgePositions(room, 2);
            foreach (var pos in edgePositions.Take(labItems.Count))
            {
                var item = SelectWeightedRandom(labItems);
                SpawnItemAtPosition(item, room, pos);
            }
        }

        private void SpawnGuardRoomItems(Room room, List<ItemSpawnData> items)
        {
            // Armas y armaduras cerca de las puertas
            var equipment = items.Where(i => 
                i.itemId.Contains("weapon") || 
                i.itemId.Contains("armor") || 
                i.itemId.Contains("shield")
            ).ToList();

            if (equipment.Count == 0) equipment = items;

            foreach (var doorPos in room.doorPositions)
            {
                var nearDoorPos = FindSpawnPositionNear(room, doorPos, 2f);
                if (nearDoorPos != GridPosition.zero)
                {
                    var item = SelectWeightedRandom(equipment);
                    SpawnItemAtPosition(item, room, nearDoorPos);
                }
            }
        }

        private void SpawnStandardItems(Room room, List<ItemSpawnData> items)
        {
            int itemCount = CalculateItemCount(room);
            
            for (int i = 0; i < itemCount; i++)
            {
                var item = SelectWeightedRandom(items);
                var spawnPos = FindRandomSpawnPosition(room);
                if (spawnPos != GridPosition.zero)
                {
                    SpawnItemAtPosition(item, room, spawnPos);
                }
            }
        }

        // Métodos auxiliares de utilidad
        private int CalculateEnemyCount(Room room, RoomTypeSpawnConfig config)
        {
            float baseCount = Mathf.RoundToInt(room.bounds.width * room.bounds.height * 0.02f);
            
            // Aplicar multiplicador por tipo de habitación
            switch (room.roomType)
            {
                case RoomType.SmallRoom: baseCount *= spawnSettings.generalRules.smallRoomMultiplier; break;
                case RoomType.MediumRoom: baseCount *= spawnSettings.generalRules.mediumRoomMultiplier; break;
                case RoomType.LargeRoom: baseCount *= spawnSettings.generalRules.largeRoomMultiplier; break;
                case RoomType.GuardRoom: baseCount *= 1.5f; break;
                case RoomType.BossRoom: return 1; // Solo jefe
                case RoomType.TreasureRoom: baseCount *= 0.8f; break;
            }

            if (config != null)
                baseCount *= config.enemySpawnMultiplier;

            return Mathf.Clamp(Mathf.RoundToInt(baseCount), 0, spawnSettings.generalRules.maxSpawnsPerRoom);
        }

        private int CalculateItemCount(Room room)
        {
            float baseCount = Mathf.RoundToInt(room.bounds.width * room.bounds.height * 0.015f);
            
            switch (room.roomType)
            {
                case RoomType.SmallRoom: baseCount *= spawnSettings.generalRules.smallRoomMultiplier; break;
                case RoomType.MediumRoom: baseCount *= spawnSettings.generalRules.mediumRoomMultiplier; break;
                case RoomType.LargeRoom: baseCount *= spawnSettings.generalRules.largeRoomMultiplier; break;
                case RoomType.TreasureRoom: baseCount *= 2f; break;
                case RoomType.Laboratory: baseCount *= 1.5f; break;
            }

            return Mathf.Clamp(Mathf.RoundToInt(baseCount), 1, spawnSettings.generalRules.maxSpawnsPerRoom);
        }

        private T SelectWeightedRandom<T>(List<T> items) where T : class
        {
            if (items.Count == 0) return null;
            
            float totalWeight = 0f;
            if (typeof(T) == typeof(ItemSpawnData))
                totalWeight = items.Cast<ItemSpawnData>().Sum(i => i.weight);
            else if (typeof(T) == typeof(EnemySpawnData))
                totalWeight = items.Cast<EnemySpawnData>().Sum(e => e.weight);
            else
                return items[Random.Range(0, items.Count)];

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in items)
            {
                if (typeof(T) == typeof(ItemSpawnData))
                    currentWeight += (item as ItemSpawnData).weight;
                else if (typeof(T) == typeof(EnemySpawnData))
                    currentWeight += (item as EnemySpawnData).weight;

                if (randomValue <= currentWeight)
                    return item;
            }

            return items.LastOrDefault();
        }

        private GridPosition FindRandomSpawnPosition(Room room)
        {
            int attempts = 20;
            while (attempts > 0)
            {
                int x = Random.Range(Mathf.RoundToInt(room.bounds.x + 1), 
                                   Mathf.RoundToInt(room.bounds.x + room.bounds.width - 1));
                int y = Random.Range(Mathf.RoundToInt(room.bounds.y + 1), 
                                   Mathf.RoundToInt(room.bounds.y + room.bounds.height - 1));

                GridPosition pos = new GridPosition(x, y);
                if (IsValidSpawnPosition(room, pos))
                    return pos;

                attempts--;
            }
            return GridPosition.zero;
        }

        private GridPosition FindSpawnPositionNear(Room room, GridPosition target, float maxDistance)
        {
            int attempts = 15;
            while (attempts > 0)
            {
                int offsetX = Random.Range(-Mathf.RoundToInt(maxDistance), Mathf.RoundToInt(maxDistance) + 1);
                int offsetY = Random.Range(-Mathf.RoundToInt(maxDistance), Mathf.RoundToInt(maxDistance) + 1);

                GridPosition pos = new GridPosition(target.x + offsetX, target.y + offsetY);
                
                if (room.bounds.Contains(new Vector2(pos.x, pos.y)) && IsValidSpawnPosition(room, pos))
                    return pos;

                attempts--;
            }
            return GridPosition.zero;
        }

        private bool IsValidSpawnPosition(Room room, GridPosition pos)
        {
            // Verificar que esté dentro de la habitación
            if (!room.bounds.Contains(new Vector2(pos.x, pos.y))) return false;

            // Verificar distancia mínima de otros spawns
            foreach (var spawn in spawnedItems.Concat(spawnedEnemies))
            {
                float distance = Vector2.Distance(new Vector2(pos.x, pos.y), 
                                                new Vector2(spawn.position.x, spawn.position.y));
                if (distance < spawnSettings.generalRules.minDistanceBetweenSpawns)
                    return false;
            }

            return true;
        }

        private List<GridPosition> GetCircularPositions(GridPosition center, float radius, int count)
        {
            List<GridPosition> positions = new List<GridPosition>();
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                int x = Mathf.RoundToInt(center.x + Mathf.Cos(angle) * radius);
                int y = Mathf.RoundToInt(center.y + Mathf.Sin(angle) * radius);
                positions.Add(new GridPosition(x, y));
            }

            return positions;
        }

        private List<GridPosition> GetRoomCorners(Room room)
        {
            return new List<GridPosition>
            {
                new GridPosition(Mathf.RoundToInt(room.bounds.x + 1), Mathf.RoundToInt(room.bounds.y + 1)),
                new GridPosition(Mathf.RoundToInt(room.bounds.x + room.bounds.width - 2), Mathf.RoundToInt(room.bounds.y + 1)),
                new GridPosition(Mathf.RoundToInt(room.bounds.x + room.bounds.width - 2), Mathf.RoundToInt(room.bounds.y + room.bounds.height - 2)),
                new GridPosition(Mathf.RoundToInt(room.bounds.x + 1), Mathf.RoundToInt(room.bounds.y + room.bounds.height - 2))
            };
        }

        private List<GridPosition> GetRoomEdgePositions(Room room, int spacing)
        {
            List<GridPosition> positions = new List<GridPosition>();
            
            // Borde superior e inferior
            for (int x = Mathf.RoundToInt(room.bounds.x + spacing); x < room.bounds.x + room.bounds.width - spacing; x += spacing)
            {
                positions.Add(new GridPosition(x, Mathf.RoundToInt(room.bounds.y + 1)));
                positions.Add(new GridPosition(x, Mathf.RoundToInt(room.bounds.y + room.bounds.height - 2)));
            }
            
            // Bordes izquierdo y derecho
            for (int y = Mathf.RoundToInt(room.bounds.y + spacing); y < room.bounds.y + room.bounds.height - spacing; y += spacing)
            {
                positions.Add(new GridPosition(Mathf.RoundToInt(room.bounds.x + 1), y));
                positions.Add(new GridPosition(Mathf.RoundToInt(room.bounds.x + room.bounds.width - 2), y));
            }
            
            return positions;
        }

        private void SpawnItemAtPosition(ItemSpawnData itemData, Room room, GridPosition position)
        {
            SpawnItemInRoom(itemData, room, false, position);
        }

        private void SpawnEnemyAtPosition(EnemySpawnData enemyData, Room room, GridPosition position)
        {
            if (enemyData?.prefab == null) return;

            Vector3 worldPos = new Vector3(position.x, 0, position.y);
            GameObject instance = Instantiate(enemyData.prefab, worldPos, Quaternion.identity);
            
            SpawnedEntity spawn = new SpawnedEntity
            {
                instance = instance,
                entityId = enemyData.enemyId,
                position = position,
                room = room,
                isEnemy = true
            };
            
            spawnedEnemies.Add(spawn);
            room.enemySpawnPoints.Add(position);
        }

        private void SpawnItemInRoom(ItemSpawnData itemData, Room room, bool isCritical, GridPosition? forcePosition = null)
        {
            if (itemData?.prefab == null) return;

            GridPosition spawnPos = forcePosition ?? FindRandomSpawnPosition(room);
            if (spawnPos == GridPosition.zero) return;

            Vector3 worldPos = new Vector3(spawnPos.x, 0, spawnPos.y);
            GameObject instance = Instantiate(itemData.prefab, worldPos, Quaternion.identity);
            
            SpawnedEntity spawn = new SpawnedEntity
            {
                instance = instance,
                entityId = itemData.itemId,
                position = spawnPos,
                room = room,
                isEnemy = false,
                isCritical = isCritical
            };
            
            spawnedItems.Add(spawn);
            room.itemSpawnPoints.Add(spawnPos);
        }

        private void ValidateAndBalance()
        {
            // Verificar que hay al menos una ruta válida al objetivo
            // Balancear densidad de spawns
            // Validar que no hay clustering excesivo
        }

        public void ClearPreviousSpawns()
        {
            foreach (var spawn in spawnedItems.Concat(spawnedEnemies))
            {
                if (spawn.instance != null)
                    DestroyImmediate(spawn.instance);
            }
            
            spawnedItems.Clear();
            spawnedEnemies.Clear();
        }

        void OnDrawGizmos()
        {
            if (!spawnSettings || !spawnSettings.showSpawnGizmos) return;

            foreach (var spawn in spawnedItems)
            {
                Gizmos.color = spawnSettings.itemSpawnColor;
                Gizmos.DrawWireSphere(new Vector3(spawn.position.x, 0.5f, spawn.position.y), 0.3f);
            }

            foreach (var spawn in spawnedEnemies)
            {
                Gizmos.color = spawnSettings.enemySpawnColor;
                Gizmos.DrawWireCube(new Vector3(spawn.position.x, 1f, spawn.position.y), Vector3.one * 0.8f);
            }
        }
    }

    [System.Serializable]
    public class SpawnedEntity
    {
        public GameObject instance;
        public string entityId;
        public GridPosition position;
        public Room room;
        public bool isEnemy;
        public bool isCritical;
    }
}