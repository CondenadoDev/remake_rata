using UnityEngine;

// Enumerador para la calidad de audio (AudioConfig)
public enum AudioQuality
{
    Baja = 0,     // Low
    Media = 1,    // Medium  
    Alta = 2,     // High
    Ultra = 3     // Ultra
}

// Enumerador para el nivel de dificultad (GameplayConfig)
public enum DifficultyLevel
{
    Facil = 0,      // Easy
    Normal = 1,     // Normal
    Dificil = 2,    // Hard
    Pesadilla = 3   // Nightmare
}

// Enumerador para los niveles de calidad gráfica (GraphicsConfig)
public enum QualityLevel
{
    MuyBajo = 0,   // Very Low
    Bajo = 1,      // Low
    Medio = 2,     // Medium
    Alto = 3,      // High
    MuyAlto = 4,   // Very High
    Ultra = 5      // Ultra
}

// Enumerador para la resolución de sombras (GraphicsConfig) 
// Nota: Renombrado para evitar conflicto con UnityEngine.ShadowResolution
public enum CustomShadowResolution
{
    Baja = 0,      // Low
    Media = 1,     // Medium
    Alta = 2,      // High
    MuyAlta = 3    // Very High
}

// Enumerador para el modo de pantalla completa (GraphicsConfig)
// Nota: Renombrado para evitar conflicto con UnityEngine.FullScreenMode
public enum CustomFullScreenMode
{
    PantallaCompletaExclusiva = 0,  // Exclusive FullScreen
    VentanaPantallaCompleta = 1,    // FullScreen Window
    VentanaMaximizada = 2,          // Maximized Window
    Ventana = 3                     // Windowed
}