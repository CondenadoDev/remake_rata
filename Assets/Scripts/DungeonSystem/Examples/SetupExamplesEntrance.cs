#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DungeonSystem;
using DungeonSystem.Settings;
using DungeonSystem.Core;

namespace DungeonSystem.Examples
{
    /// <summary>
    /// Ejemplo de cómo configurar el sistema para tener entrada desde el exterior
    /// </summary>
    public static class SetupExampleEntrance
    {
        [MenuItem("Tools/Dungeon System/Examples/Setup Exterior Entrance")]
        public static void SetupExteriorEntrance()
        {
            // Buscar o crear DungeonManager
            DungeonManager dungeonManager = GameObject.FindObjectOfType<DungeonManager>();
            if (dungeonManager == null)
            {
                GameObject go = new GameObject("Dungeon System with Entrance");
                dungeonManager = go.AddComponent<DungeonManager>();
            }

            // Crear o modificar StartingPointCriteria
            StartingPointCriteria criteria = ScriptableObject.CreateInstance<StartingPointCriteria>();
            criteria.name = "Entrance_StartingCriteria";
            
            // Configurar para preferir bordes
            criteria.preferMapEdge = true;
            criteria.edgePreferenceStrength = 80f; // Alta preferencia por bordes
            criteria.createExteriorEntrance = true;
            criteria.entranceWidth = 3f;
            criteria.allowCorners = false; // No permitir esquinas
            criteria.minConnections = 1; // Solo necesita una conexión (hacia adentro)
            criteria.maxConnections = 3; // No demasiadas conexiones en la entrada
            criteria.preferredEdge = EdgePreference.South; // Entrada por el sur
            criteria.minRoomArea = 64f; // Habitación de entrada decente
            
            // Asignar al DungeonManager
            dungeonManager.startingPointCriteria = criteria;
            
            // Mensaje de confirmación
            EditorUtility.DisplayDialog("Setup Complete", 
                "El sistema está configurado para crear una entrada desde el exterior.\n\n" +
                "La habitación inicial estará en el borde del mapa con una puerta hacia afuera.\n\n" +
                "Ajusta los parámetros en StartingPointCriteria según necesites.", 
                "OK");
            
            // Seleccionar el GameObject
            Selection.activeGameObject = dungeonManager.gameObject;
            
            Debug.Log("Sistema configurado para entrada exterior. Genera el dungeon para ver el resultado.");
        }
        
        [MenuItem("Tools/Dungeon System/Examples/Create Entrance Prefabs")]
        public static void CreateEntrancePrefabs()
        {
            string prefabPath = "Assets/DungeonSystem/Prefabs/Entrance";
            
            // Crear carpeta si no existe
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets/DungeonSystem/Prefabs", "Entrance");
            }
            
            // Crear prefab de entrada principal
            GameObject entranceDoor = new GameObject("EntranceDoor");
            
            // Marco de la puerta
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "DoorFrame";
            frame.transform.SetParent(entranceDoor.transform);
            frame.transform.localScale = new Vector3(2f, 3f, 0.3f);
            frame.transform.localPosition = Vector3.up * 1.5f;
            
            // Puerta
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(entranceDoor.transform);
            door.transform.localScale = new Vector3(1.6f, 2.4f, 0.2f);
            door.transform.localPosition = Vector3.up * 1.2f;
            
            // Material diferente para la puerta
            var doorRenderer = door.GetComponent<MeshRenderer>();
            Material doorMat = new Material(Shader.Find("Standard"));
            doorMat.color = new Color(0.4f, 0.2f, 0.1f); // Marrón madera
            doorRenderer.material = doorMat;
            
            // Crear indicador de entrada (arco decorativo)
            GameObject entranceArch = new GameObject("EntranceArch");
            
            GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPillar.name = "LeftPillar";
            leftPillar.transform.SetParent(entranceArch.transform);
            leftPillar.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            leftPillar.transform.localPosition = new Vector3(-1.5f, 2f, 0);
            
            GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPillar.name = "RightPillar";
            rightPillar.transform.SetParent(entranceArch.transform);
            rightPillar.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            rightPillar.transform.localPosition = new Vector3(1.5f, 2f, 0);
            
            GameObject archTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archTop.name = "ArchTop";
            archTop.transform.SetParent(entranceArch.transform);
            archTop.transform.localScale = new Vector3(3.5f, 0.5f, 0.5f);
            archTop.transform.localPosition = new Vector3(0, 4f, 0);
            
            // Guardar como prefabs
            GameObject entrancePrefab = PrefabUtility.SaveAsPrefabAsset(entranceDoor, 
                $"{prefabPath}/EntranceDoor.prefab");
            GameObject archPrefab = PrefabUtility.SaveAsPrefabAsset(entranceArch, 
                $"{prefabPath}/EntranceArch.prefab");
            
            // Limpiar escena
            GameObject.DestroyImmediate(entranceDoor);
            GameObject.DestroyImmediate(entranceArch);
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Prefabs Created", 
                "Se crearon los prefabs de entrada en:\n" + prefabPath, 
                "OK");
        }
    }
}
#endif