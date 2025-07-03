using UnityEngine;
using System.Collections;
using DungeonSystem;

/// <summary>
/// Prueba diferentes métodos para ver cuál falla
/// </summary>
public class PortalComparisonTest : MonoBehaviour
{
    public DungeonManager dungeonManager;
    
    void Start()
    {
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();
            
        if (dungeonManager != null)
            dungeonManager.autoGenerateOnStart = false;
    }
    
    void Update()
    {
        // Método 1: Directo (como G)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[TEST] Method 1: Direct call");
            Method1_Direct();
        }
        
        // Método 2: Corrutina simple
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[TEST] Method 2: Simple coroutine");
            StartCoroutine(Method2_SimpleCoroutine());
        }
        
        // Método 3: Corrutina con yields
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[TEST] Method 3: Coroutine with yields");
            StartCoroutine(Method3_CoroutineWithYields());
        }
        
        // Método 4: Llamadas individuales
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("[TEST] Method 4: Individual calls");
            StartCoroutine(Method4_IndividualCalls());
        }
    }
    
    // MÉTODO 1: Como el SafeGenerator (funciona con G)
    void Method1_Direct()
    {
        Debug.Log("[Method1] Starting...");
        dungeonManager.GenerateCompleteDungeon();
        Debug.Log("[Method1] Done!");
    }
    
    // MÉTODO 2: Corrutina simple
    IEnumerator Method2_SimpleCoroutine()
    {
        Debug.Log("[Method2] Starting coroutine...");
        dungeonManager.GenerateCompleteDungeon();
        yield return null;
        Debug.Log("[Method2] Done!");
    }
    
    // MÉTODO 3: Corrutina con yields entre pasos
    IEnumerator Method3_CoroutineWithYields()
    {
        Debug.Log("[Method3] Starting with yields...");
        
        dungeonManager.ClearDungeon();
        yield return new WaitForSeconds(0.1f);
        
        dungeonManager.GenerateMapStructure();
        yield return new WaitForSeconds(0.1f);
        
        dungeonManager.SelectStartingPoint();
        yield return new WaitForSeconds(0.1f);
        
        dungeonManager.SetupInitialProgression();
        yield return new WaitForSeconds(0.1f);
        
        dungeonManager.PopulateWithEntities();
        yield return new WaitForSeconds(0.1f);
        
        dungeonManager.RenderDungeon();
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("[Method3] Done!");
    }
    
    // MÉTODO 4: Como el portal original
    IEnumerator Method4_IndividualCalls()
    {
        Debug.Log("[Method4] Individual calls...");
        
        dungeonManager.ClearDungeon();
        yield return null;
        
        dungeonManager.GenerateMapStructure();
        yield return null;
        
        dungeonManager.SelectStartingPoint();
        yield return null;
        
        dungeonManager.SetupInitialProgression();
        yield return null;
        
        dungeonManager.PopulateWithEntities();
        yield return null;
        
        dungeonManager.RenderDungeon();
        yield return null;
        
        Debug.Log("[Method4] Done!");
    }
    
    void OnGUI()
    {
        int y = 10;
        GUI.Label(new Rect(10, y, 500, 20), "PORTAL COMPARISON TEST:"); y += 25;
        GUI.Label(new Rect(10, y, 500, 20), "[1] Direct call (like G key)"); y += 20;
        GUI.Label(new Rect(10, y, 500, 20), "[2] Simple coroutine"); y += 20;
        GUI.Label(new Rect(10, y, 500, 20), "[3] Coroutine with waits"); y += 20;
        GUI.Label(new Rect(10, y, 500, 20), "[4] Individual calls (like original portal)"); y += 20;
    }
}