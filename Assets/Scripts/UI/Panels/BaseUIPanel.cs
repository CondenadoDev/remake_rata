using UnityEngine;

/// <summary>
/// Clase concreta base para paneles UI genéricos
/// </summary>
public class BaseUIPanel : UIPanel
{
    protected override void OnInitialize()
    {
        // Implementación básica
        LogDebug($"Base UI Panel initialized: {panelID}");
    }
}