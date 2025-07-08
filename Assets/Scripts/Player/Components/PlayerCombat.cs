using UnityEngine;
using System.Collections;
using UISystem.Configuration;

public class PlayerCombat : MonoBehaviour
{
    [Header("🗡️ Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackRange = 1.5f;
    
    private PlayerConfig config;
    private Animator animator;
    
    // Combat state
    private float lastAttackTime = -Mathf.Infinity;
    private float lastDodgeTime = -Mathf.Infinity;
    private bool isInvulnerable;
    private int comboCount;
    private float lastComboTime;
    
    // Properties
    public bool IsInvulnerable => isInvulnerable;
    public int ComboCount => comboCount;
    
    void Awake()
    {
        config = ConfigurationManager.Instance.Player;
        animator = GetComponent<Animator>();
        
        // Crear punto de ataque si no existe
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("Attack Point");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = Vector3.forward;
            attackPoint = attackPointObj.transform;
        }
    }
    
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + config.attackCooldown && !isInvulnerable;
    }
    
    public bool CanDodge()
    {
        return Time.time >= lastDodgeTime + config.dodgeCooldown;
    }
    
    public bool CanCombo()
    {
        return comboCount < config.maxComboCount && 
               Time.time - lastComboTime <= config.comboWindow;
    }
    
    public void PerformAttack()
    {
        lastAttackTime = Time.time;
        lastComboTime = Time.time;
        comboCount++;
        
        // Reset combo si excede el máximo
        if (comboCount > config.maxComboCount)
        {
            comboCount = 1;
        }
        
        // Ejecutar ataque después del delay
        StartCoroutine(ExecuteAttackAfterDelay(0.2f));
        
        Debug.Log($"⚔️ Attack {comboCount} performed!");
    }
    
    public void PerformCombo()
    {
        PerformAttack();
        Debug.Log($"⚔️ Combo attack {comboCount}!");
    }
    
    IEnumerator ExecuteAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Detectar enemigos en rango
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        
        foreach (Collider enemy in hitEnemies)
        {
            // Aplicar daño
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                float damage = config.attackDamage * comboCount; // Daño incrementa con combo
                enemyHealth.TakeDamage(damage);
                
                Debug.Log($"💥 Dealt {damage} damage to {enemy.name}");
                
                // Efectos visuales/auditivos aquí
                InputHelper.TriggerHapticFeedback(0.7f, 0.3f);
            }
        }
    }
    
    public void SetInvulnerable(bool invulnerable, float duration = 0f)
    {
        isInvulnerable = invulnerable;
        
        if (invulnerable && duration > 0f)
        {
            StartCoroutine(RemoveInvulnerabilityAfterDelay(duration));
        }
    }
    
    IEnumerator RemoveInvulnerabilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInvulnerable = false;
    }
    
    public void ResetCombo()
    {
        comboCount = 0;
    }
    
    void Update()
    {
        // Reset combo si pasa mucho tiempo
        if (Time.time - lastComboTime > config.comboWindow)
        {
            comboCount = 0;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}