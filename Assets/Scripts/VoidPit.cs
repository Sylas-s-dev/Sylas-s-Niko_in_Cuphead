using UnityEngine;

public class VoidPit : MonoBehaviour
{
    public Transform player;
    public int damage = 1;
    private Transform respawnPoint;
    private bool canHit = true;

    void Start()
    {
        if (player != null)
        {
            GameObject go = new GameObject("RespawnPoint_P3");
            go.transform.position = player.position;
            respawnPoint = go.transform;
        }
    }

    // Marche si le sol est en Trigger
    void OnTriggerEnter2D(Collider2D other) => TryPunish(other.gameObject);
    // Marche si le sol est en Solide (ton cas actuel)
    void OnCollisionEnter2D(Collision2D other) => TryPunish(other.gameObject);

    void TryPunish(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        if (!canHit) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) pc.TakeDamage(damage);

        if (respawnPoint != null)
            other.transform.position = respawnPoint.position;

        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        canHit = false;
        Invoke(nameof(ResetHit), 0.5f);
    }

    void ResetHit() => canHit = true;
}