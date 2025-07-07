using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class IrregularMeshSettings
{
    [Header("Mesh Generation")]
    public int resolution = 4; // Subdivisiones del plano
    public float irregularityStrength = 0.3f; // Qué tan irregular
    public float heightVariation = 0.1f; // Variación en altura
    
    [Header("Noise Settings")]
    public float noiseScale = 2f;
    public float noiseStrength = 0.2f;
    
    [Header("Edge Variation")]
    public bool varyEdges = true;
    public float edgeVariation = 0.4f;
    
    [Header("Optimization")]
    public bool optimizeMesh = true;
    public float weldThreshold = 0.01f;
}

public class IrregularPlaneGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public IrregularMeshSettings meshSettings;
    public Material floorMaterial;
    public Material wallMaterial;
    
    [Header("Prefab Generation")]
    public bool generateOnStart = false;
    public int prefabVariations = 5;
    public string savePath = "Assets/Generated/";
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateIrregularPlane();
        }
    }
    
    void Awake()
    {
        SetupComponents();
    }
    
    void SetupComponents()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
    }
    
    public Mesh GenerateIrregularPlane(int seed = -1)
    {
        if (seed != -1)
            Random.InitState(seed);
        
        SetupComponents();
        
        Mesh mesh = CreateIrregularMesh();
        
        meshFilter.mesh = mesh;
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
        
        return mesh;
    }
    
    public Mesh CreateIrregularMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int res = meshSettings.resolution;
        float step = 1f / res;
        
        // Generar vertices con irregularidad
        for (int y = 0; y <= res; y++)
        {
            for (int x = 0; x <= res; x++)
            {
                Vector3 vertex = new Vector3(
                    (x * step - 0.5f),
                    0,
                    (y * step - 0.5f)
                );
                
                // Aplicar irregularidad
                vertex = ApplyIrregularity(vertex, x, y, res);
                
                vertices.Add(vertex);
                
                // UV coordinates
                uvs.Add(new Vector2((float)x / res, (float)y / res));
            }
        }
        
        // Generar triángulos
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int i = y * (res + 1) + x;
                
                // Primer triángulo
                triangles.Add(i);
                triangles.Add(i + res + 1);
                triangles.Add(i + 1);
                
                // Segundo triángulo
                triangles.Add(i + 1);
                triangles.Add(i + res + 1);
                triangles.Add(i + res + 2);
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "IrregularPlane_" + Random.Range(1000, 9999);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        if (meshSettings.optimizeMesh)
        {
            mesh.Optimize();
        }
        
        return mesh;
    }
    
    public Vector3 ApplyIrregularity(Vector3 vertex, int x, int y, int resolution)
    {
        Vector3 result = vertex;
        
        // Irregularidad general
        float randomX = (Random.value - 0.5f) * meshSettings.irregularityStrength * (1f / resolution);
        float randomZ = (Random.value - 0.5f) * meshSettings.irregularityStrength * (1f / resolution);
        
        // No mover los vértices de los bordes tanto si no queremos variación de bordes
        bool isEdge = (x == 0 || x == resolution || y == 0 || y == resolution);
        float edgeFactor = isEdge && !meshSettings.varyEdges ? 0.2f : 1f;
        
        // Aplicar variación de bordes
        if (isEdge && meshSettings.varyEdges)
        {
            float edgeRandom = (Random.value - 0.5f) * meshSettings.edgeVariation * (1f / resolution);
            randomX += edgeRandom;
            randomZ += edgeRandom;
        }
        
        result.x += randomX * edgeFactor;
        result.z += randomZ * edgeFactor;
        
        // Variación en altura usando Perlin noise
        float noiseX = (vertex.x + 0.5f) * meshSettings.noiseScale;
        float noiseZ = (vertex.z + 0.5f) * meshSettings.noiseScale;
        float heightNoise = Mathf.PerlinNoise(noiseX, noiseZ) - 0.5f;
        
        result.y += heightNoise * meshSettings.noiseStrength;
        
        // Variación adicional en altura
        result.y += (Random.value - 0.5f) * meshSettings.heightVariation;
        
        return result;
    }
    
    // Método para generar múltiples variaciones
    public List<Mesh> GenerateMultipleVariations(int count)
    {
        List<Mesh> variations = new List<Mesh>();
        
        for (int i = 0; i < count; i++)
        {
            Mesh variation = CreateIrregularMesh();
            variation.name = $"IrregularPlane_Variation_{i}";
            variations.Add(variation);
        }
        
        return variations;
    }
    
    // Crear prefab a partir del mesh generado
    public GameObject CreatePrefabFromMesh(Mesh mesh, bool isFloor = true)
    {
        GameObject prefab = new GameObject(mesh.name);
        
        MeshFilter mf = prefab.AddComponent<MeshFilter>();
        MeshRenderer mr = prefab.AddComponent<MeshRenderer>();
        MeshCollider mc = prefab.AddComponent<MeshCollider>();
        
        mf.mesh = mesh;
        mr.material = isFloor ? floorMaterial : wallMaterial;
        mc.sharedMesh = mesh;
        
        return prefab;
    }
    
    [ContextMenu("Generate Random Irregular Plane")]
    public void GenerateRandomPlane()
    {
        GenerateIrregularPlane(Random.Range(0, 10000));
    }
    
    [ContextMenu("Generate Multiple Variations")]
    public void GenerateVariations()
    {
        List<Mesh> variations = GenerateMultipleVariations(prefabVariations);
        Debug.Log($"Generated {variations.Count} variations");
    }
}

