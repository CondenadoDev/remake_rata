using UnityEngine;
using System.Collections;

/// <summary>
/// Detecta si las corrutinas están causando el problema
/// </summary>
public class CoroutineDebugger : MonoBehaviour
{
    private bool testInProgress = false;
    private float testStartTime;
    
    void Update()
    {
        // Test básico de corrutina
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("[CoroutineDebug] Testing basic coroutine...");
            StartCoroutine(TestBasicCoroutine());
        }
        
        // Test con WaitForSeconds
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("[CoroutineDebug] Testing WaitForSeconds...");
            StartCoroutine(TestWaitForSeconds());
        }
        
        // Test con yield return null
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("[CoroutineDebug] Testing yield return null...");
            StartCoroutine(TestYieldNull());
        }
        
        // Mostrar si hay un test en progreso
        if (testInProgress)
        {
            float elapsed = Time.realtimeSinceStartup - testStartTime;
            if (elapsed > 5f)
            {
                Debug.LogError($"[CoroutineDebug] Test stuck for {elapsed:F2} seconds!");
                testInProgress = false;
            }
        }
    }
    
    IEnumerator TestBasicCoroutine()
    {
        testInProgress = true;
        testStartTime = Time.realtimeSinceStartup;
        
        Debug.Log("[Test1] Start");
        yield return null;
        Debug.Log("[Test1] End");
        
        testInProgress = false;
    }
    
    IEnumerator TestWaitForSeconds()
    {
        testInProgress = true;
        testStartTime = Time.realtimeSinceStartup;
        
        Debug.Log("[Test2] Start");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[Test2] After 0.1s");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[Test2] End");
        
        testInProgress = false;
    }
    
    IEnumerator TestYieldNull()
    {
        testInProgress = true;
        testStartTime = Time.realtimeSinceStartup;
        
        Debug.Log("[Test3] Start");
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"[Test3] Frame {i}");
            yield return null;
        }
        Debug.Log("[Test3] End");
        
        testInProgress = false;
    }
    
    void OnGUI()
    {
        int y = 200;
        GUI.Label(new Rect(10, y, 400, 20), "COROUTINE TESTS:"); y += 25;
        GUI.Label(new Rect(10, y, 400, 20), "[F1] Test basic coroutine"); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), "[F2] Test WaitForSeconds"); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), "[F3] Test yield null"); y += 20;
        
        if (testInProgress)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, y + 20, 400, 20), "TEST IN PROGRESS...");
        }
    }
}