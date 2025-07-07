using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName = "Espada";
    public float  attackDamage   = 25f;
    public float  attackCooldown = 0.6f;

    private float lastAttackTime = -Mathf.Infinity;

    public abstract void Attack();

    public bool  CanAttack()         => Time.time >= lastAttackTime + attackCooldown;
    protected void UpdateAttackTime() => lastAttackTime = Time.time;
    public    void ResetCooldown()    => lastAttackTime = Time.time - attackCooldown;
}