// Herramienta para crear meshes irregulares para muros también
public class IrregularWallGenerator : MonoBehaviour
{
    [Header("Wall Settings")]
    public IrregularMeshSettings meshSettings;
    public float wallHeight = 3f;
    public bool addTopFace = true;
    public bool addBottomFace = false;
    
    public Mesh GenerateIrregularWall(int seed = -1)
    {
        if (seed != -1)
            Random.InitState(seed);
        
        return CreateIrregularWallMesh();
    }
    
    Mesh CreateIrregularWallMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int res = meshSettings.resolution;
        float step = 1f / res;
        
        // Generar vértices para la base del muro (similar al suelo)
        List<Vector3> baseVertices = new List<Vector3>();
        for (int y = 0; y <= res; y++)
        {
            for (int x = 0; x <= res; x++)
            {
                Vector3 vertex = new Vector3(
                    (x * step - 0.5f),
                    0,
                    (y * step - 0.5f)
                );
                
                vertex = ApplyWallIrregularity(vertex, x, y, res);
                baseVertices.Add(vertex);
            }
        }
        
        // Crear vértices superiores
        foreach (Vector3 baseVertex in baseVertices)
        {
            Vector3 topVertex = baseVertex + Vector3.up * wallHeight;
            // Añadir algo de variación en la altura
            topVertex.y += (Random.value - 0.5f) * meshSettings.heightVariation * 2f;
            
            vertices.Add(baseVertex); // Vértice inferior
            vertices.Add(topVertex);  // Vértice superior
        }
        
        // Generar UVs
        for (int i = 0; i < vertices.Count; i++)
        {
            uvs.Add(new Vector2(vertices[i].x + 0.5f, vertices[i].y / wallHeight));
        }
        
        // Generar triángulos para las caras del muro
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int baseIndex = (y * (res + 1) + x) * 2;
                
                // Caras frontales y traseras
                if (y == 0 || y == res - 1) // Cara frontal/trasera
                {
                    AddQuad(triangles, baseIndex, baseIndex + 2, baseIndex + 3, baseIndex + 1);
                }
                
                if (x == 0 || x == res - 1) // Caras laterales
                {
                    AddQuad(triangles, baseIndex, baseIndex + 1, baseIndex + (res + 1) * 2 + 1, baseIndex + (res + 1) * 2);
                }
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "IrregularWall_" + Random.Range(1000, 9999);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    Vector3 ApplyWallIrregularity(Vector3 vertex, int x, int y, int resolution)
    {
        Vector3 result = vertex;
        
        // Menos irregularidad en muros para mantener estructura
        float randomX = (Random.value - 0.5f) * meshSettings.irregularityStrength * 0.5f * (1f / resolution);
        float randomZ = (Random.value - 0.5f) * meshSettings.irregularityStrength * 0.5f * (1f / resolution);
        
        result.x += randomX;
        result.z += randomZ;
        
        return result;
    }
    
