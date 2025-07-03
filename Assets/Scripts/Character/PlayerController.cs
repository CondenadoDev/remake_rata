using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CombatSystemPlayer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 2f;
    public float runSpeed  = 5f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;

    [Header("Dash / Esquiva")]
    public float dodgeDuration = 0.5f;
    public float dodgeSpeed    = 10f;

    [Header("Dash Settings")]
    public float dashCooldown = 1f;
    private float lastDashTime = -Mathf.Infinity;
    public  bool  isInvulnerable = false;

    [Header("Input System")]
    public InputActionReference moveAction;
    public InputActionReference sprintAction;
    public InputActionReference attackAction;
    public InputActionReference dodgeAction;

    private CharacterController controller;
    private Animator            animator;
    private CombatSystemPlayer  combat;

    private Vector2 moveInput;
    private bool    isRunning;
    private bool    isDodging;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator   = GetComponent<Animator>();
        combat     = GetComponent<CombatSystemPlayer>();
    }

    /* ---------- INPUT ---------- */

    private void OnAttack(InputAction.CallbackContext _)
    {
        Debug.Log("<color=orange>[INPUT]</color> Se presionó ataque");

        if (isDodging)
        {
            Debug.Log("<color=red>Bloqueado</color>: estás dashing");
            return;
        }
        if (combat.IsAttacking)
        {
            Debug.Log("<color=red>Bloqueado</color>: ya estás atacando");
            return;
        }

        combat.TryAttack();               // continúa cadena
    }

    private void OnDodge(InputAction.CallbackContext _)
    {
        Debug.Log("<color=orange>[INPUT]</color> Se presionó dash");

        if (isDodging || combat.IsAttacking) return;
        if (Time.time < lastDashTime + dashCooldown)
        {
            Debug.Log("<color=red>Dash en cooldown</color>");
            return;
        }

        Vector3 camFwd  = cameraTransform.forward; camFwd.y  = 0; camFwd.Normalize();
        Vector3 camRight= cameraTransform.right;  camRight.y = 0; camRight.Normalize();
        Vector3 dir = camFwd * moveInput.y + camRight * moveInput.x;
        if (dir.sqrMagnitude < 0.1f) dir = transform.forward;

        lastDashTime = Time.time;
        StartCoroutine(DodgeCoroutine(dir.normalized));
    }

    /* ---------- DASH ---------- */

    private IEnumerator DodgeCoroutine(Vector3 dir)
    {
        Debug.Log("<color=cyan>▶ DASH START</color>");
        isDodging      = true;
        isInvulnerable = true;
        animator.SetBool("IsDashing", true);

        float t = 0f;
        while (t < dodgeDuration)
        {
            controller.Move(dir * dodgeSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("IsDashing", false);
        isDodging      = false;
        isInvulnerable = false;
        Debug.Log("<color=cyan>■ DASH END</color>");
    }

    /* ---------- UPDATE MOVIMIENTO (sin cambios) ---------- */

    void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        attackAction.action.Enable();
        dodgeAction.action.Enable();

        attackAction.action.performed += OnAttack;
        dodgeAction.action.performed  += OnDodge;
    }
    void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
        attackAction.action.Disable();
        dodgeAction.action.Disable();

        attackAction.action.performed -= OnAttack;
        dodgeAction.action.performed  -= OnDodge;
    }
    void Update()
    {
        if (isDodging) return;

        moveInput = moveAction.action.ReadValue<Vector2>();
        isRunning = sprintAction.action.IsPressed();

        animator.applyRootMotion = false;

        Vector3 camFwd  = cameraTransform.forward;
        Vector3 camRight= cameraTransform.right;
        camFwd.y  = 0; camRight.y = 0;
        camFwd.Normalize(); camRight.Normalize();

        Vector3 moveDir = camFwd * moveInput.y + camRight * moveInput.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation  = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveSpeed", isRunning ? 1f : moveInput.magnitude * 0.5f, 0.1f, Time.deltaTime);

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        controller.SimpleMove(moveDir * targetSpeed);
    }
}
