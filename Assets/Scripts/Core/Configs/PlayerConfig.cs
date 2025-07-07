using UnityEngine;
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Configs/Player Config")]
public class PlayerConfig : ConfigurationBase
{
    [Header("🚶 Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    
    [Header("🤸 Dodge/Dash")]
    public float dodgeDuration = 0.5f;
    public float dodgeSpeed = 10f;
    public float dodgeCooldown = 1f;
    public float invulnerabilityDuration = 0.3f;
    
    [Header("⚔️ Combat")]
    public float attackDamage = 25f;
    public float attackCooldown = 0.6f;
    public float comboWindow = 1.5f;
    public int maxComboCount = 3;
    
    [Header("💚 Health")]
    public float maxHealth = 100f;
    public float healthRegenRate = 5f;
    public float healthRegenDelay = 3f;
    
    [Header("⚡ Stamina")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 20f;
    public float staminaRegenDelay = 1f;
    public float runStaminaCost = 15f;
    public float dodgeStaminaCost = 25f;
    public float attackStaminaCost = 10f;
    
    [Header("🎯 Targeting")]
    public float lockOnRange = 15f;
    public float lockOnLoseRange = 20f;
    public LayerMask targetLayers = -1;
    
    protected override void ValidateValues()
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
    
    public override void ResetToDefaults()
    {
        walkSpeed = 2f;
        runSpeed = 5f;
        rotationSpeed = 10f;
        dodgeDuration = 0.5f;
        dodgeSpeed = 10f;
        attackDamage = 25f;
        attackCooldown = 0.6f;
        maxHealth = 100f;
        maxStamina = 100f;
    }
}