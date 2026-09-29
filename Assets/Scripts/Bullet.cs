using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Base")]
    public int baseDamage = 1;
    public int damage = 1;
    public float speed = 15f;
    public float lifeTime = 3f;

    [Header("Médaillon - Homing")]
    public bool hasHoming = false;
    public float homingStrength = 0f;
    public string bossTag = "Boss";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Destroy(gameObject, lifeTime);
    }

    // Appelé par PlayerController2D
    public void Init(float dmgMult, float projSpeed, bool homing, float homingStr, bool isRed, RuntimeAnimatorController bulletAnim, Color bulletColor)
    {
        damage = Mathf.RoundToInt(baseDamage * dmgMult);
        speed = projSpeed;
        hasHoming = homing;
        homingStrength = homingStr;

        // --- ANIMATION SELON L'ARME ---
        if (animator != null && bulletAnim != null)
        {
            animator.runtimeAnimatorController = bulletAnim;
        }

        if (spriteRenderer != null)
        {
            if (isRed) spriteRenderer.color = Color.red;
            else spriteRenderer.color = bulletColor;
        }

        if (rb != null)
            rb.linearVelocity = transform.right * speed;
    }

    // Ancienne version pour compatibilité si tu l'appelles ailleurs
    public void Init(float dmgMult, float projSpeed, bool homing, float homingStr, bool isRed)
    {
        Init(dmgMult, projSpeed, homing, homingStr, isRed, null, Color.white);
    }

    void Update()
    {
        if (hasHoming)
        {
            Transform target = FindClosestBoss();
            if (target != null && rb != null)
            {
                Vector2 dir = (target.position - transform.position).normalized;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, dir * speed, homingStrength * Time.deltaTime);
                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    Transform FindClosestBoss()
    {
        GameObject[] bosses = GameObject.FindGameObjectsWithTag(bossTag);
        if (bosses.Length == 0) return null;
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var b in bosses)
        {
            float d = Vector2.Distance(transform.position, b.transform.position);
            if (d < minDist) { minDist = d; closest = b.transform; }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(bossTag) || other.GetComponentInParent<Boss>() != null)
        {
            var boss = other.GetComponentInParent<Boss>();
            if (boss != null) boss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}