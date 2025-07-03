using UnityEngine;
using System.Collections.Generic;
using DungeonSystem.Core;
using DungeonSystem.Settings;
using DungeonSystem.Utils;

namespace DungeonSystem.Rendering
{
    public class DungeonRenderer : MonoBehaviour
    {
        //──────────────────────────────────────── Runtime
        [Header("Runtime Data")]
        public List<GameObject> activeAssets = new();
        private Dictionary<GridPosition, GameObject> doorObjects = new();

        //──────────────────────────────────────── Systems
        [SerializeField] private MeshGenerator   meshGenerator;
        [SerializeField] private MaterialManager materialManager;

        private RenderSettingsDungeon renderSettingsDungeon;
        private ObjectPooling    objectPooling;
        private bool             isInitialized = false;

        //──────────────────────────────────────── Init
        public void Initialize(RenderSettingsDungeon settingsDungeon)
        {
            renderSettingsDungeon = settingsDungeon;

            InitializeMeshGenerator();
            InitializeMaterialManager();
            InitializeObjectPooling();

            isInitialized = true;
        }

        public void RenderDungeon(DungeonData dungeonData)
        {
            if (!isInitialized)
            {
                Debug.LogError("DungeonRenderer not initialized");
                return;
            }

            ClearRendering();

            if (renderSettingsDungeon.generate3DAssets)
                Render3DAssets(dungeonData);

            Debug.Log($"Rendered {activeAssets.Count} 3D assets");
        }

        //──────────────────────────────────────── 3D Tiles
        private void Render3DAssets(DungeonData dungeonData)
        {
            // Garantizar padre
            if (renderSettingsDungeon.assetsParent == null)
            {
                var parent = new GameObject("Dungeon 3D Assets");
                parent.transform.SetParent(transform);
                renderSettingsDungeon.assetsParent = parent.transform;
            }

            // Primero renderizar pisos y muros
            for (int x = 0; x < dungeonData.width; x++)
            {
                for (int y = 0; y < dungeonData.height; y++)
                {
                    TileType tile = dungeonData.GetTile(x, y);

                    // Las puertas las renderizamos aparte con información de orientación
                    if (tile == TileType.Door) continue;

                    if (tile == TileType.Wall && !ShouldRenderWall(dungeonData, x, y))
                        continue;

                    GameObject prefab = GetPrefabForTileType(tile);
                    if (prefab == null) continue;

                    Vector3 pos = GetWorldPosition(x, y);
                    GameObject obj = CreateTileObject(tile, prefab, pos, Quaternion.identity, x, y);

                    if (obj != null) activeAssets.Add(obj);
                }
            }

            // Renderizar puertas con orientación correcta
            RenderDoors(dungeonData);

            if (renderSettingsDungeon.generateIrregularMeshes)
                meshGenerator.GenerateIrregularMeshes(dungeonData, renderSettingsDungeon);
        }

        private void RenderDoors(DungeonData dungeonData)
        {
            // Renderizar cada puerta con su orientación correcta
            foreach (var door in dungeonData.doors)
            {
                if (!dungeonData.IsValidPosition(door.position.x, door.position.y))
                    continue;

                GameObject doorPrefab = renderSettingsDungeon.doorPrefab;
                if (doorPrefab == null) continue;

                Vector3 pos = GetWorldPosition(door.position.x, door.position.y);
                
                // Aplicar rotación según orientación
                Quaternion rotation = Quaternion.Euler(0, door.GetRotationAngle(), 0);
                
                // Ajustar posición para centrar la puerta correctamente
                pos = AdjustDoorPosition(pos, door.orientation);
                
                GameObject doorObj = CreateTileObject(TileType.Door, doorPrefab, pos, rotation, 
                                                    door.position.x, door.position.y);

                if (doorObj != null)
                {
                    activeAssets.Add(doorObj);
                    doorObjects[door.position] = doorObj;
                    
                    // Añadir componente para interacción si es necesario
                    AddDoorComponent(doorObj, door, dungeonData);
                }
            }
        }

        private Vector3 AdjustDoorPosition(Vector3 basePos, DoorOrientation orientation)
        {
            // Ajustar la posición de la puerta según su orientación
            // Esto puede necesitar ajustes según cómo estén configurados tus prefabs
            Vector3 adjustedPos = basePos;
            
            if (orientation == DoorOrientation.Vertical)
            {
                // Las puertas verticales pueden necesitar un offset diferente
                // Ajusta estos valores según tus prefabs
                adjustedPos += Vector3.up * (renderSettingsDungeon.wallHeight * 0.5f);
            }
            else
            {
                // Puertas horizontales
                adjustedPos += Vector3.up * (renderSettingsDungeon.wallHeight * 0.5f);
            }
            
            return adjustedPos;
        }

        private void AddDoorComponent(GameObject doorObj, DungeonDoor doorData, DungeonData dungeonData)
        {
            // Añadir un componente para manejar la interacción con la puerta
            DoorBehaviour doorBehaviour = doorObj.AddComponent<DoorBehaviour>();
            doorBehaviour.Initialize(doorData, dungeonData);
        }

