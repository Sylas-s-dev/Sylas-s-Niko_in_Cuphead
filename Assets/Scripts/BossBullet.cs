using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 7f;
    private bool isP3 = false;

    void Awake()
    {
        // On récupère le RB dès le début
        if (GetComponent<Rigidbody2D>() == null) gameObject.AddComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Si le Boss ne m'a PAS parlé, c'est une balle P1/P2 -> va à gauche
        if (!isP3)
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.left * speed;
        }
        Destroy(gameObject, 6f);
    }

    // Appelé par le Boss pour la P3 - cette fonction DOIT passer avant le Start()
    public void SetP3Velocity(Vector2 dir, float p3Speed)
    {
        isP3 = true; // important : on dit que c'est une balle P3
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = dir.normalized * p3Speed;
        Debug.Log("Balle P3 spawn dir: " + dir + " vitesse: " + rb.linearVelocity, gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            var player = col.GetComponent<PlayerController2D>();
            if (player != null && player.IsInvincible) return;
            player.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}