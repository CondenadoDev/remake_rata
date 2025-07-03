/*using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileType
{
    Wall = 0,
    Floor = 1,
    Door = 2,
    CaveFloor = 3,
    CaveWall = 4
}

public enum RoomType
{
    BSPRoom,
    CellularCave,
    Corridor
}

[System.Serializable]
public class BSPNode
{
    public Rect bounds;
    public BSPNode leftChild;
    public BSPNode rightChild;
    public Rect room;
    public bool isLeaf => leftChild == null && rightChild == null;
}

[System.Serializable]
public class MaterialSettings
{
    [Header("Procedural Materials")]
    public bool useProceduralMaterials = true;
    
    [Header("BSP Materials")]
    public Material bspFloorMaterial;
    public Material bspWallMaterial;
    
    [Header("Cave Materials")]
    public Material caveFloorMaterial;
    public Material caveWallMaterial;
    
    [Header("Door Material")]
    public Material doorMaterial;
    
    [Header("Procedural Settings")]
    public Color caveFloorBaseColor = new Color(0.4f, 0.35f, 0.3f);
    public Color caveWallBaseColor = new Color(0.3f, 0.25f, 0.2f);
    public float textureScale = 2f;
    public Texture2D caveFloorTexture;
    public Texture2D caveWallTexture;
    public Texture2D caveFloorNormal;
    public Texture2D caveWallNormal;
    
    [Header("Double Sided Settings")]
    public bool useDoubleSidedMaterials = true;
    public bool generateInteriorFaces = false; // Alternativa a double-sided
}

// Cache de vértices compartidos para meshes sin costuras
public class SharedVertexCache
{
    private Dictionary<Vector3Int, Vector3> vertexPositions = new Dictionary<Vector3Int, Vector3>();
    private Dictionary<Vector3Int, int> vertexIndices = new Dictionary<Vector3Int, int>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    
    public void Clear()
    {
        vertexPositions.Clear();
        vertexIndices.Clear();
        vertices.Clear();
        uvs.Clear();
    }
    
    public int GetOrCreateVertex(Vector3 worldPos, Vector2 uv, float gridSize)
    {
        // Convertir posición mundial a clave de grid
        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(worldPos.x / gridSize * 1000),
            Mathf.RoundToInt(worldPos.y / gridSize * 1000),
            Mathf.RoundToInt(worldPos.z / gridSize * 1000)
        );
        
        if (vertexIndices.ContainsKey(key))
        {
            return vertexIndices[key];
        }
        else
        {
            int index = vertices.Count;
            vertices.Add(worldPos);
            uvs.Add(uv);
            vertexPositions[key] = worldPos;
            vertexIndices[key] = index;
            return index;
        }
    }
    
    public List<Vector3> GetVertices() => vertices;
    public List<Vector2> GetUVs() => uvs;
}

[System.Serializable]
public class GenerationSettings
{
    [Header("General Settings")]
    public int dungeonWidth = 100;
    public int dungeonHeight = 100;
    public int seed = 12345;
    
    [Header("BSP Settings")]
    public int minRoomSize = 8;
    public int maxRoomSize = 20;
    public int corridorWidth = 3;
    
    [Header("Cellular Automata Settings")]
    public int caveSections = 3;
    public float initialWallChance = 0.45f;
    public int smoothingIterations = 5;
    public int birthLimit = 4;
    public int deathLimit = 3;
    public bool organicCaveShapes = true;
    public float caveShapeNoise = 0.3f;
    
    [Header("Visualization")]
    public float tileSize = 1f;
    public bool showGizmos = true;
    public bool generate3DAssets = false;
    
    [Header("3D Asset Prefabs")]
    public GameObject bspFloorPrefab;
    public GameObject bspWallPrefab;
    public GameObject caveFloorPrefab;
    public GameObject caveWallPrefab;
    public GameObject doorPrefab;
    
    [Header("3D Settings")]
    public float wallHeight = 3f;
    public bool useObjectPooling = true;
    public Transform assetsParent;
    
    [Header("Irregular Mesh Generation")]
    public bool generateIrregularMeshes = true;
    public int meshResolution = 4;
    public float irregularityStrength = 0.3f;
    public float heightVariation = 0.1f;
    public float noiseScale = 2f;
    public float noiseStrength = 0.15f;
    public bool varyEdges = true;
    public float edgeVariation = 0.4f;
    public bool generateCaveFloorVariation = true;
    public float caveFloorHeightScale = 0.5f;
    public bool seamlessMeshes = true; // Nueva opción para meshes sin costuras
    
    [Header("Gizmo Colors")]
    public Color bspFloorColor = Color.blue;
    public Color bspWallColor = Color.red;
    public Color caveFloorColor = Color.green;
    public Color caveWallColor = Color.gray;
    public Color doorColor = Color.yellow;
    public Color corridorColor = Color.cyan;
}

// Sistema de Object Pooling
[System.Serializable]
public class ObjectPool
{
    public GameObject prefab;
    public Queue<GameObject> pool = new Queue<GameObject>();
    public Transform parent;
    
    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject newObj = Object.Instantiate(prefab, parent);
            return newObj;
        }
    }
    
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}

// Pool de meshes para reutilización
public class MeshPool
{
    private Queue<Mesh> availableMeshes = new Queue<Mesh>();
    private List<Mesh> allMeshes = new List<Mesh>();
    
    public Mesh GetMesh()
    {
        if (availableMeshes.Count > 0)
        {
            return availableMeshes.Dequeue();
        }
        else
        {
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Para meshes grandes
            allMeshes.Add(newMesh);
            return newMesh;
        }
    }
    
    public void ReturnMesh(Mesh mesh)
    {
        if (mesh != null)
        {
            mesh.Clear();
            availableMeshes.Enqueue(mesh);
        }
    }
    
    public void ClearAll()
    {
        foreach (var mesh in allMeshes)
        {
            if (mesh != null)
                Object.DestroyImmediate(mesh);
        }
        availableMeshes.Clear();
        allMeshes.Clear();
    }
}

// Información de altura para cada posición (para meshes sin costuras)
public class HeightMap
{
    private Dictionary<Vector2Int, float> heights = new Dictionary<Vector2Int, float>();
    private int seed;
    private float noiseScale;
    private float noiseStrength;
    private float heightVariation;
    
    public HeightMap(int seed, float noiseScale, float noiseStrength, float heightVariation)
    {
        this.seed = seed;
        this.noiseScale = noiseScale;
        this.noiseStrength = noiseStrength;
        this.heightVariation = heightVariation;
    }
    
    public float GetHeight(float worldX, float worldZ, bool isEdge = false)
    {
        Vector2Int key = new Vector2Int(
            Mathf.RoundToInt(worldX * 100),
            Mathf.RoundToInt(worldZ * 100)
        );
        
        if (heights.ContainsKey(key))
        {
            return heights[key];
        }
        
        // Generar altura consistente basada en posición
        Random.State oldState = Random.state;
        Random.InitState(seed + key.x * 1000 + key.y);
        
        float height = 0f;
        
        // Perlin noise para variación suave
        float noiseX = worldX * noiseScale;
        float noiseZ = worldZ * noiseScale;
        float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ) - 0.5f;
        height += noiseValue * noiseStrength;
        
        // Variación aleatoria pequeña
        height += (Random.value - 0.5f) * heightVariation;
        
        Random.state = oldState;
        
        heights[key] = height;
        return height;
    }
}

public class HybridDungeonGenerator : MonoBehaviour
{
    public GenerationSettings settings;
    public MaterialSettings materialSettings;
    
    private TileType[,] dungeonMap;
    private BSPNode rootNode;
    private List<Rect> bspRooms = new List<Rect>();
    private List<Rect> caveAreas = new List<Rect>();
    
    // Sistema 3D
    private Dictionary<TileType, ObjectPool> objectPools = new Dictionary<TileType, ObjectPool>();
    public List<GameObject> activeAssets = new List<GameObject>();
    private bool assetsGenerated = false;
    private MeshPool meshPool = new MeshPool();
    
    // Cache de materiales procedurales
    private Dictionary<TileType, Material> proceduralMaterials = new Dictionary<TileType, Material>();
    
    // Sistema de altura compartida para meshes sin costuras
    private HeightMap heightMap;
    private SharedVertexCache vertexCache = new SharedVertexCache();
    
    void Start()
    {
        GenerateDungeon();
    }
    
    void OnDestroy()
    {
        Clear3DAssets();
        meshPool.ClearAll();
        ClearProceduralMaterials();
    }
    
    void OnApplicationQuit()
    {
        meshPool.ClearAll();
        ClearProceduralMaterials();
    }
    
    public void GenerateDungeon()
    {
        Random.InitState(settings.seed);
        
        // Inicializar sistema de altura compartida
        heightMap = new HeightMap(
            settings.seed,
            settings.noiseScale,
            settings.noiseStrength,
            settings.heightVariation
        );
        
        // Inicializar mapa
        dungeonMap = new TileType[settings.dungeonWidth, settings.dungeonHeight];
        InitializeMap();
        
        // Limpiar listas
        bspRooms.Clear();
        caveAreas.Clear();
        
        // Generar habitaciones BSP
        GenerateBSPRooms();
        
        // Generar cavernas con Cellular Automata mejorado
        GenerateCellularCaves();
        
        // Conectar secciones
        ConnectSections();
        
        // Post-procesamiento para suavizar transiciones
        SmoothTransitions();
        
        // Generar assets 3D si está habilitado
        if (settings.generate3DAssets)
        {
            Generate3DAssets();
        }
        else
        {
            Clear3DAssets();
        }
        
        Debug.Log($"Dungeon generated with seed: {settings.seed}");
    }
    
    void InitializeMap()
    {
        for (int x = 0; x < settings.dungeonWidth; x++)
        {
            for (int y = 0; y < settings.dungeonHeight; y++)
            {
                dungeonMap[x, y] = TileType.Wall;
            }
        }
    }
    
    void GenerateBSPRooms()
    {
        // Crear nodo raíz para BSP (usando 60% del mapa)
        int bspWidth = Mathf.RoundToInt(settings.dungeonWidth * 0.6f);
        int bspHeight = settings.dungeonHeight;
        
        rootNode = new BSPNode
        {
            bounds = new Rect(0, 0, bspWidth, bspHeight)
        };
        
        // Dividir recursivamente
        SplitNode(rootNode, 0);
        
        // Crear habitaciones
        CreateRoomsFromBSP(rootNode);
        
        // Conectar habitaciones BSP
        ConnectBSPRooms(rootNode);
    }
    
    void SplitNode(BSPNode node, int depth)
    {
        if (depth > 6 || node.bounds.width < settings.minRoomSize * 2 || 
            node.bounds.height < settings.minRoomSize * 2)
            return;
        
        bool splitHorizontal = Random.Range(0f, 1f) > 0.5f;
        
        if (node.bounds.width > node.bounds.height && node.bounds.width / node.bounds.height >= 1.25f)
            splitHorizontal = false;
        else if (node.bounds.height > node.bounds.width && node.bounds.height / node.bounds.width >= 1.25f)
            splitHorizontal = true;
        
        if (splitHorizontal)
        {
            int splitY = Random.Range(settings.minRoomSize, 
                                    Mathf.RoundToInt(node.bounds.height - settings.minRoomSize));
            
            node.leftChild = new BSPNode
            {
                bounds = new Rect(node.bounds.x, node.bounds.y, node.bounds.width, splitY)
            };
            
            node.rightChild = new BSPNode
            {
                bounds = new Rect(node.bounds.x, node.bounds.y + splitY, 
                                node.bounds.width, node.bounds.height - splitY)
            };
        }
        else
        {
            int splitX = Random.Range(settings.minRoomSize, 
                                    Mathf.RoundToInt(node.bounds.width - settings.minRoomSize));
            
            node.leftChild = new BSPNode
            {
                bounds = new Rect(node.bounds.x, node.bounds.y, splitX, node.bounds.height)
            };
            
            node.rightChild = new BSPNode
            {
                bounds = new Rect(node.bounds.x + splitX, node.bounds.y, 
                                node.bounds.width - splitX, node.bounds.height)
            };
        }
        
        SplitNode(node.leftChild, depth + 1);
        SplitNode(node.rightChild, depth + 1);
    }
    
    void CreateRoomsFromBSP(BSPNode node)
    {
        if (node.isLeaf)
        {
            // Crear habitación en el nodo hoja
            int roomWidth = Random.Range(settings.minRoomSize, 
                                       Mathf.Min(settings.maxRoomSize, Mathf.RoundToInt(node.bounds.width - 2)));
            int roomHeight = Random.Range(settings.minRoomSize, 
                                        Mathf.Min(settings.maxRoomSize, Mathf.RoundToInt(node.bounds.height - 2)));
            
            int roomX = Random.Range(Mathf.RoundToInt(node.bounds.x + 1), 
                                   Mathf.RoundToInt(node.bounds.x + node.bounds.width - roomWidth - 1));
            int roomY = Random.Range(Mathf.RoundToInt(node.bounds.y + 1), 
                                   Mathf.RoundToInt(node.bounds.y + node.bounds.height - roomHeight - 1));
            
            node.room = new Rect(roomX, roomY, roomWidth, roomHeight);
            bspRooms.Add(node.room);
            
            // Dibujar habitación en el mapa
            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    if (IsValidPosition(x, y))
                        dungeonMap[x, y] = TileType.Floor;
                }
            }
        }
        else
        {
            CreateRoomsFromBSP(node.leftChild);
            CreateRoomsFromBSP(node.rightChild);
        }
    }
    
    void ConnectBSPRooms(BSPNode node)
    {
        if (!node.isLeaf)
        {
            ConnectBSPRooms(node.leftChild);
            ConnectBSPRooms(node.rightChild);
            
            if (node.leftChild != null && node.rightChild != null)
            {
                Rect leftRoom = GetRandomRoomFromNode(node.leftChild);
                Rect rightRoom = GetRandomRoomFromNode(node.rightChild);
                
                ConnectRooms(leftRoom, rightRoom);
            }
        }
    }
    
    Rect GetRandomRoomFromNode(BSPNode node)
    {
        if (node.isLeaf)
            return node.room;
        
        if (Random.Range(0f, 1f) > 0.5f && node.leftChild != null)
            return GetRandomRoomFromNode(node.leftChild);
        else if (node.rightChild != null)
            return GetRandomRoomFromNode(node.rightChild);
        else
            return GetRandomRoomFromNode(node.leftChild);
    }
    
    void ConnectRooms(Rect room1, Rect room2)
    {
        Vector2Int point1 = new Vector2Int(
            Mathf.RoundToInt(room1.x + room1.width / 2),
            Mathf.RoundToInt(room1.y + room1.height / 2)
        );
        
        Vector2Int point2 = new Vector2Int(
            Mathf.RoundToInt(room2.x + room2.width / 2),
            Mathf.RoundToInt(room2.y + room2.height / 2)
        );
        
        // Crear corredor en L
        CreateCorridor(point1, new Vector2Int(point2.x, point1.y));
        CreateCorridor(new Vector2Int(point2.x, point1.y), point2);
    }
    
    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        Vector2Int direction = new Vector2Int(
            end.x > start.x ? 1 : end.x < start.x ? -1 : 0,
            end.y > start.y ? 1 : end.y < start.y ? -1 : 0
        );
        
        while (current != end)
        {
            for (int i = -settings.corridorWidth/2; i <= settings.corridorWidth/2; i++)
            {
                for (int j = -settings.corridorWidth/2; j <= settings.corridorWidth/2; j++)
                {
                    int x = current.x + i;
                    int y = current.y + j;
                    
                    if (IsValidPosition(x, y))
                        dungeonMap[x, y] = TileType.Floor;
                }
            }
            
            if (direction.x != 0 && current.x != end.x)
                current.x += direction.x;
            else if (direction.y != 0 && current.y != end.y)
                current.y += direction.y;
        }
    }
    
    void GenerateCellularCaves()
    {
        // Generar cavernas en la parte derecha del mapa
        int caveStartX = Mathf.RoundToInt(settings.dungeonWidth * 0.65f);
        int caveWidth = settings.dungeonWidth - caveStartX;
        
        for (int i = 0; i < settings.caveSections; i++)
        {
            int sectionHeight = settings.dungeonHeight / settings.caveSections;
            int startY = i * sectionHeight;
            
            // Crear forma orgánica si está habilitado
            Rect caveArea;
            if (settings.organicCaveShapes)
            {
                caveArea = GenerateOrganicCaveArea(caveStartX, startY, caveWidth, sectionHeight);
            }
            else
            {
                caveArea = new Rect(caveStartX, startY, caveWidth, sectionHeight);
            }
            
            caveAreas.Add(caveArea);
            GenerateCaveSection(caveArea);
        }
    }
    
    Rect GenerateOrganicCaveArea(int startX, int startY, int width, int height)
    {
        // Usar Perlin noise para crear formas más orgánicas
        float noiseOffset = Random.Range(0f, 100f);
        int margin = Mathf.RoundToInt(width * 0.2f);
        
        int minX = startX + margin;
        int maxX = startX + width - margin;
        int minY = startY + margin;
        int maxY = startY + height - margin;
        
        // Ajustar los límites con noise
        for (int y = minY; y < maxY; y++)
        {
            float noiseValue = Mathf.PerlinNoise(y * 0.05f + noiseOffset, 0);
            int xVariation = Mathf.RoundToInt((noiseValue - 0.5f) * margin * settings.caveShapeNoise);
            
            minX = Mathf.Min(minX, startX + margin + xVariation);
            maxX = Mathf.Max(maxX, startX + width - margin + xVariation);
        }
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
    
    void GenerateCaveSection(Rect area)
    {
        // Inicialización aleatoria
        for (int x = Mathf.RoundToInt(area.x); x < area.x + area.width; x++)
        {
            for (int y = Mathf.RoundToInt(area.y); y < area.y + area.height; y++)
            {
                if (IsValidPosition(x, y))
                {
                    // Usar noise para formas más orgánicas
                    float noiseValue = 0f;
                    if (settings.organicCaveShapes)
                    {
                        float nx = x * 0.1f;
                        float ny = y * 0.1f;
                        noiseValue = Mathf.PerlinNoise(nx, ny) * 0.3f;
                    }
                    
                    dungeonMap[x, y] = Random.Range(0f, 1f) < (settings.initialWallChance + noiseValue) ? 
                                      TileType.CaveWall : TileType.CaveFloor;
                }
            }
        }
        
        // Aplicar reglas de Cellular Automata
        for (int iteration = 0; iteration < settings.smoothingIterations; iteration++)
        {
            TileType[,] newMap = (TileType[,])dungeonMap.Clone();
            
            for (int x = Mathf.RoundToInt(area.x); x < area.x + area.width; x++)
            {
                for (int y = Mathf.RoundToInt(area.y); y < area.y + area.height; y++)
                {
                    if (IsValidPosition(x, y))
                    {
                        int neighborWalls = CountNeighborWalls(x, y, area);
                        
                        if (dungeonMap[x, y] == TileType.CaveWall)
                        {
                            newMap[x, y] = neighborWalls < settings.deathLimit ? 
                                          TileType.CaveFloor : TileType.CaveWall;
                        }
                        else
                        {
                            newMap[x, y] = neighborWalls > settings.birthLimit ? 
                                          TileType.CaveWall : TileType.CaveFloor;
                        }
                    }
                }
            }
            
            dungeonMap = newMap;
        }
    }
    
    int CountNeighborWalls(int x, int y, Rect area)
    {
        int wallCount = 0;
        
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int checkX = x + i;
                int checkY = y + j;
                
                if (i == 0 && j == 0) continue;
                
                if (!IsValidPosition(checkX, checkY) || 
                    !area.Contains(new Vector2(checkX, checkY)))
                {
                    wallCount++;
                }
                else if (dungeonMap[checkX, checkY] == TileType.CaveWall || 
                         dungeonMap[checkX, checkY] == TileType.Wall)
                {
                    wallCount++;
                }
            }
        }
        
        return wallCount;
    }
    
    void SmoothTransitions()
    {
        // Suavizar transiciones entre diferentes tipos de tiles
        TileType[,] smoothedMap = (TileType[,])dungeonMap.Clone();
        
        for (int x = 1; x < settings.dungeonWidth - 1; x++)
        {
            for (int y = 1; y < settings.dungeonHeight - 1; y++)
            {
                // Si es un muro entre BSP y caverna, mantenerlo
                if (IsBorderWall(x, y))
                    continue;
                
                // Contar vecinos de cada tipo
                Dictionary<TileType, int> neighborCounts = new Dictionary<TileType, int>();
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        TileType neighborType = dungeonMap[x + dx, y + dy];
                        if (!neighborCounts.ContainsKey(neighborType))
                            neighborCounts[neighborType] = 0;
                        neighborCounts[neighborType]++;
                    }
                }
                
                // Suavizar basándose en mayoría de vecinos
                if (neighborCounts.Count > 0)
                {
                    var majorityType = neighborCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                    if (neighborCounts[majorityType] >= 5)
                    {
                        smoothedMap[x, y] = majorityType;
                    }
                }
            }
        }
        
        dungeonMap = smoothedMap;
    }
    
    bool IsBorderWall(int x, int y)
    {
        if (dungeonMap[x, y] != TileType.Wall && dungeonMap[x, y] != TileType.CaveWall)
            return false;
        
        bool hasBSP = false;
        bool hasCave = false;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int checkX = x + dx;
                int checkY = y + dy;
                
                if (IsValidPosition(checkX, checkY))
                {
                    TileType type = dungeonMap[checkX, checkY];
                    if (type == TileType.Floor || type == TileType.Wall)
                        hasBSP = true;
                    if (type == TileType.CaveFloor || type == TileType.CaveWall)
                        hasCave = true;
                }
            }
        }
        
        return hasBSP && hasCave;
    }
    
    void ConnectSections()
    {
        // Conectar la sección BSP más cercana con cada caverna
        foreach (Rect cave in caveAreas)
        {
            if (bspRooms.Count == 0) continue;
            
            Rect closestBSPRoom = bspRooms[0];
            float minDistance = float.MaxValue;
            
            foreach (Rect room in bspRooms)
            {
                float distance = Vector2.Distance(room.center, cave.center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBSPRoom = room;
                }
            }
            
            // Crear conexión
            Vector2Int bspPoint = new Vector2Int(
                Mathf.RoundToInt(closestBSPRoom.x + closestBSPRoom.width),
                Mathf.RoundToInt(closestBSPRoom.y + closestBSPRoom.height / 2)
            );
            
            Vector2Int cavePoint = FindNearestCaveFloor(cave, bspPoint);
            
            if (cavePoint != Vector2Int.zero)
            {
                ConnectBSPToCave(bspPoint, cavePoint);
            }
        }
    }
    
    Vector2Int FindNearestCaveFloor(Rect cave, Vector2Int target)
    {
        Vector2Int nearest = Vector2Int.zero;
        float minDistance = float.MaxValue;
        
        for (int x = Mathf.RoundToInt(cave.x); x < cave.x + cave.width; x++)
        {
            for (int y = Mathf.RoundToInt(cave.y); y < cave.y + cave.height; y++)
            {
                if (IsValidPosition(x, y) && dungeonMap[x, y] == TileType.CaveFloor)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), target);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = new Vector2Int(x, y);
                    }
                }
            }
        }
        
        return nearest;
    }
    
    void ConnectBSPToCave(Vector2Int bspPoint, Vector2Int cavePoint)
    {
        CreateCorridor(bspPoint, cavePoint);
        
        // Añadir puerta en la conexión
        Vector2 lerpPos = Vector2.Lerp(bspPoint, cavePoint, 0.3f);
        Vector2Int doorPos = new Vector2Int(Mathf.RoundToInt(lerpPos.x), Mathf.RoundToInt(lerpPos.y));
        if (IsValidPosition(doorPos.x, doorPos.y))
        {
            dungeonMap[doorPos.x, doorPos.y] = TileType.Door;
        }
    }
    
    #region 3D Asset Generation
    
    void InitializeObjectPools()
    {
        if (!settings.useObjectPooling) return;
        
        // Crear parent para assets si no existe
        if (settings.assetsParent == null)
        {
            GameObject parentObj = new GameObject("Dungeon Assets");
            parentObj.transform.SetParent(transform);
            settings.assetsParent = parentObj.transform;
        }
        
        objectPools.Clear();
        
        // Inicializar pools para cada tipo de tile
        if (settings.bspFloorPrefab != null)
        {
            objectPools[TileType.Floor] = new ObjectPool 
            { 
                prefab = settings.bspFloorPrefab, 
                parent = settings.assetsParent 
            };
        }
        
        if (settings.bspWallPrefab != null)
        {
            objectPools[TileType.Wall] = new ObjectPool 
            { 
                prefab = settings.bspWallPrefab, 
                parent = settings.assetsParent 
            };
        }
        
        // Solo crear pools para cavernas si no se usan meshes irregulares
        if (!settings.generateIrregularMeshes)
        {
            if (settings.caveFloorPrefab != null)
            {
                objectPools[TileType.CaveFloor] = new ObjectPool 
                { 
                    prefab = settings.caveFloorPrefab, 
                    parent = settings.assetsParent 
                };
            }
            
            if (settings.caveWallPrefab != null)
            {
                objectPools[TileType.CaveWall] = new ObjectPool 
                { 
                    prefab = settings.caveWallPrefab, 
                    parent = settings.assetsParent 
                };
            }
        }
        
        if (settings.doorPrefab != null)
        {
            objectPools[TileType.Door] = new ObjectPool 
            { 
                prefab = settings.doorPrefab, 
                parent = settings.assetsParent 
            };
        }
    }
    
    void InitializeProceduralMaterials()
    {
        if (!materialSettings.useProceduralMaterials)
            return;
        
        proceduralMaterials.Clear();
        
        // Crear material para suelo de caverna
        Material caveFloorMat = new Material(Shader.Find("Standard"));
        caveFloorMat.name = "ProceduralCaveFloor";
        caveFloorMat.color = materialSettings.caveFloorBaseColor;
        if (materialSettings.caveFloorTexture != null)
        {
            caveFloorMat.mainTexture = materialSettings.caveFloorTexture;
            caveFloorMat.mainTextureScale = Vector2.one * materialSettings.textureScale;
        }
        if (materialSettings.caveFloorNormal != null)
        {
            caveFloorMat.SetTexture("_BumpMap", materialSettings.caveFloorNormal);
            caveFloorMat.EnableKeyword("_NORMALMAP");
        }
        caveFloorMat.SetFloat("_Smoothness", 0.2f);
        
        // Configurar material double-sided si está habilitado
        if (materialSettings.useDoubleSidedMaterials)
        {
            caveFloorMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        }
        
        proceduralMaterials[TileType.CaveFloor] = caveFloorMat;
        
        // Crear material para muros de caverna
        Material caveWallMat = new Material(Shader.Find("Standard"));
        caveWallMat.name = "ProceduralCaveWall";
        caveWallMat.color = materialSettings.caveWallBaseColor;
        if (materialSettings.caveWallTexture != null)
        {
            caveWallMat.mainTexture = materialSettings.caveWallTexture;
            caveWallMat.mainTextureScale = Vector2.one * materialSettings.textureScale;
        }
        if (materialSettings.caveWallNormal != null)
        {
            caveWallMat.SetTexture("_BumpMap", materialSettings.caveWallNormal);
            caveWallMat.EnableKeyword("_NORMALMAP");
        }
        caveWallMat.SetFloat("_Smoothness", 0.1f);
        
        // Configurar material double-sided si está habilitado
        if (materialSettings.useDoubleSidedMaterials)
        {
            caveWallMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        }
        
        proceduralMaterials[TileType.CaveWall] = caveWallMat;
    }
    
    void ClearProceduralMaterials()
    {
        foreach (var mat in proceduralMaterials.Values)
        {
            if (mat != null)
                DestroyImmediate(mat);
        }
        proceduralMaterials.Clear();
    }
    
    public void Generate3DAssets()
    {
        if (dungeonMap == null) return;
        
        // Limpiar assets existentes
        Clear3DAssets();
        
        // Inicializar sistemas
        if (settings.useObjectPooling)
        {
            InitializeObjectPools();
        }
        
        if (materialSettings.useProceduralMaterials)
        {
            InitializeProceduralMaterials();
        }
        
        // Crear parent para assets si no existe
        if (settings.assetsParent == null)
        {
            GameObject parentObj = new GameObject("Dungeon Assets");
            parentObj.transform.SetParent(transform);
            settings.assetsParent = parentObj.transform;
        }
        
        // Generar meshes combinadas para cavernas si se usan irregulares
        if (settings.generateIrregularMeshes)
        {
            if (settings.seamlessMeshes)
            {
                GenerateSeamlessCaveMeshes();
            }
            else
            {
                GenerateCaveMeshes();
            }
        }
        
        // Generar todos los tiles individuales
        for (int x = 0; x < settings.dungeonWidth; x++)
        {
            for (int y = 0; y < settings.dungeonHeight; y++)
            {
                TileType tileType = dungeonMap[x, y];
                
                // Skip cave tiles if using irregular meshes
                if (settings.generateIrregularMeshes && 
                    (tileType == TileType.CaveFloor || tileType == TileType.CaveWall))
                {
                    continue;
                }
                
                if (tileType == TileType.Wall && !ShouldGenerateWall(x, y)) continue;
                
                GameObject prefabToUse = GetPrefabForTileType(tileType);
                if (prefabToUse == null) continue;
                
                Vector3 position = GetWorldPosition(x, y);
                GameObject tileObject = CreateTileObject(tileType, prefabToUse, position, x, y);
                
                if (tileObject != null)
                {
                    activeAssets.Add(tileObject);
                }
            }
        }
        
        assetsGenerated = true;
        Debug.Log($"Generated {activeAssets.Count} 3D assets");
    }
    
    void GenerateSeamlessCaveMeshes()
    {
        // Generar una sola mesh grande para cada área de caverna con vértices compartidos
        foreach (var caveArea in caveAreas)
        {
            GenerateSeamlessCaveAreaMesh(caveArea);
        }
    }
    
    void GenerateSeamlessCaveAreaMesh(Rect caveArea)
    {
        vertexCache.Clear();
        
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();
        Dictionary<Vector2Int, List<int>> tileVertexMap = new Dictionary<Vector2Int, List<int>>();
        
        int startX = Mathf.Max(0, Mathf.FloorToInt(caveArea.x));
        int endX = Mathf.Min(settings.dungeonWidth, Mathf.CeilToInt(caveArea.x + caveArea.width));
        int startY = Mathf.Max(0, Mathf.FloorToInt(caveArea.y));
        int endY = Mathf.Min(settings.dungeonHeight, Mathf.CeilToInt(caveArea.y + caveArea.height));
        
        // Primera pasada: generar todos los vértices
        for (int tileX = startX; tileX < endX; tileX++)
        {
            for (int tileY = startY; tileY < endY; tileY++)
            {
                if (!IsValidPosition(tileX, tileY)) continue;
                
                TileType tileType = dungeonMap[tileX, tileY];
                if (tileType == TileType.CaveFloor)
                {
                    GenerateSeamlessFloorVertices(tileX, tileY, tileVertexMap);
                }
            }
        }
        
        // Segunda pasada: generar triángulos para suelos
        for (int tileX = startX; tileX < endX; tileX++)
        {
            for (int tileY = startY; tileY < endY; tileY++)
            {
                if (!IsValidPosition(tileX, tileY)) continue;
                
                TileType tileType = dungeonMap[tileX, tileY];
                if (tileType == TileType.CaveFloor)
                {
                    Vector2Int tileKey = new Vector2Int(tileX, tileY);
                    if (tileVertexMap.ContainsKey(tileKey))
                    {
                        GenerateFloorTriangles(tileVertexMap[tileKey], floorTriangles);
                    }
                }
            }
        }
        
        // Tercera pasada: generar muros
        for (int tileX = startX; tileX < endX; tileX++)
        {
            for (int tileY = startY; tileY < endY; tileY++)
            {
                if (!IsValidPosition(tileX, tileY)) continue;
                
                TileType tileType = dungeonMap[tileX, tileY];
                if (tileType == TileType.CaveWall && ShouldGenerateCaveWall(tileX, tileY))
                {
                    GenerateSeamlessWallMesh(tileX, tileY, wallTriangles);
                }
            }
        }
        
        // Crear objeto para suelo de caverna
        if (floorTriangles.Count > 0)
        {
            GameObject caveFloorObj = new GameObject($"SeamlessCaveFloor_Area_{caveAreas.IndexOf(caveArea)}");
            caveFloorObj.transform.SetParent(settings.assetsParent);
            
            MeshFilter meshFilter = caveFloorObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = caveFloorObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = caveFloorObj.AddComponent<MeshCollider>();
            
            Mesh floorMesh = meshPool.GetMesh();
            floorMesh.name = "SeamlessCaveFloor";
            floorMesh.vertices = vertexCache.GetVertices().ToArray();
            floorMesh.triangles = floorTriangles.ToArray();
            floorMesh.uv = vertexCache.GetUVs().ToArray();
            
            // Si se requieren caras interiores, duplicar triángulos en orden inverso
            if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
            {
                List<int> interiorTriangles = new List<int>();
                for (int i = 0; i < floorTriangles.Count; i += 3)
                {
                    // Invertir orden para cara interior
                    interiorTriangles.Add(floorTriangles[i + 2]);
                    interiorTriangles.Add(floorTriangles[i + 1]);
                    interiorTriangles.Add(floorTriangles[i]);
                }
                floorTriangles.AddRange(interiorTriangles);
                floorMesh.triangles = floorTriangles.ToArray();
            }
            
            floorMesh.RecalculateNormals();
            floorMesh.RecalculateTangents();
            floorMesh.RecalculateBounds();
            floorMesh.Optimize();
            
            meshFilter.mesh = floorMesh;
            meshCollider.sharedMesh = floorMesh;
            
            // Asignar material
            meshRenderer.material = GetOrCreateMaterial(TileType.CaveFloor);
            
            activeAssets.Add(caveFloorObj);
        }
        
        // Crear objeto para muros de caverna
        if (wallTriangles.Count > 0)
        {
            GameObject caveWallObj = new GameObject($"SeamlessCaveWall_Area_{caveAreas.IndexOf(caveArea)}");
            caveWallObj.transform.SetParent(settings.assetsParent);
            
            MeshFilter meshFilter = caveWallObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = caveWallObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = caveWallObj.AddComponent<MeshCollider>();
            
            Mesh wallMesh = meshPool.GetMesh();
            wallMesh.name = "SeamlessCaveWall";
            wallMesh.vertices = vertexCache.GetVertices().ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            wallMesh.uv = vertexCache.GetUVs().ToArray();
            
            wallMesh.RecalculateNormals();
            wallMesh.RecalculateTangents();
            wallMesh.RecalculateBounds();
            wallMesh.Optimize();
            
            meshFilter.mesh = wallMesh;
            meshCollider.sharedMesh = wallMesh;
            
            // Asignar material
            meshRenderer.material = GetOrCreateMaterial(TileType.CaveWall);
            
            activeAssets.Add(caveWallObj);
        }
    }
    
    void GenerateSeamlessFloorVertices(int tileX, int tileY, Dictionary<Vector2Int, List<int>> tileVertexMap)
    {
        List<int> vertexIndices = new List<int>();
        Vector3 tileWorldPos = GetWorldPosition(tileX, tileY);
        
        int res = settings.meshResolution;
        float step = settings.tileSize / res;
        
        // Generar vértices compartidos
        for (int y = 0; y <= res; y++)
        {
            for (int x = 0; x <= res; x++)
            {
                float localX = x * step - settings.tileSize * 0.5f;
                float localZ = y * step - settings.tileSize * 0.5f;
                
                Vector3 worldPos = tileWorldPos + new Vector3(localX, 0, localZ);
                
                // Obtener altura consistente
                float height = 0f;
                bool isEdge = (x == 0 || x == res || y == 0 || y == res);
                
                if (settings.seamlessMeshes)
                {
                    height = heightMap.GetHeight(worldPos.x, worldPos.z, isEdge);
                }
                else
                {
                    // Aplicar irregularidad normal
                    height = ApplyHeightIrregularity(worldPos, x, y, res);
                }
                
                worldPos.y = height;
                
                // UV
                float u = (tileX + (float)x / res);
                float v = (tileY + (float)y / res);
                Vector2 uv = new Vector2(u, v) * materialSettings.textureScale / 10f;
                
                int vertexIndex = vertexCache.GetOrCreateVertex(worldPos, uv, step * 0.01f);
                vertexIndices.Add(vertexIndex);
            }
        }
        
        tileVertexMap[new Vector2Int(tileX, tileY)] = vertexIndices;
    }
    
    void GenerateFloorTriangles(List<int> vertexIndices, List<int> triangles)
    {
        int res = settings.meshResolution;
        
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int i = y * (res + 1) + x;
                
                if ((x + y) % 2 == 0)
                {
                    // Primer triángulo
                    triangles.Add(vertexIndices[i]);
                    triangles.Add(vertexIndices[i + res + 1]);
                    triangles.Add(vertexIndices[i + 1]);
                    
                    // Segundo triángulo
                    triangles.Add(vertexIndices[i + 1]);
                    triangles.Add(vertexIndices[i + res + 1]);
                    triangles.Add(vertexIndices[i + res + 2]);
                }
                else
                {
                    // Primer triángulo (dirección opuesta)
                    triangles.Add(vertexIndices[i]);
                    triangles.Add(vertexIndices[i + res + 1]);
                    triangles.Add(vertexIndices[i + res + 2]);
                    
                    // Segundo triángulo
                    triangles.Add(vertexIndices[i]);
                    triangles.Add(vertexIndices[i + res + 2]);
                    triangles.Add(vertexIndices[i + 1]);
                }
            }
        }
    }
    
    void GenerateSeamlessWallMesh(int tileX, int tileY, List<int> wallTriangles)
    {
        Vector3 tileWorldPos = GetWorldPosition(tileX, tileY);
        
        // Determinar qué caras del muro generar
        bool[] facesToGenerate = new bool[4];
        Vector3[] faceDirections = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        
        for (int i = 0; i < 4; i++)
        {
            int checkX = tileX + Mathf.RoundToInt(faceDirections[i].x);
            int checkY = tileY + Mathf.RoundToInt(faceDirections[i].z);
            
            if (IsValidPosition(checkX, checkY))
            {
                TileType neighborType = dungeonMap[checkX, checkY];
                facesToGenerate[i] = (neighborType == TileType.CaveFloor || 
                                    neighborType == TileType.Floor || 
                                    neighborType == TileType.Door);
            }
        }
        
        // Generar caras necesarias
        for (int face = 0; face < 4; face++)
        {
            if (facesToGenerate[face])
            {
                GenerateWallFaceSeamless(tileWorldPos, faceDirections[face], wallTriangles);
            }
        }
        
        // Generar tapa superior
        GenerateWallTopSeamless(tileWorldPos, wallTriangles);
    }
    
    void GenerateWallFaceSeamless(Vector3 tilePos, Vector3 direction, List<int> triangles)
    {
        Vector3 right = Vector3.Cross(Vector3.up, direction);
        float halfSize = settings.tileSize * 0.5f;
        
        Vector3 center = tilePos + direction * halfSize;
        
        // Crear vértices con altura compartida
        Vector3[] positions = new Vector3[4];
        positions[0] = center - right * halfSize;
        positions[1] = center + right * halfSize;
        positions[2] = center - right * halfSize + Vector3.up * settings.wallHeight;
        positions[3] = center + right * halfSize + Vector3.up * settings.wallHeight;
        
        // Aplicar altura consistente en la base
        positions[0].y = heightMap.GetHeight(positions[0].x, positions[0].z);
        positions[1].y = heightMap.GetHeight(positions[1].x, positions[1].z);
        
        // Altura variable en la parte superior
        positions[2].y = positions[0].y + settings.wallHeight + heightMap.GetHeight(positions[2].x, positions[2].z) * 0.3f;
        positions[3].y = positions[1].y + settings.wallHeight + heightMap.GetHeight(positions[3].x, positions[3].z) * 0.3f;
        
        // Crear/obtener índices de vértices
        int[] indices = new int[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 uv = new Vector2(
                (i % 2) * materialSettings.textureScale,
                (i / 2) * materialSettings.textureScale
            );
            indices[i] = vertexCache.GetOrCreateVertex(positions[i], uv, 0.01f);
        }
        
        // Triángulos exteriores
        triangles.Add(indices[0]);
        triangles.Add(indices[2]);
        triangles.Add(indices[1]);
        
        triangles.Add(indices[1]);
        triangles.Add(indices[2]);
        triangles.Add(indices[3]);
        
        // Si se requieren caras interiores
        if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
        {
            triangles.Add(indices[1]);
            triangles.Add(indices[2]);
            triangles.Add(indices[0]);
            
            triangles.Add(indices[3]);
            triangles.Add(indices[2]);
            triangles.Add(indices[1]);
        }
    }
    
    void GenerateWallTopSeamless(Vector3 tilePos, List<int> triangles)
    {
        float halfSize = settings.tileSize * 0.5f;
        
        // Posiciones de la tapa superior
        Vector3[] positions = new Vector3[4];
        positions[0] = tilePos + new Vector3(-halfSize, 0, -halfSize);
        positions[1] = tilePos + new Vector3(halfSize, 0, -halfSize);
        positions[2] = tilePos + new Vector3(halfSize, 0, halfSize);
        positions[3] = tilePos + new Vector3(-halfSize, 0, halfSize);
        
        // Aplicar altura consistente
        for (int i = 0; i < 4; i++)
        {
            positions[i].y = settings.wallHeight + heightMap.GetHeight(positions[i].x, positions[i].z) * 0.3f;
        }
        
        // Crear/obtener índices
        int[] indices = new int[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 uv = new Vector2(
                positions[i].x / materialSettings.textureScale,
                positions[i].z / materialSettings.textureScale
            );
            indices[i] = vertexCache.GetOrCreateVertex(positions[i], uv, 0.01f);
        }
        
        // Triángulos superiores
        triangles.Add(indices[0]);
        triangles.Add(indices[1]);
        triangles.Add(indices[2]);
        
        triangles.Add(indices[0]);
        triangles.Add(indices[2]);
        triangles.Add(indices[3]);
        
        // Si se requieren caras interiores (parte inferior de la tapa)
        if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
        {
            triangles.Add(indices[2]);
            triangles.Add(indices[1]);
            triangles.Add(indices[0]);
            
            triangles.Add(indices[3]);
            triangles.Add(indices[2]);
            triangles.Add(indices[0]);
        }
    }
    
    void GenerateCaveMeshes()
    {
        // Generar una mesh combinada para cada sección de caverna
        foreach (var caveArea in caveAreas)
        {
            GenerateCaveAreaMesh(caveArea);
        }
    }
    
    void GenerateCaveAreaMesh(Rect caveArea)
    {
        List<CombineInstance> floorCombine = new List<CombineInstance>();
        List<CombineInstance> wallCombine = new List<CombineInstance>();
        
        int startX = Mathf.Max(0, Mathf.FloorToInt(caveArea.x));
        int endX = Mathf.Min(settings.dungeonWidth, Mathf.CeilToInt(caveArea.x + caveArea.width));
        int startY = Mathf.Max(0, Mathf.FloorToInt(caveArea.y));
        int endY = Mathf.Min(settings.dungeonHeight, Mathf.CeilToInt(caveArea.y + caveArea.height));
        
        // Generar meshes para cada tile de la caverna
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (!IsValidPosition(x, y)) continue;
                
                TileType tileType = dungeonMap[x, y];
                if (tileType == TileType.CaveFloor)
                {
                    Mesh tileMesh = GenerateIrregularMesh(TileType.CaveFloor, x, y);
                    if (tileMesh != null)
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = tileMesh;
                        ci.transform = Matrix4x4.TRS(GetWorldPosition(x, y), Quaternion.identity, Vector3.one);
                        floorCombine.Add(ci);
                    }
                }
                else if (tileType == TileType.CaveWall && ShouldGenerateCaveWall(x, y))
                {
                    Mesh tileMesh = GenerateIrregularMesh(TileType.CaveWall, x, y);
                    if (tileMesh != null)
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = tileMesh;
                        ci.transform = Matrix4x4.TRS(GetWorldPosition(x, y), Quaternion.identity, Vector3.one);
                        wallCombine.Add(ci);
                    }
                }
            }
        }
        
        // Crear objeto para suelos de caverna
        if (floorCombine.Count > 0)
        {
            GameObject caveFloorObj = new GameObject($"CaveFloor_Area_{caveAreas.IndexOf(caveArea)}");
            caveFloorObj.transform.SetParent(settings.assetsParent);
            
            MeshFilter meshFilter = caveFloorObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = caveFloorObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = caveFloorObj.AddComponent<MeshCollider>();
            
            Mesh combinedFloorMesh = new Mesh();
            combinedFloorMesh.name = "CaveFloorCombined";
            combinedFloorMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedFloorMesh.CombineMeshes(floorCombine.ToArray(), true, true);
            combinedFloorMesh.RecalculateNormals();
            combinedFloorMesh.RecalculateBounds();
            combinedFloorMesh.Optimize();
            
            meshFilter.mesh = combinedFloorMesh;
            meshCollider.sharedMesh = combinedFloorMesh;
            
            // Asignar material
            meshRenderer.material = GetOrCreateMaterial(TileType.CaveFloor);
            
            activeAssets.Add(caveFloorObj);
        }
        
        // Crear objeto para muros de caverna
        if (wallCombine.Count > 0)
        {
            GameObject caveWallObj = new GameObject($"CaveWall_Area_{caveAreas.IndexOf(caveArea)}");
            caveWallObj.transform.SetParent(settings.assetsParent);
            
            MeshFilter meshFilter = caveWallObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = caveWallObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = caveWallObj.AddComponent<MeshCollider>();
            
            Mesh combinedWallMesh = new Mesh();
            combinedWallMesh.name = "CaveWallCombined";
            combinedWallMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedWallMesh.CombineMeshes(wallCombine.ToArray(), true, true);
            combinedWallMesh.RecalculateNormals();
            combinedWallMesh.RecalculateBounds();
            combinedWallMesh.Optimize();
            
            meshFilter.mesh = combinedWallMesh;
            meshCollider.sharedMesh = combinedWallMesh;
            
            // Asignar material
            meshRenderer.material = GetOrCreateMaterial(TileType.CaveWall);
            
            activeAssets.Add(caveWallObj);
        }
        
        // Limpiar meshes temporales
        foreach (var ci in floorCombine)
        {
            if (ci.mesh != null)
                DestroyImmediate(ci.mesh);
        }
        foreach (var ci in wallCombine)
        {
            if (ci.mesh != null)
                DestroyImmediate(ci.mesh);
        }
    }
    
    bool ShouldGenerateCaveWall(int x, int y)
    {
        // Solo generar muros de caverna que sean visibles
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int checkX = x + dx;
                int checkY = y + dy;
                
                if (IsValidPosition(checkX, checkY))
                {
                    TileType neighborType = dungeonMap[checkX, checkY];
                    if (neighborType == TileType.CaveFloor || neighborType == TileType.Floor || 
                        neighborType == TileType.Door)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    GameObject CreateTileObject(TileType tileType, GameObject prefab, Vector3 position, int x, int y)
    {
        GameObject tileObject;
        
        // Usar object pooling si está habilitado
        if (settings.useObjectPooling && objectPools.ContainsKey(tileType))
        {
            tileObject = objectPools[tileType].GetObject();
            tileObject.transform.position = position;
            tileObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            tileObject = Instantiate(prefab, position, Quaternion.identity, settings.assetsParent);
        }
        
        // Ajustar posición según el tipo de tile
        switch (tileType)
        {
            case TileType.Wall:
                tileObject.transform.position = position + Vector3.up * (settings.wallHeight / 2);
                break;
            case TileType.Door:
                tileObject.transform.position = position + Vector3.up * (settings.wallHeight / 2);
                break;
        }
        
        tileObject.name = $"{tileType}_{x}_{y}";
        return tileObject;
    }
    
    Material GetOrCreateMaterial(TileType tileType)
    {
        // Primero intentar obtener material procedural
        if (materialSettings.useProceduralMaterials && proceduralMaterials.ContainsKey(tileType))
        {
            return proceduralMaterials[tileType];
        }
        
        // Luego intentar obtener material asignado manualmente
        switch (tileType)
        {
            case TileType.CaveFloor:
                if (materialSettings.caveFloorMaterial != null)
                    return materialSettings.caveFloorMaterial;
                break;
            case TileType.CaveWall:
                if (materialSettings.caveWallMaterial != null)
                    return materialSettings.caveWallMaterial;
                break;
        }
        
        // Finalmente, intentar obtener material del prefab
        return GetMaterialForTileType(tileType);
    }
    
    Mesh GenerateIrregularMesh(TileType tileType, int seedX, int seedY)
    {
        // Usar posición como seed para consistencia
        Random.InitState(settings.seed + seedX * 1000 + seedY);
        
        if (tileType == TileType.CaveFloor)
        {
            return GenerateIrregularFloorMesh();
        }
        else if (tileType == TileType.CaveWall)
        {
            return GenerateCompleteWallMesh(seedX, seedY);
        }
        
        return null;
    }
    
    Mesh GenerateIrregularFloorMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int res = settings.meshResolution;
        float step = 1f / res;
        
        // Generar vertices con irregularidad
        for (int y = 0; y <= res; y++)
        {
            for (int x = 0; x <= res; x++)
            {
                Vector3 vertex = new Vector3(
                    (x * step - 0.5f) * settings.tileSize,
                    0,
                    (y * step - 0.5f) * settings.tileSize
                );
                
                // Aplicar irregularidad
                vertex = ApplyFloorIrregularity(vertex, x, y, res);
                vertices.Add(vertex);
                
                // UV coordinates
                float u = (float)x / res;
                float v = (float)y / res;
                uvs.Add(new Vector2(u, v));
            }
        }
        
        // Generar triángulos principales
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int i = y * (res + 1) + x;
                
                // Alternar dirección de triángulos
                if ((x + y) % 2 == 0)
                {
                    triangles.Add(i);
                    triangles.Add(i + res + 1);
                    triangles.Add(i + 1);
                    
                    triangles.Add(i + 1);
                    triangles.Add(i + res + 1);
                    triangles.Add(i + res + 2);
                }
                else
                {
                    triangles.Add(i);
                    triangles.Add(i + res + 1);
                    triangles.Add(i + res + 2);
                    
                    triangles.Add(i);
                    triangles.Add(i + res + 2);
                    triangles.Add(i + 1);
                }
            }
        }
        
        // Si se requieren caras interiores, duplicar triángulos invertidos
        if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
        {
            int originalTriCount = triangles.Count;
            for (int i = 0; i < originalTriCount; i += 3)
            {
                triangles.Add(triangles[i + 2]);
                triangles.Add(triangles[i + 1]);
                triangles.Add(triangles[i]);
            }
        }
        
        Mesh mesh = meshPool.GetMesh();
        mesh.name = "IrregularFloor";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.Optimize();
        
        return mesh;
    }
    
    Mesh GenerateCompleteWallMesh(int tileX, int tileY)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Determinar qué caras del muro generar basándose en vecinos
        bool[] facesToGenerate = new bool[4]; // Norte, Este, Sur, Oeste
        Vector3[] faceDirections = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        
        // Verificar cada dirección
        for (int i = 0; i < 4; i++)
        {
            int checkX = tileX + Mathf.RoundToInt(faceDirections[i].x);
            int checkY = tileY + Mathf.RoundToInt(faceDirections[i].z);
            
            if (IsValidPosition(checkX, checkY))
            {
                TileType neighborType = dungeonMap[checkX, checkY];
                facesToGenerate[i] = (neighborType == TileType.CaveFloor || 
                                    neighborType == TileType.Floor || 
                                    neighborType == TileType.Door);
            }
        }
        
        // Generar caras necesarias
        int vertexOffset = 0;
        
        for (int face = 0; face < 4; face++)
        {
            if (facesToGenerate[face])
            {
                GenerateWallFace(vertices, triangles, uvs, faceDirections[face], vertexOffset);
                vertexOffset = vertices.Count;
            }
        }
        
        // Generar tapa superior del muro
        GenerateWallTop(vertices, triangles, uvs, vertexOffset);
        
        if (vertices.Count == 0)
            return null;
        
        Mesh mesh = meshPool.GetMesh();
        mesh.name = "CompleteWall";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.Optimize();
        
        return mesh;
    }
    
    void GenerateWallFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, 
                         Vector3 direction, int vertexOffset)
    {
        Vector3 right = Vector3.Cross(Vector3.up, direction);
        float halfSize = settings.tileSize * 0.5f;
        
        // Base del muro
        Vector3 center = direction * halfSize;
        
        // 4 vértices para la cara exterior
        Vector3[] faceVerts = new Vector3[4];
        faceVerts[0] = center - right * halfSize; // Inferior izquierda
        faceVerts[1] = center + right * halfSize; // Inferior derecha
        faceVerts[2] = center - right * halfSize + Vector3.up * settings.wallHeight; // Superior izquierda
        faceVerts[3] = center + right * halfSize + Vector3.up * settings.wallHeight; // Superior derecha
        
        // Aplicar irregularidad
        for (int i = 0; i < 4; i++)
        {
            Vector3 vert = faceVerts[i];
            
            // Más irregularidad en la parte superior
            if (i >= 2)
            {
                vert += direction * Random.Range(-0.1f, 0.1f) * settings.irregularityStrength;
                vert.y += Random.Range(-0.2f, 0.2f) * settings.heightVariation;
            }
            
            vertices.Add(vert);
        }
        
        // UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        
        // Triángulos exteriores
        triangles.Add(vertexOffset + 0);
        triangles.Add(vertexOffset + 2);
        triangles.Add(vertexOffset + 1);
        
        triangles.Add(vertexOffset + 1);
        triangles.Add(vertexOffset + 2);
        triangles.Add(vertexOffset + 3);
        
        // Si se requieren caras interiores
        if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
        {
            // Duplicar vértices para cara interior
            int interiorOffset = vertices.Count;
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(faceVerts[i]);
                uvs.Add(new Vector2(i % 2, i / 2));
            }
            
            // Triángulos interiores (orden invertido)
            triangles.Add(interiorOffset + 1);
            triangles.Add(interiorOffset + 2);
            triangles.Add(interiorOffset + 0);
            
            triangles.Add(interiorOffset + 3);
            triangles.Add(interiorOffset + 2);
            triangles.Add(interiorOffset + 1);
        }
    }
    
    void GenerateWallTop(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, int vertexOffset)
    {
        float halfSize = settings.tileSize * 0.5f;
        float height = settings.wallHeight;
        
        // Vértices de la tapa superior
        Vector3[] topVerts = new Vector3[4];
        topVerts[0] = new Vector3(-halfSize, height, -halfSize);
        topVerts[1] = new Vector3(halfSize, height, -halfSize);
        topVerts[2] = new Vector3(halfSize, height, halfSize);
        topVerts[3] = new Vector3(-halfSize, height, halfSize);
        
        // Aplicar irregularidad
        for (int i = 0; i < 4; i++)
        {
            topVerts[i].x += Random.Range(-0.1f, 0.1f) * settings.irregularityStrength;
            topVerts[i].z += Random.Range(-0.1f, 0.1f) * settings.irregularityStrength;
            topVerts[i].y += Random.Range(-0.1f, 0.1f) * settings.heightVariation;
            
            vertices.Add(topVerts[i]);
        }
        
        // UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
        
        // Triángulos superiores
        triangles.Add(vertexOffset + 0);
        triangles.Add(vertexOffset + 1);
        triangles.Add(vertexOffset + 2);
        
        triangles.Add(vertexOffset + 0);
        triangles.Add(vertexOffset + 2);
        triangles.Add(vertexOffset + 3);
        
        // Si se requieren caras interiores (parte inferior de la tapa)
        if (materialSettings.generateInteriorFaces && !materialSettings.useDoubleSidedMaterials)
        {
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 0);
            
            triangles.Add(vertexOffset + 3);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 0);
        }
    }
    
    float ApplyHeightIrregularity(Vector3 worldPos, int x, int y, int resolution)
    {
        float height = 0f;
        
        // Variación en altura usando Perlin noise
        float noiseX = worldPos.x * settings.noiseScale;
        float noiseZ = worldPos.z * settings.noiseScale;
        float heightNoise = Mathf.PerlinNoise(noiseX, noiseZ) - 0.5f;
        
        height += heightNoise * settings.noiseStrength;
        
        // Variación adicional para suelos de caverna
        if (settings.generateCaveFloorVariation)
        {
            float caveNoise = Mathf.PerlinNoise(noiseX * 0.5f, noiseZ * 0.5f);
            height += (caveNoise - 0.5f) * settings.caveFloorHeightScale;
        }
        
        // Variación aleatoria pequeña
        height += (Random.value - 0.5f) * settings.heightVariation;
        
        // Variación de bordes
        bool isEdge = (x == 0 || x == resolution || y == 0 || y == resolution);
        if (isEdge && settings.varyEdges)
        {
            height += (Random.value - 0.5f) * settings.edgeVariation * 0.5f;
        }
        
        return height;
    }
    
    Vector3 ApplyFloorIrregularity(Vector3 vertex, int x, int y, int resolution)
    {
        Vector3 result = vertex;
        
        // Irregularidad general
        float randomX = (Random.value - 0.5f) * settings.irregularityStrength * settings.tileSize / resolution;
        float randomZ = (Random.value - 0.5f) * settings.irregularityStrength * settings.tileSize / resolution;
        
        // Aplicar variación de bordes
        bool isEdge = (x == 0 || x == resolution || y == 0 || y == resolution);
        if (isEdge && settings.varyEdges)
        {
            float edgeRandom = (Random.value - 0.5f) * settings.edgeVariation * settings.tileSize / resolution;
            randomX += edgeRandom;
            randomZ += edgeRandom;
        }
        
        result.x += randomX;
        result.z += randomZ;
        
        // Aplicar altura
        result.y = ApplyHeightIrregularity(result, x, y, resolution);
        
        return result;
    }
    
    Material GetMaterialForTileType(TileType tileType)
    {
        // Intentar obtener material del prefab correspondiente
        GameObject prefab = GetPrefabForTileType(tileType);
        if (prefab != null)
        {
            MeshRenderer renderer = prefab.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                return renderer.sharedMaterial;
            }
        }
        
        // Material por defecto si no se encuentra
        return null;
    }
    
    GameObject GetPrefabForTileType(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Floor: return settings.bspFloorPrefab;
            case TileType.Wall: return settings.bspWallPrefab;
            case TileType.CaveFloor: return settings.caveFloorPrefab;
            case TileType.CaveWall: return settings.caveWallPrefab;
            case TileType.Door: return settings.doorPrefab;
            default: return null;
        }
    }
    
    bool ShouldGenerateWall(int x, int y)
    {
        // Solo generar muros que sean visibles (al lado de un suelo)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int checkX = x + dx;
                int checkY = y + dy;
                
                if (IsValidPosition(checkX, checkY))
                {
                    TileType neighborType = dungeonMap[checkX, checkY];
                    if (neighborType == TileType.Floor || neighborType == TileType.CaveFloor || neighborType == TileType.Door)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    public void Clear3DAssets()
    {
        // Limpiar vertex cache
        vertexCache.Clear();
        
        // Retornar objetos al pool o destruirlos
        foreach (GameObject asset in activeAssets)
        {
            if (asset != null)
            {
                // Si contiene una mesh generada dinámicamente, retornarla al pool
                MeshFilter meshFilter = asset.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.mesh != null)
                {
                    // Solo retornar al pool si es una mesh individual
                    if (asset.name.StartsWith("Irregular_") || asset.name.Contains("Seamless"))
                    {
                        meshPool.ReturnMesh(meshFilter.mesh);
                    }
                    else
                    {
                        // Las meshes combinadas se destruyen directamente
                        DestroyImmediate(meshFilter.mesh);
                    }
                }
                
                // Destruir o retornar al pool el GameObject
                if (settings.useObjectPooling && !asset.name.Contains("Cave"))
                {
                    TileType tileType = GetTileTypeFromName(asset.name);
                    if (objectPools.ContainsKey(tileType))
                    {
                        objectPools[tileType].ReturnObject(asset);
                    }
                    else
                    {
                        DestroyImmediate(asset);
                    }
                }
                else
                {
                    DestroyImmediate(asset);
                }
            }
        }
        
        activeAssets.Clear();
        assetsGenerated = false;
    }
    
    TileType GetTileTypeFromName(string objectName)
    {
        if (objectName.StartsWith("Floor")) return TileType.Floor;
        if (objectName.StartsWith("Wall")) return TileType.Wall;
        if (objectName.StartsWith("CaveFloor")) return TileType.CaveFloor;
        if (objectName.StartsWith("CaveWall")) return TileType.CaveWall;
        if (objectName.StartsWith("Door")) return TileType.Door;
        return TileType.Wall;
    }
    
    public void Toggle3DAssets()
    {
        settings.generate3DAssets = !settings.generate3DAssets;
        
        if (settings.generate3DAssets)
        {
            Generate3DAssets();
        }
        else
        {
            Clear3DAssets();
        }
    }
    
    #endregion
    
    void OnDrawGizmos()
    {
        if (!settings.showGizmos || dungeonMap == null || settings.generate3DAssets) return;
        
        Vector3 cubeSize = Vector3.one * settings.tileSize * 0.9f;
        
        for (int x = 0; x < settings.dungeonWidth; x++)
        {
            for (int y = 0; y < settings.dungeonHeight; y++)
            {
                Vector3 position = transform.position + new Vector3(x * settings.tileSize, 0, y * settings.tileSize);
                
                switch (dungeonMap[x, y])
                {
                    case TileType.Floor:
                        Gizmos.color = settings.bspFloorColor;
                        Gizmos.DrawCube(position, cubeSize);
                        break;
                        
                    case TileType.Wall:
                        Gizmos.color = settings.bspWallColor;
                        Vector3 wallPos = position + Vector3.up * (settings.tileSize * 0.5f);
                        Gizmos.DrawCube(wallPos, cubeSize + Vector3.up * settings.tileSize);
                        break;
                        
                    case TileType.CaveFloor:
                        Gizmos.color = settings.caveFloorColor;
                        Gizmos.DrawCube(position, cubeSize);
                        break;
                        
                    case TileType.CaveWall:
                        Gizmos.color = settings.caveWallColor;
                        Vector3 caveWallPos = position + Vector3.up * (settings.tileSize * 0.5f);
                        Gizmos.DrawCube(caveWallPos, cubeSize + Vector3.up * settings.tileSize);
                        break;
                        
                    case TileType.Door:
                        Gizmos.color = settings.doorColor;
                        Gizmos.DrawCube(position + Vector3.up * settings.tileSize, cubeSize);
                        break;
                }
            }
        }
        
        // Dibujar información de debug
        DrawDebugInfo();
    }
    
    void DrawDebugInfo()
    {
        if (bspRooms.Count == 0) return;
        
        // Dibujar bounds de habitaciones BSP
        Gizmos.color = Color.white;
        foreach (Rect room in bspRooms)
        {
            Vector3 center = transform.position + new Vector3(
                room.x + room.width / 2, 
                2f, 
                room.y + room.height / 2
            );
            Vector3 size = new Vector3(room.width, 0.1f, room.height) * settings.tileSize;
            Gizmos.DrawWireCube(center, size);
        }
        
        // Dibujar bounds de cavernas
        Gizmos.color = Color.magenta;
        foreach (Rect cave in caveAreas)
        {
            Vector3 center = transform.position + new Vector3(
                cave.x + cave.width / 2, 
                2.5f, 
                cave.y + cave.height / 2
            );
            Vector3 size = new Vector3(cave.width, 0.1f, cave.height) * settings.tileSize;
            Gizmos.DrawWireCube(center, size);
        }
    }
    
    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < settings.dungeonWidth && y >= 0 && y < settings.dungeonHeight;
    }
    
    // Métodos públicos para testing
    public void GenerateWithNewSeed()
    {
        settings.seed = Random.Range(0, 999999);
        GenerateDungeon();
    }
    
    public TileType GetTileAt(int x, int y)
    {
        if (IsValidPosition(x, y))
            return dungeonMap[x, y];
        return TileType.Wall;
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return transform.position + new Vector3(x * settings.tileSize, 0, y * settings.tileSize);
    }
    
    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x / settings.tileSize),
            Mathf.RoundToInt(localPos.z / settings.tileSize)
        );
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HybridDungeonGenerator))]
public class HybridDungeonGeneratorEditor : Editor
{
    private SerializedProperty settingsProp;
    private SerializedProperty materialSettingsProp;
    
    void OnEnable()
    {
        settingsProp = serializedObject.FindProperty("settings");
        materialSettingsProp = serializedObject.FindProperty("materialSettings");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        HybridDungeonGenerator generator = (HybridDungeonGenerator)target;
        
        // Título principal
        EditorGUILayout.LabelField("Hybrid Dungeon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Configuración principal
        EditorGUILayout.PropertyField(settingsProp, true);
        
        EditorGUILayout.Space();
        
        // Configuración de materiales
        EditorGUILayout.PropertyField(materialSettingsProp, true);
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        
        // Controles de generación
        EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Dungeon", GUILayout.Height(30)))
        {
            generator.GenerateDungeon();
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button("New Random Seed", GUILayout.Height(30)))
        {
            generator.GenerateWithNewSeed();
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Controles 3D
        EditorGUILayout.LabelField("3D Asset Controls", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        
        GUI.backgroundColor = generator.settings.generate3DAssets ? Color.green : Color.red;
        if (GUILayout.Button(generator.settings.generate3DAssets ? "3D Assets ON" : "3D Assets OFF", 
            GUILayout.Height(25)))
        {
            generator.Toggle3DAssets();
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;
        
        if (GUILayout.Button("Regenerate Assets", GUILayout.Height(25)))
        {
            if (generator.settings.generate3DAssets)
            {
                generator.Generate3DAssets();
                SceneView.RepaintAll();
            }
        }
        GUILayout.EndHorizontal();
        
        if (generator.settings.generateIrregularMeshes && generator.settings.generate3DAssets)
        {
            if (generator.settings.seamlessMeshes)
            {
                EditorGUILayout.HelpBox("Seamless meshes enabled - Cave tiles will connect perfectly without gaps. This uses more memory but looks better.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Standard irregular meshes enabled for caves. Prefabs are optional - procedural materials will be used if not provided.", MessageType.Info);
            }
            
            if (generator.materialSettings.useDoubleSidedMaterials)
            {
                EditorGUILayout.HelpBox("Double-sided materials enabled - Meshes will be visible from both sides.", MessageType.Info);
            }
            else if (generator.materialSettings.generateInteriorFaces)
            {
                EditorGUILayout.HelpBox("Interior faces will be generated for single-sided materials.", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Información de debug
        if (generator.GetTileAt(0, 0) != TileType.Wall)
        {
            EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Current Seed: {generator.settings.seed}");
            
            // Contar tipos de tiles
            int floorCount = 0, wallCount = 0, caveFloorCount = 0, caveWallCount = 0, doorCount = 0;
            
            for (int x = 0; x < generator.settings.dungeonWidth; x++)
            {
                for (int y = 0; y < generator.settings.dungeonHeight; y++)
                {
                    TileType tile = generator.GetTileAt(x, y);
                    switch (tile)
                    {
                        case TileType.Floor: floorCount++; break;
                        case TileType.Wall: wallCount++; break;
                        case TileType.CaveFloor: caveFloorCount++; break;
                        case TileType.CaveWall: caveWallCount++; break;
                        case TileType.Door: doorCount++; break;
                    }
                }
            }
            
            EditorGUILayout.LabelField($"BSP Floors: {floorCount}");
            EditorGUILayout.LabelField($"BSP Walls: {wallCount}");
            EditorGUILayout.LabelField($"Cave Floors: {caveFloorCount}");
            EditorGUILayout.LabelField($"Cave Walls: {caveWallCount}");
            EditorGUILayout.LabelField($"Doors: {doorCount}");
            
            float walkablePercent = ((floorCount + caveFloorCount + doorCount) / 
                                   (float)(generator.settings.dungeonWidth * generator.settings.dungeonHeight)) * 100f;
            EditorGUILayout.LabelField($"Walkable Area: {walkablePercent:F1}%");
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("3D Assets Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"3D Assets Enabled: {generator.settings.generate3DAssets}");
            EditorGUILayout.LabelField($"Irregular Meshes: {generator.settings.generateIrregularMeshes}");
            EditorGUILayout.LabelField($"Seamless Mode: {generator.settings.seamlessMeshes}");
            EditorGUILayout.LabelField($"Active 3D Objects: {generator.activeAssets.Count}");
            EditorGUILayout.LabelField($"Object Pooling: {generator.settings.useObjectPooling}");
            EditorGUILayout.LabelField($"Procedural Materials: {generator.materialSettings.useProceduralMaterials}");
            EditorGUILayout.LabelField($"Double-Sided: {generator.materialSettings.useDoubleSidedMaterials}");
        }
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif*/