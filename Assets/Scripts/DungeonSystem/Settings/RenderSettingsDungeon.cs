using UnityEngine;

[CreateAssetMenu(fileName = "New Render Settings", menuName = "Dungeon System/Render Settings", order = 2)]
[System.Serializable]
public class RenderSettingsDungeon : ScriptableObject
{
    [Header("3D Generation")]
    public bool generate3DAssets = false;
    public float wallHeight = 3f;
    public bool useObjectPooling = true;
    public Transform assetsParent;
        
    [Header("3D Asset Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject entrancePrefab; // Nuevo: prefab especial para la entrada principal
    
    [Header("Entrance Settings")]
    public bool useSpecialEntranceEffect = true;
    public GameObject entranceMarkerPrefab; // Marcador visual para la entrada (antorcha, cartel, etc.)
    public float entranceScale = 1.2f; // Hacer la entrada más grande
        
    [Header("Mesh Generation")]
    public bool generateIrregularMeshes = false;
    public int meshResolution = 4;
    public float irregularityStrength = 0.3f;
    public float heightVariation = 0.1f;
    public float noiseScale = 2f;
    public float noiseStrength = 0.15f;
        
    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material doorMaterial;
    public Material entranceMaterial; // Material especial para la entrada
        
    [Header("Procedural Materials")]
    public bool useProceduralMaterials = false;
    public Color floorBaseColor = new Color(0.6f, 0.6f, 0.6f);
    public Color wallBaseColor = new Color(0.4f, 0.4f, 0.4f);
    public float textureScale = 2f;

    void OnValidate()
    {
        wallHeight = Mathf.Max(0.1f, wallHeight);
        meshResolution = Mathf.Clamp(meshResolution, 2, 10);
        irregularityStrength = Mathf.Max(0f, irregularityStrength);
        heightVariation = Mathf.Max(0f, heightVariation);
        textureScale = Mathf.Max(0.1f, textureScale);
        entranceScale = Mathf.Max(0.1f, entranceScale);
    }
}