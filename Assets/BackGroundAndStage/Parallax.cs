using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;
    public float parallaxFactor; // 0 = fond qui bouge pas, 1 = devant qui bouge vite

    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        float distance = cam.transform.position.x * parallaxFactor;
        transform.position = new Vector3(startX + distance, transform.position.y, transform.position.z);
    }
}