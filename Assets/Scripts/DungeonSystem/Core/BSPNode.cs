using UnityEngine;

namespace DungeonSystem.Core
{
    [System.Serializable]
    public class BSPNode
    {
        public Rect bounds;
        public BSPNode leftChild;
        public BSPNode rightChild;
        public Room room;
        public bool isLeaf => leftChild == null && rightChild == null;

        public BSPNode(Rect bounds)
        {
            this.bounds = bounds;
        }

        public Room GetRandomRoom()
        {
            if (isLeaf)
                return room;
            
            if (Random.Range(0f, 1f) > 0.5f && leftChild != null)
                return leftChild.GetRandomRoom();
            else if (rightChild != null)
                return rightChild.GetRandomRoom();
            else if (leftChild != null)
                return leftChild.GetRandomRoom();
            
            return null;
        }

        public void GetAllRooms(System.Collections.Generic.List<Room> rooms)
        {
            if (isLeaf && room != null)
            {
                rooms.Add(room);
            }
            else
            {
                leftChild?.GetAllRooms(rooms);
                rightChild?.GetAllRooms(rooms);
            }
        }
    }
}