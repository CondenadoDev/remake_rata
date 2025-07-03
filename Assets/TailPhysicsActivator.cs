using UnityEngine;

public class TailPhysicsActivator : MonoBehaviour
{
    public MonoBehaviour tailPhysicsScript; // Asigna manualmente el TailPhysicsSetup en el inspector
    public float delay = 1f;

    void Start()
    {
        tailPhysicsScript.enabled = false; // Por seguridad
        Invoke(nameof(ActivateTailPhysics), delay);
    }

    void ActivateTailPhysics()
    {
        tailPhysicsScript.enabled = true;
        Debug.Log("🐀 TailPhysicsSetup activado después del delay.");
    }
}