    void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3)
    {
        triangles.Add(v0);
        triangles.Add(v1);
        triangles.Add(v2);
        
        triangles.Add(v0);
        triangles.Add(v2);
        triangles.Add(v3);
    }
}

// Utilidad para generar múltiples prefabs automáticamente
public class IrregularPrefabFactory : MonoBehaviour
{
    [Header("Factory Settings")]
    public IrregularMeshSettings floorSettings;
    public IrregularMeshSettings wallSettings;
    public Material floorMaterial;
    public Material wallMaterial;
    
    [Header("Generation")]
    public int floorVariations = 5;
    public int wallVariations = 5;
    
    public List<GameObject> GenerateAllPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();
        
        // Generar prefabs de suelo
        IrregularPlaneGenerator floorGen = gameObject.AddComponent<IrregularPlaneGenerator>();
        floorGen.meshSettings = floorSettings;
        floorGen.floorMaterial = floorMaterial;
        
        for (int i = 0; i < floorVariations; i++)
        {
            Mesh floorMesh = floorGen.CreateIrregularMesh();
            GameObject floorPrefab = floorGen.CreatePrefabFromMesh(floorMesh, true);
            floorPrefab.name = $"CaveFloor_Variation_{i}";
            prefabs.Add(floorPrefab);
        }
        
        // Generar prefabs de muro
        IrregularWallGenerator wallGen = gameObject.AddComponent<IrregularWallGenerator>();
        wallGen.meshSettings = wallSettings;
        
        for (int i = 0; i < wallVariations; i++)
        {
            Mesh wallMesh = wallGen.GenerateIrregularWall();
            GameObject wallPrefab = CreateWallPrefab(wallMesh);
            wallPrefab.name = $"CaveWall_Variation_{i}";
            prefabs.Add(wallPrefab);
        }
        
        DestroyImmediate(floorGen);
        DestroyImmediate(wallGen);
        
        return prefabs;
    }
    
    GameObject CreateWallPrefab(Mesh mesh)
    {
        GameObject prefab = new GameObject(mesh.name);
        
        MeshFilter mf = prefab.AddComponent<MeshFilter>();
        MeshRenderer mr = prefab.AddComponent<MeshRenderer>();
        MeshCollider mc = prefab.AddComponent<MeshCollider>();
        
        mf.mesh = mesh;
        mr.material = wallMaterial;
        mc.sharedMesh = mesh;
        
        return prefab;
    }
    
    [ContextMenu("Generate All Prefab Variations")]
    public void GenerateAllVariations()
    {
        List<GameObject> prefabs = GenerateAllPrefabs();
        Debug.Log($"Generated {prefabs.Count} irregular prefabs");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IrregularPlaneGenerator))]
public class IrregularPlaneGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        IrregularPlaneGenerator generator = (IrregularPlaneGenerator)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Irregular Plane"))
        {
            generator.GenerateRandomPlane();
        }
        
        if (GUILayout.Button("Generate Multiple Variations"))
        {
            generator.GenerateVariations();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Prefab from Current Mesh"))
        {
            if (generator.GetComponent<MeshFilter>()?.mesh != null)
            {
                Mesh mesh = generator.GetComponent<MeshFilter>().mesh;
                GameObject prefab = generator.CreatePrefabFromMesh(mesh);
                Selection.activeGameObject = prefab;
            }
        }
    }
}

[CustomEditor(typeof(IrregularPrefabFactory))]
public class IrregularPrefabFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        IrregularPrefabFactory factory = (IrregularPrefabFactory)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate All Prefab Variations"))
        {
            List<GameObject> prefabs = factory.GenerateAllPrefabs();
            
            // Seleccionar todos los prefabs generados
            Selection.objects = prefabs.ToArray();
            
            EditorGUILayout.HelpBox($"Generated {prefabs.Count} prefab variations!", MessageType.Info);
        }
        
        GUILayout.Space(5);
        
        EditorGUILayout.HelpBox("This will generate multiple irregular mesh variations for your cave floors and walls.", MessageType.Info);
    }
}
#endif