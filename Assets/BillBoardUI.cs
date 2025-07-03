using UnityEngine;

/// <summary>Hace que el Canvas mire siempre a la cámara principal.</summary>
public class BillboardUI : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main)
            transform.forward = Camera.main.transform.forward;
    }
}