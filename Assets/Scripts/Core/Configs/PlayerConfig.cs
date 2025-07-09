using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Configs/Player Config")]
public class PlayerConfig : ConfigBase
{
    [Header("🚶 Movement")]
    [UIOption("Velocidad Caminar", UIControlType.Slider, 0.1f, 10f, "Movement", 1)]
    public float walkSpeed = 2f;
    
    [UIOption("Velocidad Correr", UIControlType.Slider, 1f, 20f, "Movement", 2)]
    public float runSpeed = 5f;
    
    [UIOption("Velocidad Rotación", UIControlType.Slider, 1f, 50f, "Movement", 3)]
    public float rotationSpeed = 10f;
    
    [UIOption("Aceleración", UIControlType.Slider, 1f, 50f, "Movement", 4)]
    public float acceleration = 15f;
    
    [UIOption("Desaceleración", UIControlType.Slider, 1f, 50f, "Movement", 5)]
    public float deceleration = 20f;
    
    [Header("🤸 Dodge/Dash")]
    [UIOption("Duración Esquive", UIControlType.Slider, 0.1f, 2f, "Dodge", 10)]
    public float dodgeDuration = 0.5f;
    
    [UIOption("Velocidad Esquive", UIControlType.Slider, 5f, 30f, "Dodge", 11)]
    public float dodgeSpeed = 10f;
    
    [UIOption("Cooldown Esquive", UIControlType.Slider, 0f, 5f, "Dodge", 12)]
    public float dodgeCooldown = 1f;
    
    [UIOption("Duración Invulnerabilidad", UIControlType.Slider, 0f, 2f, "Dodge", 13)]
    public float invulnerabilityDuration = 0.3f;
    
    [Header("⚔️ Combat")]
    [UIOption("Daño Ataque", UIControlType.Slider, 1f, 100f, "Combat", 20)]
    public float attackDamage = 25f;
    
    [UIOption("Cooldown Ataque", UIControlType.Slider, 0.1f, 3f, "Combat", 21)]
    public float attackCooldown = 0.6f;
    
    [UIOption("Ventana Combo", UIControlType.Slider, 0.5f, 5f, "Combat", 22)]
    public float comboWindow = 1.5f;
    
    [UIOption("Max Combos", UIControlType.Slider, 1f, 10f, "Combat", 23)]
    public int maxComboCount = 3;
    
    [Header("💚 Health")]
    [UIOption("Vida Máxima", UIControlType.Slider, 10f, 1000f, "Health", 30)]
    public float maxHealth = 100f;
    
    [UIOption("Regeneración Vida", UIControlType.Slider, 0f, 50f, "Health", 31)]
    public float healthRegenRate = 5f;
    
    [UIOption("Delay Regeneración", UIControlType.Slider, 0f, 10f, "Health", 32)]
    public float healthRegenDelay = 3f;
    
    [Header("⚡ Stamina")]
    [UIOption("Stamina Máxima", UIControlType.Slider, 10f, 500f, "Stamina", 40)]
    public float maxStamina = 100f;
    
    [UIOption("Regeneración Stamina", UIControlType.Slider, 5f, 100f, "Stamina", 41)]
    public float staminaRegenRate = 20f;
    
    [UIOption("Delay Regen Stamina", UIControlType.Slider, 0f, 5f, "Stamina", 42)]
    public float staminaRegenDelay = 1f;
    
    [UIOption("Costo Stamina Correr", UIControlType.Slider, 0f, 50f, "Stamina", 43)]
    public float runStaminaCost = 15f;
    
    [UIOption("Costo Stamina Esquive", UIControlType.Slider, 0f, 100f, "Stamina", 44)]
    public float dodgeStaminaCost = 25f;
    
    [UIOption("Costo Stamina Ataque", UIControlType.Slider, 0f, 50f, "Stamina", 45)]
    public float attackStaminaCost = 10f;
    
    [Header("🎯 Targeting")]
    [UIOption("Rango Lock-On", UIControlType.Slider, 5f, 50f, "Targeting", 50)]
    public float lockOnRange = 15f;
    
    [UIOption("Rango Perder Lock-On", UIControlType.Slider, 10f, 100f, "Targeting", 51)]
    public float lockOnLoseRange = 20f;
    
    // LayerMask es complejo, se omite de UI automática
    public LayerMask targetLayers = -1;

    public override void ValidateValues()
    {
        walkSpeed = Mathf.Max(0.1f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
        
        dodgeDuration = Mathf.Max(0.1f, dodgeDuration);
        dodgeSpeed = Mathf.Max(0.1f, dodgeSpeed);
        dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
        
        attackDamage = Mathf.Max(1f, attackDamage);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
        
        maxHealth = Mathf.Max(1f, maxHealth);
        maxStamina = Mathf.Max(1f, maxStamina);
    }
}