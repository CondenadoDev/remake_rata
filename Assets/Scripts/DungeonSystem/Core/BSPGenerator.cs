// Core/BSPGenerator.cs
using UnityEngine;
using DungeonSystem.Settings;

namespace DungeonSystem.Core
{
    public static class BSPGenerator
    {
        public static (DungeonData data, BSPNode rootNode) GenerateDungeon(GenerationSettings settings)
        {
            Random.InitState(settings.seed);

            DungeonData data = new DungeonData(settings.dungeonWidth, settings.dungeonHeight);

            // Inicializar mapa con muros
            InitializeMap(data);

            // Crear y dividir BSP
            BSPNode rootNode = new BSPNode(new Rect(0, 0, settings.dungeonWidth, settings.dungeonHeight));
            SplitNode(rootNode, 0, settings);

            // Crear habitaciones
            CreateRoomsFromBSP(rootNode, data, settings);

            return (data, rootNode);
        }

        private static void InitializeMap(DungeonData data)
        {
            for (int x = 0; x < data.width; x++)
            {
                for (int y = 0; y < data.height; y++)
                {
                    data.SetTile(x, y, TileType.Wall);
                }
            }
        }

        private static void SplitNode(BSPNode node, int depth, GenerationSettings settings)
        {
            if (depth > 6 || 
                node.bounds.width < settings.minRoomSize * 2 || 
                node.bounds.height < settings.minRoomSize * 2)
                return;
            
            bool splitHorizontal = Random.Range(0f, 1f) > 0.5f;
            
            // Forzar división si hay mucha diferencia de aspecto
            if (node.bounds.width > node.bounds.height && node.bounds.width / node.bounds.height >= 1.25f)
                splitHorizontal = false;
            else if (node.bounds.height > node.bounds.width && node.bounds.height / node.bounds.width >= 1.25f)
                splitHorizontal = true;
            
            if (splitHorizontal)
            {
                int splitY = Random.Range(settings.minRoomSize, 
                                        Mathf.RoundToInt(node.bounds.height - settings.minRoomSize));
                
                node.leftChild = new BSPNode(new Rect(node.bounds.x, node.bounds.y, node.bounds.width, splitY));
                node.rightChild = new BSPNode(new Rect(node.bounds.x, node.bounds.y + splitY, 
                                                     node.bounds.width, node.bounds.height - splitY));
            }
            else
            {
                int splitX = Random.Range(settings.minRoomSize, 
                                        Mathf.RoundToInt(node.bounds.width - settings.minRoomSize));
                
                node.leftChild = new BSPNode(new Rect(node.bounds.x, node.bounds.y, splitX, node.bounds.height));
                node.rightChild = new BSPNode(new Rect(node.bounds.x + splitX, node.bounds.y, 
                                                     node.bounds.width - splitX, node.bounds.height));
            }
            
            SplitNode(node.leftChild, depth + 1, settings);
            SplitNode(node.rightChild, depth + 1, settings);
        }

        private static void CreateRoomsFromBSP(BSPNode node, DungeonData data, GenerationSettings settings)
        {
            if (node.isLeaf)
            {
                // Padding: separa la habitación del borde del leaf
                int padding = 2; // prueba con 2, puedes aumentar si quieres rooms más separadas

                int maxRoomWidth = Mathf.Max(settings.minRoomSize, Mathf.RoundToInt(node.bounds.width - padding * 2));
                int maxRoomHeight = Mathf.Max(settings.minRoomSize, Mathf.RoundToInt(node.bounds.height - padding * 2));

                if (maxRoomWidth < settings.minRoomSize || maxRoomHeight < settings.minRoomSize)
                    return; // No cabe una room decente, descártalo

                int roomWidth = Random.Range(settings.minRoomSize, maxRoomWidth + 1);
                int roomHeight = Random.Range(settings.minRoomSize, maxRoomHeight + 1);

                // Room centrada en el nodo (más estético)
                int roomX = Mathf.RoundToInt(node.bounds.x) + padding + Random.Range(0, (int)(node.bounds.width - roomWidth - padding * 2 + 1));
                int roomY = Mathf.RoundToInt(node.bounds.y) + padding + Random.Range(0, (int)(node.bounds.height - roomHeight - padding * 2 + 1));

                Rect roomBounds = new Rect(roomX, roomY, roomWidth, roomHeight);
                Room room = new Room(roomBounds);
                room.PopulateFloorTiles();

                node.room = room;
                data.AddRoom(room);

                CarveRoom(data, room);
            }
            else
            {
                CreateRoomsFromBSP(node.leftChild, data, settings);
                CreateRoomsFromBSP(node.rightChild, data, settings);
            }
        }

        private static void CarveRoom(DungeonData data, Room room)
        {
            for (int x = Mathf.RoundToInt(room.bounds.x); x < room.bounds.x + room.bounds.width; x++)
            {
                for (int y = Mathf.RoundToInt(room.bounds.y); y < room.bounds.y + room.bounds.height; y++)
                {
                    if (data.IsValidPosition(x, y))
                        data.SetTile(x, y, TileType.Floor);
                }
            }
        }
    }
}

