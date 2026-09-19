using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 7f;
    private bool isP3 = false;
    public bool isWallBullet = false;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        // pour pas que le dash les casse avec les collisions physiques
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        // Si le Boss ne m'a PAS parlé, c'est une balle P1/P2 -> va à gauche
        if (!isP3)
        {
            rb.linearVelocity = Vector2.left * speed;
        }
        Destroy(gameObject, 6f);
    }

    public void SetP3Velocity(Vector2 dir, float p3Speed)
    {
        isP3 = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = dir.normalized * p3Speed;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            var player = col.GetComponent<PlayerController2D>();
            if (player == null) return;

            // Si Niko dash ou est en invincibilité de hit, la balle le traverse sans se casser
            if (player.IsInvincible)
            {
                return;
            }

            player.TakeDamage(1);

            // Le mur ne se casse pas, les petites balles oui
            if (!isWallBullet)
                Destroy(gameObject);
        }
        // Si tu veux que les balles se détruisent contre le sol, ajoute ça
        else if (col.CompareTag("Ground"))
        {
            if (!isWallBullet)
                Destroy(gameObject);
        }
    }
}