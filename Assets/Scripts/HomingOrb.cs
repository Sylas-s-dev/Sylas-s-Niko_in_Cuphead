using UnityEngine;

public class HomingOrb : MonoBehaviour
{
    public float speed = 2.2f;
    public float turnSpeed = 2.5f;
    public float lifetime = 5f;
    public float explosionRadius = 2.2f;
    public GameObject explosionPrefab;

    private Transform target;
    private Rigidbody2D rb;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
        timer = lifetime;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f) { Explode(); return; }

        Vector2 dir = (target.position - transform.position).normalized;
        Vector2 currentDir = rb.linearVelocity.normalized;
        if (currentDir == Vector2.zero) currentDir = dir;

        Vector2 newDir = Vector2.Lerp(currentDir, dir, turnSpeed * Time.fixedDeltaTime).normalized;
        rb.linearVelocity = newDir * speed;
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                // C'est TA fonction qui est dans PlayerController2D
                var pc = h.GetComponent<PlayerController2D>();
                if (pc != null) pc.TakeDamage(1);
            }
        }

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            Explode();
    }
}