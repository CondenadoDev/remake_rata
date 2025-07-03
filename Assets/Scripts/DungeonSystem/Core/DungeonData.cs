using System.Collections.Generic;
using UnityEngine;

namespace DungeonSystem.Core
{
    [System.Serializable]
    public class Room
    {
        public Rect bounds;
        public RoomType roomType;
        public GridPosition centerPoint;
        public List<GridPosition> doorPositions = new List<GridPosition>();
        public List<GridPosition> floorTiles = new List<GridPosition>();
        public bool isStartingRoom = false;
        public int distanceFromStart = -1; // Para progresión
        
        
        // Para spawning
        public bool hasBeenPopulated = false;
        public List<GridPosition> spawnPoints = new List<GridPosition>();
        public List<GridPosition> itemSpawnPoints = new List<GridPosition>();
        public List<GridPosition> enemySpawnPoints = new List<GridPosition>();

        public Room(Rect bounds)
        {
            this.bounds = bounds;
            this.centerPoint = new GridPosition(
                Mathf.RoundToInt(bounds.x + bounds.width / 2),
                Mathf.RoundToInt(bounds.y + bounds.height / 2)
            );
            
            // Determinar tipo basado en tamaño
            float area = bounds.width * bounds.height;
            if (area <= 100) roomType = RoomType.SmallRoom;
            else if (area <= 400) roomType = RoomType.MediumRoom;
            else roomType = RoomType.LargeRoom;
        }

        public void PopulateFloorTiles()
        {
            floorTiles.Clear();
            for (int x = Mathf.RoundToInt(bounds.x); x < bounds.x + bounds.width; x++)
            {
                for (int y = Mathf.RoundToInt(bounds.y); y < bounds.y + bounds.height; y++)
                {
                    floorTiles.Add(new GridPosition(x, y));
                }
            }
        }
        
        // Método útil para obtener las habitaciones conectadas
        public List<Room> GetConnectedRooms(DungeonData dungeonData)
        {
            List<Room> connected = new List<Room>();
            foreach (var door in dungeonData.doors)
            {
                if (door.roomA == this && door.roomB != null)
                    connected.Add(door.roomB);
                else if (door.roomB == this && door.roomA != null)
                    connected.Add(door.roomA);
            }
            return connected;
        }
    }

    [System.Serializable]
    public class DungeonDoor
    {
        public bool isEntrance = false;
        public GridPosition position;
        public DoorState state = DoorState.Sealed; // Por defecto todo cerrado
        public string keyId = "";
        public Room roomA;
        public Room roomB;
        public DoorOrientation orientation = DoorOrientation.Horizontal; // Nueva propiedad
        
        public DungeonDoor(GridPosition pos, Room a, Room b)
        {
            position = pos;
            roomA = a;
            roomB = b;
            isEntrance = false;
        }
        
        // Método helper para obtener el ángulo de rotación para el prefab
        public float GetRotationAngle()
        {
            return orientation == DoorOrientation.Vertical ? 90f : 0f;
        }
        
        // Método para obtener la habitación opuesta
        public Room GetOtherRoom(Room currentRoom)
        {
            if (roomA == currentRoom) return roomB;
            if (roomB == currentRoom) return roomA;
            return null;
        }
    }

    public class DungeonData
    {
        public TileType[,] tileMap;
        public List<Room> rooms = new List<Room>();
        public List<DungeonDoor> doors = new List<DungeonDoor>();
        public List<GridPosition> corridors = new List<GridPosition>();
        
        public int width;
        public int height;
        public Room startingRoom;
        
        // Para el sistema de spawning
        public Dictionary<RoomType, List<Room>> roomsByType = new Dictionary<RoomType, List<Room>>();

        public DungeonData(int width, int height)
        {
            this.width = width;
            this.height = height;
            tileMap = new TileType[width, height];
            InitializeRoomDictionary();
        }

        private void InitializeRoomDictionary()
        {
            foreach (RoomType type in System.Enum.GetValues(typeof(RoomType)))
            {
                roomsByType[type] = new List<Room>();
            }
        }

        public void AddRoom(Room room)
        {
            rooms.Add(room);
            roomsByType[room.roomType].Add(room);
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public TileType GetTile(int x, int y)
        {
            if (IsValidPosition(x, y))
                return tileMap[x, y];
            return TileType.Wall;
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (IsValidPosition(x, y))
                tileMap[x, y] = type;
        }

        public Room GetRoomAt(GridPosition pos)
        {
            foreach (Room room in rooms)
            {
                if (room.bounds.Contains(new Vector2(pos.x, pos.y)))
                    return room;
            }
            return null;
        }
        
        // Método para validar conectividad
        public bool AreAllRoomsConnected()
        {
            if (rooms.Count <= 1) return true;
            
            HashSet<Room> visited = new HashSet<Room>();
            Queue<Room> queue = new Queue<Room>();
            
            queue.Enqueue(rooms[0]);
            visited.Add(rooms[0]);
            
            while (queue.Count > 0)
            {
                Room current = queue.Dequeue();
                
                foreach (var door in doors)
                {
                    Room neighbor = door.GetOtherRoom(current);
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            return visited.Count == rooms.Count;
        }
        
        // Obtener todas las puertas de una habitación
        public List<DungeonDoor> GetRoomDoors(Room room)
        {
            return doors.FindAll(door => door.roomA == room || door.roomB == room);
        }
    }
    
    // Enum para orientación de puertas
    public enum DoorOrientation
    {
        Horizontal,  // Puerta en pared horizontal (arriba/abajo)
        Vertical     // Puerta en pared vertical (izquierda/derecha)
    }
}