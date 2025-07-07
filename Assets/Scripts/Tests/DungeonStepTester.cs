using UnityEngine;
using DungeonSystem;

/// <summary>
/// Prueba la generación paso a paso para encontrar dónde falla
/// </summary>
public class DungeonStepTester : MonoBehaviour
{
    public DungeonManager dungeonManager;
    public bool testInProgress = false;
    
    void Start()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();
            
        if (dungeonManager != null)
        {
            dungeonManager.autoGenerateOnStart = false;
            Debug.Log("[StepTester] Disabled auto generation");
        }
    }
    
    [ContextMenu("Test Step 1: Clear")]
    public void TestClear()
    {
        Debug.Log("[StepTester] === STEP 1: CLEAR ===");
        try
        {
            dungeonManager.ClearDungeon();
            Debug.Log("[StepTester] ✓ Clear completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Clear failed: {e.Message}");
        }
    }
    
    [ContextMenu("Test Step 2: Generate Structure")]
    public void TestGenerateStructure()
    {
        Debug.Log("[StepTester] === STEP 2: GENERATE STRUCTURE ===");
        try
        {
            dungeonManager.GenerateMapStructure();
            Debug.Log("[StepTester] ✓ Structure generation completed");
            
            if (dungeonManager.DungeonData != null)
            {
                Debug.Log($"[StepTester] - Rooms: {dungeonManager.DungeonData.rooms.Count}");
                Debug.Log($"[StepTester] - Size: {dungeonManager.DungeonData.width}x{dungeonManager.DungeonData.height}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Structure generation failed: {e.Message}\n{e.StackTrace}");
        }
    }
    
    [ContextMenu("Test Step 3: Select Starting Point")]
    public void TestSelectStartingPoint()
    {
        Debug.Log("[StepTester] === STEP 3: SELECT STARTING POINT ===");
        try
        {
            dungeonManager.SelectStartingPoint();
            Debug.Log("[StepTester] ✓ Starting point selected");
            
            if (dungeonManager.DungeonData?.startingRoom != null)
            {
                Debug.Log($"[StepTester] - Starting room at: {dungeonManager.DungeonData.startingRoom.centerPoint}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Starting point selection failed: {e.Message}");
        }
    }
    
    [ContextMenu("Test Step 4: Setup Progression")]
    public void TestSetupProgression()
    {
        Debug.Log("[StepTester] === STEP 4: SETUP PROGRESSION ===");
        try
        {
            dungeonManager.SetupInitialProgression();
            Debug.Log("[StepTester] ✓ Progression setup completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Progression setup failed: {e.Message}");
        }
    }
    
    [ContextMenu("Test Step 5: Populate Entities")]
    public void TestPopulateEntities()
    {
        Debug.Log("[StepTester] === STEP 5: POPULATE ENTITIES ===");
        try
        {
            dungeonManager.PopulateWithEntities();
            Debug.Log("[StepTester] ✓ Entity population completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Entity population failed: {e.Message}");
        }
    }
    
    [ContextMenu("Test Step 6: Render")]
    public void TestRender()
    {
        Debug.Log("[StepTester] === STEP 6: RENDER ===");
        try
        {
            dungeonManager.RenderDungeon();
            Debug.Log("[StepTester] ✓ Rendering completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StepTester] ✗ Rendering failed: {e.Message}");
        }
    }
    
    [ContextMenu("Test All Steps")]
    public void TestAllSteps()
    {
        Debug.Log("[StepTester] === TESTING ALL STEPS ===");
        TestClear();
        TestGenerateStructure();
        TestSelectStartingPoint();
        TestSetupProgression();
        TestPopulateEntities();
        TestRender();
        Debug.Log("[StepTester] === ALL STEPS COMPLETED ===");
    }
    
    void Update()
    {
        // Teclas para pruebas rápidas
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[StepTester] F5 pressed - Testing all steps");
            TestAllSteps();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("[StepTester] F6 pressed - Clearing dungeon");
            TestClear();
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DungeonStepTester))]
public class DungeonStepTesterEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Step by Step Testing", UnityEditor.EditorStyles.boldLabel);
        
        DungeonStepTester tester = (DungeonStepTester)target;
        
        if (GUILayout.Button("1. Clear", GUILayout.Height(25)))
            tester.TestClear();
            
        if (GUILayout.Button("2. Generate Structure", GUILayout.Height(25)))
            tester.TestGenerateStructure();
            
        if (GUILayout.Button("3. Select Starting Point", GUILayout.Height(25)))
            tester.TestSelectStartingPoint();
            
        if (GUILayout.Button("4. Setup Progression", GUILayout.Height(25)))
            tester.TestSetupProgression();
            
        if (GUILayout.Button("5. Populate Entities", GUILayout.Height(25)))
            tester.TestPopulateEntities();
            
        if (GUILayout.Button("6. Render", GUILayout.Height(25)))
            tester.TestRender();
            
        UnityEditor.EditorGUILayout.Space();
        
        if (GUILayout.Button("TEST ALL STEPS", GUILayout.Height(30)))
            tester.TestAllSteps();
            
        UnityEditor.EditorGUILayout.HelpBox(
            "Test each step individually to find where it fails.\n" +
            "Check the Console for detailed logs.", 
            UnityEditor.MessageType.Info);
    }
}
#endif