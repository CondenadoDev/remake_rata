// Utils/ObjectPooling.cs
using UnityEngine;
using System.Collections.Generic;
using DungeonSystem.Core;
using DungeonSystem.Settings;

namespace DungeonSystem.Utils
{
    public class ObjectPooling : MonoBehaviour
    {
        private Dictionary<TileType, Queue<GameObject>> pools = new Dictionary<TileType, Queue<GameObject>>();
        private Dictionary<TileType, GameObject> prefabs = new Dictionary<TileType, GameObject>();
        private Transform poolParent;

        public void InitializePools(RenderSettingsDungeon settingsDungeon)
        {
            // Crear parent para el pool
            if (poolParent == null)
            {
                GameObject poolParentObj = new GameObject("Object Pool");
                poolParentObj.transform.SetParent(transform);
                poolParent = poolParentObj.transform;
            }

            // Inicializar pools para cada tipo
            InitializePool(TileType.Floor, settingsDungeon.floorPrefab);
            InitializePool(TileType.Wall, settingsDungeon.wallPrefab);
            InitializePool(TileType.Door, settingsDungeon.doorPrefab);
        }

        private void InitializePool(TileType tileType, GameObject prefab)
        {
            if (prefab == null) return;

            pools[tileType] = new Queue<GameObject>();
            prefabs[tileType] = prefab;
        }

        public GameObject GetPooledObject(TileType tileType)
        {
            if (!pools.ContainsKey(tileType) || !prefabs.ContainsKey(tileType))
                return null;

            if (pools[tileType].Count > 0)
            {
                return pools[tileType].Dequeue();
            }
            else
            {
                return Instantiate(prefabs[tileType], poolParent);
            }
        }

        public void ReturnToPool(TileType tileType, GameObject obj)
        {
            if (!pools.ContainsKey(tileType)) return;

            obj.SetActive(false);
            obj.transform.SetParent(poolParent);
            pools[tileType].Enqueue(obj);
        }

        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                        DestroyImmediate(obj);
                }
            }
            pools.Clear();
        }

        void OnDestroy()
        {
            ClearAllPools();
        }
    }
}