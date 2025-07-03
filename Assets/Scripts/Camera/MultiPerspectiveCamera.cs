using UnityEngine;

public class RPGCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform thirdPersonTarget;
    [SerializeField] private Transform firstPersonTarget;
    [Tooltip("Asigna el objeto MESH, NO el root del player")]
    [SerializeField] private GameObject[] meshesToHide;

    [Header("Camera Settings")]
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float minDistance = 1.2f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomSmooth = 0.08f;
    [SerializeField] private float mouseSmooth = 0.04f;
    [SerializeField] private Vector2 sensitivity = new Vector2(1.6f, 1.2f);
    [SerializeField] private KeyCode switchKey = KeyCode.Q;
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float minCamDistance = 0.25f;

    // Internals
    private float yaw = 0;
    private float pitch = 0;
    private float desiredDistance;
    private float currentDistance;
    private Vector2 currentMouseDelta;
    private Vector2 smoothMouseDelta;
    private Transform followTarget;
    private bool isThirdPerson = true;
    private Transform playerRoot;

    void Awake()
    {
        if (thirdPersonTarget == null || firstPersonTarget == null)
            Debug.LogError("Camera targets not assigned.");

        followTarget = thirdPersonTarget;
        desiredDistance = currentDistance = Mathf.Lerp(minDistance, maxDistance, 0.5f);

        yaw = followTarget.eulerAngles.y;
        pitch = 15f; // Leve inclinación inicial

        // Encuentra el root del player
        playerRoot = thirdPersonTarget.root;

        SetMeshesVisibility(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandlePerspectiveSwitch();
        HandleCameraInput();
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void HandlePerspectiveSwitch()
    {
        if (Input.GetKeyDown(switchKey))
        {
            isThirdPerson = !isThirdPerson;
            followTarget = isThirdPerson ? thirdPersonTarget : firstPersonTarget;
            SetMeshesVisibility(isThirdPerson);

            // Ajusta el zoom al cambiar
            desiredDistance = isThirdPerson ? Mathf.Clamp(currentDistance, minDistance, maxDistance) : minCamDistance;
        }
    }

    void HandleCameraInput()
    {
        // Mouse input suavizado (mejor sensación)
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        smoothMouseDelta = Vector2.Lerp(smoothMouseDelta, mouseDelta, 1f / mouseSmooth);

        yaw += smoothMouseDelta.x * sensitivity.x;
        pitch -= smoothMouseDelta.y * sensitivity.y;
        pitch = Mathf.Clamp(pitch, -65f, 75f);

        // Rota el jugador sólo en primera persona
        if (!isThirdPerson && playerRoot != null)
            playerRoot.rotation = Quaternion.Euler(0, yaw, 0);

        // Zoom solo en tercera persona
        if (isThirdPerson)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                desiredDistance -= scroll * zoomSpeed;
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            }
        }
        else
        {
            desiredDistance = minCamDistance;
        }
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, zoomSmooth);
    }

    void UpdateCameraPosition()
    {
        if (followTarget == null) return;

        // En tercera persona, sigue al target desde atrás; en primera, está sobre el target
        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPos = followTarget.position;
        Vector3 desiredCamPos = targetPos - camRot * Vector3.forward * currentDistance;

        // Previene clipping (SphereCast mejor que Raycast)
        Vector3 camDir = (desiredCamPos - targetPos).normalized;
        float castDist = currentDistance + cameraRadius;
        if (Physics.SphereCast(targetPos, cameraRadius, camDir, out RaycastHit hit, castDist, cameraCollisionMask))
        {
            float correctedDist = hit.distance - cameraRadius;
            if (correctedDist < minCamDistance) correctedDist = minCamDistance;
            currentDistance = Mathf.Lerp(currentDistance, correctedDist, 0.5f);
            desiredCamPos = targetPos + camDir * correctedDist;
        }

        // Suaviza la transición
        transform.position = Vector3.Lerp(transform.position, desiredCamPos, 1f - Mathf.Exp(-16f * Time.deltaTime));
        transform.rotation = camRot;
    }

    void SetMeshesVisibility(bool visible)
    {
        // Solo oculta los MESH, nunca el objeto player
        if (meshesToHide == null) return;
        foreach (var go in meshesToHide)
        {
            if (go == null) continue;
            go.SetActive(visible);
        }
    }
}
