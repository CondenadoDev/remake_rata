using UnityEngine;

/// <summary>
/// GUÍA SIMPLE DE IMPLEMENTACIÓN DE COLA DE RATÓN
/// 
/// PASO 1: Añadir este script (AutoMouseTailSetup) al GameObject que contiene la cola
/// PASO 2: Ejecutar el juego - TODO se configura automáticamente
/// PASO 3: (Opcional) Ajustar el estilo de movimiento en el inspector
/// 
/// ¡ESO ES TODO! La cola se optimizará sola y se comportará como la de un ratón real.
/// </summary>

[System.Serializable]
public class MouseTailGuide : MonoBehaviour
{
    [Header("📋 INSTRUCCIONES DE USO")]
    [TextArea(10, 15)]
    public string instructions = 
        "🐭 COLA DE RATÓN - CONFIGURACIÓN AUTOMÁTICA\n\n" +
        "✅ PASO 1: Asegúrate de que este script esté en el GameObject de la cola\n" +
        "✅ PASO 2: Ejecuta el juego - se configura automáticamente\n" +
        "✅ PASO 3: ¡Disfruta de tu cola realista!\n\n" +
        "🎛️ CONFIGURACIONES OPCIONALES:\n" +
        "• Movement Style: Automático detecta la velocidad del ratón\n" +
        "• Slow: Para ratones lentos/pesados\n" +
        "• Normal: Comportamiento estándar\n" +
        "• Fast: Para ratones ágiles/rápidos\n\n" +
        "🔧 COMANDOS ÚTILES (Click derecho en el componente):\n" +
        "• 'Reconfigurar Automáticamente': Vuelve a optimizar todo\n" +
        "• 'Mostrar Estado Completo': Ver información de diagnóstico\n\n" +
        "❓ SOLUCIÓN DE PROBLEMAS:\n" +
        "• Si la cola no aparece: Verifica que 'BASECOLA' exista\n" +
        "• Si se estira: El sistema se auto-corrige automáticamente\n" +
        "• Si no se mueve: Verifica que el personaje tenga Rigidbody\n\n" +
        "🎯 RESULTADO ESPERADO:\n" +
        "• Cola que siempre va hacia atrás del personaje\n" +
        "• Movimiento natural y fluido como un ratón real\n" +
        "• Auto-optimización sin intervención manual\n" +
        "• Adaptación automática al estilo de movimiento";

    [Header("🎯 Estado Actual")]
    [SerializeField] private bool isConfigured = false;
    [SerializeField] private string currentStatus = "Esperando configuración...";
    
    private AutoMouseTailSetup autoSetup;

    void Start()
    {
        // Verificar si AutoMouseTailSetup está presente
        autoSetup = GetComponent<AutoMouseTailSetup>();
        
        if (autoSetup == null)
        {
            Debug.LogWarning("⚠️ AutoMouseTailSetup no encontrado. Añadiéndolo automáticamente...");
            autoSetup = gameObject.AddComponent<AutoMouseTailSetup>();
        }
        
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (autoSetup != null)
        {
            currentStatus = "✅ AutoMouseTailSetup detectado - Configuración en progreso...";
            isConfigured = true;
        }
        else
        {
            currentStatus = "❌ Requiere AutoMouseTailSetup";
            isConfigured = false;
        }
    }

    [ContextMenu("🚀 Configuración Rápida")]
    public void QuickSetup()
    {
        Debug.Log("🚀 === CONFIGURACIÓN RÁPIDA DE COLA DE RATÓN ===");
        
        // Verificar AutoMouseTailSetup
        if (autoSetup == null)
        {
            autoSetup = gameObject.AddComponent<AutoMouseTailSetup>();
            Debug.Log("➕ AutoMouseTailSetup añadido");
        }
        
        // Configurar para auto-setup
        autoSetup.autoSetupOnStart = true;
        autoSetup.movementStyle = MouseMovementStyle.Automatic;
        autoSetup.adaptiveReconfiguration = true;
        
        Debug.Log("✅ Configuración rápida completada");
        Debug.Log("🎮 Ejecuta el juego para ver la cola en acción");
        
        UpdateStatus();
    }

