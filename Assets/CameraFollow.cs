using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // glisse ton Player ici
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10);

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}