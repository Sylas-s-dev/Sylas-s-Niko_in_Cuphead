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
        // C'est le FirePoint qui donne la direction maintenant (souris)
        rb.linearVelocity = transform.right * bulletSpeed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BossBullet")) return;
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Boss"))
        {
            other.GetComponent<Boss>()?.TakeDamage(1);
        }

        // Mur, sol, boss = on détruit
        Destroy(gameObject);
    }
}