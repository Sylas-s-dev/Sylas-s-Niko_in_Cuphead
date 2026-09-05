using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 7f;
    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * speed;
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            var player = col.GetComponent<PlayerController2D>();
            if (player != null && player.IsInvincible) return; // traverse, ne détruit pas

            player.TakeDamage(1);
            Destroy(gameObject); // détruit seulement si tu n'es PAS en dash
        }
    }
}