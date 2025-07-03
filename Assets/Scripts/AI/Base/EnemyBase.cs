using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }
    public State currentState = State.Patrol;

    [Header("Parámetros Generales")]
    public float detectionRadius = 10f;       // Radio de detección del jugador
    public float moveSpeed = 2f;              // Velocidad de movimiento
    public Transform player;                  // Referencia al jugador

    [Header("Patrulla")]
    public float patrolRange = 3f;            // Distancia máxima para desplazamientos aleatorios
    public float patrolWaitTimeMin = 2f;      // Tiempo mínimo de espera
    public float patrolWaitTimeMax = 3f;      // Tiempo máximo de espera

    protected Vector3 patrolTarget;

    private HealthSystem healthSystem; // Sistema de salud
    private Coroutine patrolRoutine;

    protected virtual void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
            healthSystem.OnDeath.AddListener(OnDeath);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        patrolRoutine = StartCoroutine(PatrolRoutine());
    }

    protected virtual void Update()
    {
        if (currentState == State.Patrol && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= detectionRadius)
                ActivateChase();
        }

        if (player != null && (currentState == State.Chase || currentState == State.Attack))
        {
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);
        }

        switch (currentState)
        {
            case State.Chase:
                ChaseBehavior();
                break;
            case State.Attack:
                AttackBehavior();
                break;
            // En Patrol, la corrutina se encarga
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (currentState == State.Patrol)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-patrolRange, patrolRange), 0, Random.Range(-patrolRange, patrolRange));
            patrolTarget = transform.position + randomOffset;

            // Moverse hacia el objetivo de patrulla
            while (Vector3.Distance(transform.position, patrolTarget) > 0.1f && currentState == State.Patrol)
            {
                Vector3 nextPos = Vector3.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);
                transform.position = nextPos;
                yield return null;
            }

            // Espera aleatoria
            float waitTime = Random.Range(patrolWaitTimeMin, patrolWaitTimeMax);
            float elapsed = 0f;
            while (elapsed < waitTime && currentState == State.Patrol)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    protected virtual void ChaseBehavior()
    {
        if (player == null) return;

        Vector3 targetPos = player.position;
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.position = newPos;

        if (Vector3.Distance(transform.position, targetPos) <= GetAttackRange())
            currentState = State.Attack;
    }

    protected abstract void AttackBehavior();

    protected virtual float GetAttackRange()
    {
        return 1.5f;
    }

    public virtual void TakeDamage(float damage)
    {
        if (healthSystem != null)
            healthSystem.TakeDamage(damage);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void ActivateChase()
    {
        if (currentState == State.Patrol)
        {
            currentState = State.Chase;
            if (patrolRoutine != null)
            {
                StopCoroutine(patrolRoutine);
                patrolRoutine = null;
            }
        }
    }
}
