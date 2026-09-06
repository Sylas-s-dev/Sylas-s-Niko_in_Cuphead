using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 15f;
    public float lifeTime = 2f;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BossBullet")) return;
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Bullet")) return;

        // --- FIX : on traverse les plateformes bleues ---
        // Même si tag = Ground, si c'est le layer Platform ou si ça a un PlatformEffector, on ignore
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform")) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ignore platform")) return;
        if (other.GetComponent<PlatformEffector2D>() != null) return;
        if (other.GetComponentInParent<PlatformEffector2D>() != null) return;

        if (other.CompareTag("Boss"))
        {
            other.GetComponent<Boss>()?.TakeDamage(1);
        }

        // Ici on ne détruit que pour vrai mur / sol / boss
        Destroy(gameObject);
    }
}