    [ContextMenu("📖 Mostrar Ayuda Completa")]
    public void ShowFullHelp()
    {
        Debug.Log("📖 === AYUDA COMPLETA - COLA DE RATÓN ===");
        Debug.Log("");
        Debug.Log("🎯 OBJETIVO:");
        Debug.Log("   Crear una cola que se comporte como la de un ratón real");
        Debug.Log("   - Siempre hacia atrás del personaje");
        Debug.Log("   - Movimiento natural y fluido");
        Debug.Log("   - Auto-optimización automática");
        Debug.Log("");
        Debug.Log("⚙️ COMPONENTES INCLUIDOS:");
        Debug.Log("   • AutoMouseTailSetup: Configuración automática principal");
        Debug.Log("   • RealisticMouseTail: Comportamiento natural de ratón");
        Debug.Log("   • TailPhysicsSetup: Física básica de la cola");
        Debug.Log("   • TailStretchPreventer: Previene estiramiento excesivo");
        Debug.Log("   • TailJointStabilizer: Estabiliza las conexiones");
        Debug.Log("");
        Debug.Log("🔧 CONFIGURACIÓN:");
        Debug.Log("   1. Añade AutoMouseTailSetup a tu GameObject de cola");
        Debug.Log("   2. Ejecuta el juego");
        Debug.Log("   3. ¡Todo se configura automáticamente!");
        Debug.Log("");
        Debug.Log("🎛️ ESTILOS DE MOVIMIENTO:");
        Debug.Log("   • Automatic: Detecta automáticamente (recomendado)");
        Debug.Log("   • Slow: Ratón lento (masa alta, movimiento pausado)");
        Debug.Log("   • Normal: Comportamiento estándar");
        Debug.Log("   • Fast: Ratón ágil (masa baja, movimiento rápido)");
        Debug.Log("");
        Debug.Log("🐛 SOLUCIÓN DE PROBLEMAS:");
        Debug.Log("   • Cola no aparece: Verifica jerarquía BASECOLA/Cola_X");
        Debug.Log("   • Se estira mucho: El sistema se auto-corrige");
        Debug.Log("   • No sigue al personaje: Verifica Rigidbody del personaje");
        Debug.Log("   • Muy rígida/blanda: Cambia Movement Style");
        Debug.Log("");
        Debug.Log("📞 COMANDOS DE EMERGENCIA:");
        Debug.Log("   • 'Configuración Rápida': Configura todo automáticamente");
        Debug.Log("   • 'Reconfigurar Automáticamente': Vuelve a optimizar");
        Debug.Log("   • 'Mostrar Estado Completo': Diagnóstico completo");
    }

    [ContextMenu("🔍 Verificar Requisitos")]
    public void CheckRequirements()
    {
        Debug.Log("🔍 === VERIFICANDO REQUISITOS ===");
        
        bool allGood = true;
        
        // Verificar jerarquía de cola
        Transform baseCola = transform.Find("BASECOLA");
        if (baseCola != null)
        {
            Debug.Log("✅ BASECOLA encontrado");
            
            // Verificar huesos de cola
            int boneCount = 0;
            for (int i = 1; i <= 8; i++)
            {
                if (FindBoneRecursive(baseCola, $"Cola_{i}") != null)
                    boneCount++;
            }
            
            Debug.Log($"✅ {boneCount} huesos de cola encontrados");
        }
        else
        {
            Debug.LogWarning("❌ BASECOLA no encontrado en la jerarquía");
            allGood = false;
        }
        
        // Verificar personaje
        Transform character = transform.parent;
        if (character != null)
        {
            Rigidbody rb = character.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log($"✅ Rigidbody del personaje encontrado (Kinematic: {rb.isKinematic})");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró Rigidbody en el personaje");
            }
        }
        
        // Verificar AutoMouseTailSetup
        if (autoSetup != null)
        {
            Debug.Log("✅ AutoMouseTailSetup presente");
        }
        else
        {
            Debug.LogWarning("❌ AutoMouseTailSetup no encontrado");
            allGood = false;
        }
        
        if (allGood)
        {
            Debug.Log("🎉 ¡Todos los requisitos cumplidos! Ejecuta el juego para ver la cola en acción.");
        }
        else
        {
            Debug.Log("⚠️ Algunos requisitos no se cumplen. Usa 'Configuración Rápida' para corregirlos.");
        }
    }

    Transform FindBoneRecursive(Transform parent, string boneName)
    {
        if (parent.name == boneName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindBoneRecursive(parent.GetChild(i), boneName);
            if (result != null)
                return result;
        }

        return null;
    }

    void OnValidate()
    {
        UpdateStatus();
    }
}

/*
===============================================
🐭 RESUMEN PARA EL USUARIO
===============================================

IMPLEMENTACIÓN SÚPER SIMPLE:

1. 📁 Añade el script "AutoMouseTailSetup" a tu GameObject de cola
2. ▶️ Ejecuta el juego  
3. 🎉 ¡La cola se comporta como la de un ratón real!

CARACTERÍSTICAS AUTOMÁTICAS:
✅ Siempre va hacia atrás del personaje
✅ Movimiento natural y fluido
✅ Se adapta automáticamente al estilo de movimiento
✅ Previene estiramiento excesivo
✅ Optimización automática de física
✅ No requiere configuración manual

ESTILOS DE MOVIMIENTO:
🔄 Automatic: Detecta automáticamente (RECOMENDADO)
🐌 Slow: Para ratones lentos/pesados  
🐭 Normal: Comportamiento estándar
⚡ Fast: Para ratones ágiles/rápidos

¡ESO ES TODO! Tu cola de ratón estará lista para usar.
===============================================
*/