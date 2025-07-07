using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    private PlayerConfig config;
    private HealthSystem healthSystem;
    
    // Stats actuales
    private float currentHealth;
    private float currentStamina;
    
    // Regeneración
    private float lastDamageTime;
    private float lastStaminaUseTime;
    private bool isRegeneratingHealth;
    private bool isRegeneratingStamina;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => config.maxHealth;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => config.maxStamina;
    public float HealthPercentage => currentHealth / config.maxHealth;
    public float StaminaPercentage => currentStamina / config.maxStamina;
    
    // Eventos
    public static event System.Action<float, float> OnHealthChanged;
    public static event System.Action<float, float> OnStaminaChanged;
    
    void Awake()
    {
        config = ConfigurationManager.Player;
        healthSystem = GetComponent<HealthSystem>();
        
        // Inicializar stats
        currentHealth = config.maxHealth;
        currentStamina = config.maxStamina;
        
        // Suscribirse a eventos de salud
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(OnHealthSystemChanged);
        }
    }
    
    void Start()
    {
        // Disparar eventos iniciales
        OnHealthChanged?.Invoke(currentHealth, config.maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, config.maxStamina);
    }
    
    public void UpdateStats()
    {
        HandleHealthRegeneration();
        HandleStaminaRegeneration();
    }
    
    void HandleHealthRegeneration()
    {
        if (currentHealth < config.maxHealth && 
            Time.time - lastDamageTime >= config.healthRegenDelay)
        {
            if (!isRegeneratingHealth)
            {
                StartCoroutine(RegenerateHealth());
            }
        }
    }
    
    void HandleStaminaRegeneration()
    {
        if (currentStamina < config.maxStamina && 
            Time.time - lastStaminaUseTime >= config.staminaRegenDelay)
        {
            if (!isRegeneratingStamina)
            {
                StartCoroutine(RegenerateStamina());
            }
        }
    }
    
    IEnumerator RegenerateHealth()
    {
        isRegeneratingHealth = true;
        
        while (currentHealth < config.maxHealth && 
               Time.time - lastDamageTime >= config.healthRegenDelay)
        {
            currentHealth += config.healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, config.maxHealth);
            
            OnHealthChanged?.Invoke(currentHealth, config.maxHealth);
            
            yield return null;
        }
        
        isRegeneratingHealth = false;
    }
    
    IEnumerator RegenerateStamina()
    {
        isRegeneratingStamina = true;
        
        while (currentStamina < config.maxStamina && 
               Time.time - lastStaminaUseTime >= config.staminaRegenDelay)
        {
            currentStamina += config.staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, config.maxStamina);
            
            OnStaminaChanged?.Invoke(currentStamina, config.maxStamina);
            
            yield return null;
        }
        
        isRegeneratingStamina = false;
    }
    
    public bool CanSprint()
    {
        return currentStamina >= config.runStaminaCost * Time.deltaTime;
    }
    
    public bool CanConsummeStamina(float amount)
    {
        return currentStamina >= amount;
    }
    
    public void ConsumeStamina(float amount)
    {
        if (amount <= 0) return;
        
        currentStamina -= amount;
        currentStamina = Mathf.Max(0f, currentStamina);
        
        lastStaminaUseTime = Time.time;
        
        OnStaminaChanged?.Invoke(currentStamina, config.maxStamina);
    }
    
    public void RestoreStamina(float amount)
    {
        if (amount <= 0) return;
        
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, config.maxStamina);
        
        OnStaminaChanged?.Invoke(currentStamina, config.maxStamina);
    }
    
    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        lastDamageTime = Time.time;
        
        OnHealthChanged?.Invoke(currentHealth, config.maxHealth);
        
        // Sincronizar con HealthSystem si existe
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
        }
    }
    
    public void RestoreHealth(float amount)
    {
        if (amount <= 0) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, config.maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, config.maxHealth);
        
        // Sincronizar con HealthSystem si existe
        if (healthSystem != null)
        {
            healthSystem.RestoreHealth(amount);
        }
    }
    
    void OnHealthSystemChanged(float newHealth, float maxHealth)
    {
        // Sincronizar con el HealthSystem existente
        currentHealth = newHealth;
        OnHealthChanged?.Invoke(currentHealth, config.maxHealth);
    }
    
    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(OnHealthSystemChanged);
        }
    }
}