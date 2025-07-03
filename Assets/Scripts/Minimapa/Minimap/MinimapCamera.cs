using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        
        Vector3 newPosition = player.position;
        
        newPosition.y = transform.position.y;
        
        transform.position = newPosition;
        
    }
}