        //──────────────────────────────────────── Init Helpers
        private void InitializeMeshGenerator()
        {
            if (meshGenerator != null) return;

            var go = new GameObject("MeshGenerator");
            go.transform.SetParent(transform);
            meshGenerator = go.AddComponent<MeshGenerator>();
        }

        private void InitializeMaterialManager()
        {
            if (materialManager != null) return;

            var go = new GameObject("MaterialManager");
            go.transform.SetParent(transform);
            materialManager = go.AddComponent<MaterialManager>();

            materialManager.Initialize(renderSettingsDungeon);
        }

        private void InitializeObjectPooling()
        {
            if (!renderSettingsDungeon.useObjectPooling) return;

            if (objectPooling == null)
            {
                var go = new GameObject("ObjectPooling");
                go.transform.SetParent(transform);
                objectPooling = go.AddComponent<ObjectPooling>();
            }

            objectPooling.InitializePools(renderSettingsDungeon);
        }

        //──────────────────────────────────────── Tile factory
        private GameObject CreateTileObject(
            TileType tile, GameObject prefab, Vector3 pos, Quaternion rotation, int x, int y)
        {
            GameObject obj = null;

            if (renderSettingsDungeon.useObjectPooling && objectPooling != null)
            {
                obj = objectPooling.GetPooledObject(tile);
                if (obj != null)
                {
                    obj.transform.SetPositionAndRotation(pos, rotation);
                    obj.SetActive(true);
                }
            }
            else
            {
                obj = Instantiate(
                    prefab, pos, rotation, renderSettingsDungeon.assetsParent);
            }

            if (obj == null) return null;

            // Ajuste de altura para muros (las puertas se ajustan en AdjustDoorPosition)
            if (tile == TileType.Wall)
                obj.transform.position += Vector3.up * (renderSettingsDungeon.wallHeight * 0.5f);

            ApplyMaterial(obj, tile);

            obj.name = $"{tile}_{x}_{y}";
            return obj;
        }

        private void ApplyMaterial(GameObject obj, TileType tile)
        {
            if (materialManager == null) return;
            var rend = obj.GetComponent<MeshRenderer>();
            if (rend == null) return;

            Material mat = materialManager.GetMaterialForTileType(tile);
            if (mat != null) rend.material = mat;
        }

        //──────────────────────────────────────── Helpers
        private bool ShouldRenderWall(DungeonData data, int x, int y)
        {
            // Solo muros vecinos a un piso/puerta/corredor
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (!data.IsValidPosition(nx, ny)) continue;
                var n = data.GetTile(nx, ny);
                if (n == TileType.Floor || n == TileType.Door) return true;
            }
            return false;
        }

        private GameObject GetPrefabForTileType(TileType t) =>
            t switch
            {
                TileType.Floor => renderSettingsDungeon.floorPrefab,
                TileType.Wall  => renderSettingsDungeon.wallPrefab,
                TileType.Door  => renderSettingsDungeon.doorPrefab,
                _              => null
            };

        private Vector3 GetWorldPosition(int x, int y)
        {
            // Si necesitas tamaño real del tile y este dato está en GenerationSettings,
            // pásalo por parámetro al Renderer; de momento usamos 1 u.
            float size = 1f;
            return transform.position + new Vector3(x * size, 0, y * size);
        }

        //──────────────────────────────────────── Cleanup
        public void ClearRendering()
        {
            foreach (var go in activeAssets)
            {
                if (!go) continue;
                if (renderSettingsDungeon.useObjectPooling && objectPooling != null)
                {
                    objectPooling.ReturnToPool(GetTileTypeFromName(go.name), go);
                }
                else
                {
                    DestroyImmediate(go);
                }
            }
            activeAssets.Clear();
            doorObjects.Clear();
            meshGenerator?.ClearGeneratedMeshes();
        }

        private TileType GetTileTypeFromName(string n) =>
            n.StartsWith("Floor") ? TileType.Floor :
            n.StartsWith("Wall")  ? TileType.Wall  :
            n.StartsWith("Door")  ? TileType.Door  : TileType.Wall;

        private void OnDestroy()
        {
            ClearRendering();
            materialManager?.ClearProceduralMaterials();
        }
    }

    // Componente simple para manejar el comportamiento de las puertas
    public class DoorBehaviour : MonoBehaviour
    {
        private DungeonDoor doorData;
        private DungeonData dungeonData;
        
        public void Initialize(DungeonDoor data, DungeonData dungeon)
        {
            doorData = data;
            dungeonData = dungeon;
        }
        
        // Puedes expandir esto con lógica de apertura/cierre, animaciones, etc.
        void OnDrawGizmos()
        {
            if (doorData == null) return;
            
            // Dibujar conexión entre habitaciones
            if (doorData.roomA != null && doorData.roomB != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 pos = transform.position;
                Vector3 roomAPos = new Vector3(doorData.roomA.centerPoint.x, 0, doorData.roomA.centerPoint.y);
                Vector3 roomBPos = new Vector3(doorData.roomB.centerPoint.x, 0, doorData.roomB.centerPoint.y);
                
                Gizmos.DrawLine(pos, roomAPos);
                Gizmos.DrawLine(pos, roomBPos);
            }
        }
    }
}