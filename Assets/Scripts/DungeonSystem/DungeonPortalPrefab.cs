#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DungeonSystem.Interaction;

namespace DungeonSystem.EditorTools
{
    public class DungeonPortalPrefabBuilder : EditorWindow
    {
        private enum PortalType
        {
            Simple,
            Advanced,
            Terminal,
            MagicCircle,
            Door
        }
        
        private PortalType selectedType = PortalType.Simple;
        private string prefabName = "DungeonPortal";
        
        [MenuItem("Tools/Dungeon System/Create Portal Prefab")]
        public static void ShowWindow()
        {
            GetWindow<DungeonPortalPrefabBuilder>("Portal Creator");
        }
        
        void OnGUI()
        {
            EditorGUILayout.LabelField("Dungeon Portal Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            selectedType = (PortalType)EditorGUILayout.EnumPopup("Portal Type", selectedType);
            prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Portal Prefab", GUILayout.Height(30)))
            {
                CreatePortalPrefab();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This will create a complete portal prefab with:\n" +
                "• 3D Model\n" +
                "• Interaction Script\n" +
                "• UI Elements\n" +
                "• Particle Effects", 
                MessageType.Info);
        }
        
        void CreatePortalPrefab()
        {
            // Crear GameObject principal
            GameObject portal = new GameObject(prefabName);
            
            // Añadir componente según tipo
            switch (selectedType)
            {
                case PortalType.Simple:
                    CreateSimplePortal(portal);
                    break;
                case PortalType.Advanced:
                    CreateAdvancedPortal(portal);
                    break;
                case PortalType.Terminal:
                    CreateTerminalPortal(portal);
                    break;
                case PortalType.MagicCircle:
                    CreateMagicCirclePortal(portal);
                    break;
                case PortalType.Door:
                    CreateDoorPortal(portal);
                    break;
            }
            
            // Crear prefab
            string path = "Assets/DungeonSystem/Prefabs/Portals";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/DungeonSystem/Prefabs", "Portals");
            }
            
            string prefabPath = $"{path}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(portal, prefabPath);
            
            // Limpiar escena
            DestroyImmediate(portal);
            
            // Seleccionar prefab creado
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"Portal prefab created at: {prefabPath}");
        }
        
        void CreateSimplePortal(GameObject portal)
        {
            // Modelo 3D simple
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            model.name = "Portal Model";
            model.transform.SetParent(portal.transform);
            model.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            // Material emisivo
            var renderer = model.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.cyan * 2f);
            renderer.material = mat;
            
            // Script simple
            var generator = portal.AddComponent<SimpleDungeonGenerator>();
            
            // UI Canvas para prompt
            CreatePromptUI(portal);
            
            // Efectos
            CreateGlowEffect(portal);
            
