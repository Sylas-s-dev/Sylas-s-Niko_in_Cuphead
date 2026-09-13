using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // glisse ton Player ici
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10);

    void LateUpdate()
    {
        Vector3 desired = player.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        // Snap caméra sur la grille pixel
        float ppu = 40f;
        float unitsPerPixel = 1f / ppu;
        smoothed.x = Mathf.Round(smoothed.x / unitsPerPixel) * unitsPerPixel;
        smoothed.y = Mathf.Round(smoothed.y / unitsPerPixel) * unitsPerPixel;

        transform.position = smoothed;
    }
}