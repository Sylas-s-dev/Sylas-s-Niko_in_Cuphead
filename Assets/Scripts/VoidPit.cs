using UnityEngine;

public class VoidPit : MonoBehaviour
{
    public Transform player;
    public int damage = 1;
    public float damageInterval = 0.25f;
    private Transform respawnPoint;
    private float lastDamageTime;

    void Start()
    {
        if (player != null)
        {
            GameObject go = new GameObject("RespawnPoint_P3");
            go.transform.position = player.position;
            respawnPoint = go.transform;
        }
    }

    void OnTriggerEnter2D(Collider2D other) => TryPunish(other.gameObject);
    void OnTriggerStay2D(Collider2D other) => TryPunish(other.gameObject);
    void OnCollisionEnter2D(Collision2D other) => TryPunish(other.gameObject);
    void OnCollisionStay2D(Collision2D other) => TryPunish(other.gameObject);

    void TryPunish(GameObject other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastDamageTime + damageInterval) return;
        if (respawnPoint != null)
               other.transform.position = respawnPoint.position;

            var pc = other.GetComponent<PlayerController2D>();
        if (pc != null) pc.TakeDamage(damage);

        lastDamageTime = Time.time;
    }
}