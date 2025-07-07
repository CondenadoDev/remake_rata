using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CombatSystemPlayer : MonoBehaviour
{
    [Header("Arma")]
    public MeleeWeapon sword;          // asigna en el inspector

    [Header("Timings")]
    public float recoverTime = 0.4f;

    private Animator animator;
    public  bool    IsAttacking { get; private set; }

    void Awake() => animator = GetComponent<Animator>();

    public void TryAttack()
    {
        Debug.Log("<color=yellow>→ TryAttack()</color>");

        if (IsAttacking)
        {
            Debug.Log("<color=red>Bloqueado</color>: IsAttacking aún true");
            return;
        }
        if (sword == null)
        {
            Debug.LogError("❌ Sin espada asignada");
            return;
        }
        if (!sword.CanAttack())
        {
            Debug.Log("<color=red>Cooldown del arma</color>");
            return;
        }

        IsAttacking = true;
        Debug.Log("<color=lime>▶ ATACAR (trigger)</color>");
        animator.SetTrigger("Attack");
        sword.Attack();            // activa la rutina del hitbox
        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(recoverTime);
        IsAttacking = false;
        Debug.Log("<color=gray>■ Fin bloqueo de ataque</color>");
    }
}