            // Collider
            var collider = portal.AddComponent<CapsuleCollider>();
            collider.radius = 1f;
            collider.height = 0.5f;
            collider.isTrigger = true;
        }
        
        void CreateAdvancedPortal(GameObject portal)
        {
            // Estructura más compleja
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(portal.transform);
            
            // Crear arco
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * 2f, 0, Mathf.Cos(angle) * 2f);
                
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.transform.SetParent(frame.transform);
                pillar.transform.localPosition = pos;
                pillar.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
                pillar.transform.LookAt(portal.transform.position);
            }
            
            // Portal center
            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Quad);
            center.name = "Portal Surface";
            center.transform.SetParent(portal.transform);
            center.transform.localRotation = Quaternion.Euler(90, 0, 0);
            center.transform.localScale = Vector3.one * 3f;
            
            // Script avanzado
            var interactable = portal.AddComponent<DungeonPortalInteractable>();
            
            // UI completa
            CreateFullUI(portal);
            
            // Partículas
            CreateParticleEffect(portal);
        }
        
        void CreateTerminalPortal(GameObject portal)
        {
            // Terminal/Consola
            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "Console";
            console.transform.SetParent(portal.transform);
            console.transform.localScale = new Vector3(1.5f, 2f, 0.5f);
            
            // Pantalla
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "Screen";
            screen.transform.SetParent(console.transform);
            screen.transform.localPosition = new Vector3(0, 0.3f, 0.51f);
            screen.transform.localScale = new Vector3(0.8f, 0.6f, 1f);
            
            // Material de pantalla
            var screenRenderer = screen.GetComponent<MeshRenderer>();
            Material screenMat = new Material(Shader.Find("Standard"));
            screenMat.EnableKeyword("_EMISSION");
            screenMat.SetColor("_EmissionColor", Color.green);
            screenMat.SetColor("_Color", Color.black);
            screenRenderer.material = screenMat;
            
            // Añadir scripts
            portal.AddComponent<DungeonPortalInteractable>();
        }
        
        void CreateMagicCirclePortal(GameObject portal)
        {
            // Círculo mágico en el suelo
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "Magic Circle";
            circle.transform.SetParent(portal.transform);
            circle.transform.localScale = new Vector3(4f, 0.01f, 4f);
            
            // Runas alrededor
            int runeCount = 8;
            for (int i = 0; i < runeCount; i++)
            {
                float angle = (i * 360f / runeCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * 2.5f, 0.1f, Mathf.Cos(angle) * 2.5f);
                
                GameObject rune = GameObject.CreatePrimitive(PrimitiveType.Quad);
                rune.name = $"Rune_{i}";
                rune.transform.SetParent(portal.transform);
                rune.transform.localPosition = pos;
                rune.transform.localRotation = Quaternion.Euler(90, i * 45, 0);
                rune.transform.localScale = Vector3.one * 0.5f;
            }
            
            // Script
            portal.AddComponent<SimpleDungeonGenerator>();
            
            // Efectos de partículas mágicas
            CreateMagicParticles(portal);
        }
        
        void CreateDoorPortal(GameObject portal)
        {
            // Marco de puerta
            GameObject frame = new GameObject("Door Frame");
            frame.transform.SetParent(portal.transform);
            
            // Lados
            GameObject leftSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSide.transform.SetParent(frame.transform);
            leftSide.transform.localPosition = new Vector3(-1f, 1.5f, 0);
            leftSide.transform.localScale = new Vector3(0.2f, 3f, 0.5f);
            
            GameObject rightSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSide.transform.SetParent(frame.transform);
            rightSide.transform.localPosition = new Vector3(1f, 1.5f, 0);
            rightSide.transform.localScale = new Vector3(0.2f, 3f, 0.5f);
            
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(frame.transform);
            top.transform.localPosition = new Vector3(0, 3f, 0);
            top.transform.localScale = new Vector3(2.2f, 0.2f, 0.5f);
            
            // Puerta
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(portal.transform);
            door.transform.localPosition = new Vector3(0, 1.5f, 0);
            door.transform.localScale = new Vector3(1.8f, 2.8f, 0.1f);
            
            // Script
            portal.AddComponent<DungeonPortalInteractable>();
        }
        
        void CreatePromptUI(GameObject portal)
        {
            // Canvas para el prompt
            GameObject canvasObj = new GameObject("Prompt Canvas");
            canvasObj.transform.SetParent(portal.transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.localPosition = new Vector3(0, 2f, 0);
            canvasRect.sizeDelta = new Vector2(3, 1);
            canvasRect.localScale = Vector3.one * 0.01f;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Panel de fondo
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.8f);
            
            // Texto
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(panel.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "Press [E] to Enter";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            // Billboard script (opcional)
            canvasObj.AddComponent<FaceCamera>();
        }
        
        void CreateFullUI(GameObject portal)
        {
            // Crear UI más compleja con selección de semilla
            // Similar a CreatePromptUI pero con más elementos
            CreatePromptUI(portal); // Por ahora usar la simple
        }
        
        void CreateGlowEffect(GameObject portal)
        {
            GameObject glow = new GameObject("Glow Effect");
            glow.transform.SetParent(portal.transform);
            glow.transform.localPosition = Vector3.up * 0.1f;
            
            // Light
            Light light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.cyan;
            light.intensity = 2f;
            light.range = 5f;
        }
        
        void CreateParticleEffect(GameObject portal)
        {
            GameObject particleObj = new GameObject("Portal Particles");
            particleObj.transform.SetParent(portal.transform);
            
            ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2f;
            shape.rotation = new Vector3(90, 0, 0);
            
            var emission = particles.emission;
            emission.rateOverTime = 20f;
            
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        void CreateMagicParticles(GameObject portal)
        {
            GameObject particleObj = new GameObject("Magic Particles");
            particleObj.transform.SetParent(portal.transform);
            
            ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 3f;
            main.startSpeed = 0.5f;
            main.startSize = 0.2f;
            main.startColor = new Color(0.5f, 0, 1f);
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2f;
            shape.rotation = new Vector3(90, 0, 0);
            
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.5f);
            velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(1f);
            
            var emission = particles.emission;
            emission.rateOverTime = 10f;
        }
    }
    
    // Script auxiliar para que el UI mire a la cámara
    public class FaceCamera : MonoBehaviour
    {
        void Update()
        {
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up);
            }
        }
    }
}
#endif