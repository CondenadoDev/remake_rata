using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeWeapon : Weapon
{
    [Header("Hitbox")]
    public Collider  hitbox;
    public LayerMask enemyLayer;

    [Header("Timings")]
    public float preHitDelay = 0.15f;
    public float activeTime  = 0.25f;

    private void Awake()
    {
        if (hitbox == null) hitbox = GetComponent<Collider>();
        hitbox.isTrigger = true;
        hitbox.enabled   = false;
    }

    public override void Attack()
    {
        if (!CanAttack()) { Debug.Log("<color=red>Arma en cooldown</color>"); return; }

        UpdateAttackTime();
        Debug.Log("<color=purple>▶ Rutina de golpe</color>");
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(preHitDelay);
        Debug.Log("<color=purple>🔴 Hitbox ON</color>");
        hitbox.enabled = true;

        yield return new WaitForSeconds(activeTime);
        hitbox.enabled = false;
        Debug.Log("<color=purple>⚪ Hitbox OFF</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=white>Contacto con {other.name}</color>");

        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        HealthSystem hs = other.GetComponent<HealthSystem>();
        if (hs != null)
        {
            Debug.Log($"<color=magenta>Daño {attackDamage} a {other.name}</color>");
            hs.TakeDamage(attackDamage);
        }
    }
}