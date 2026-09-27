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
    public string bossTag = "Boss"; // ou "Enemy", mets le tag de ton boss

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    // Appelé par PlayerController2D
    public void Init(float dmgMult, float projSpeed, bool homing, float homingStr, bool isRed)
    {
        damage = Mathf.RoundToInt(baseDamage * dmgMult);
        speed = projSpeed;
        hasHoming = homing;
        homingStrength = homingStr;

        if (isRed)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;
        }

        // On lance la balle
        if (rb != null)
            rb.linearVelocity = transform.right * speed;
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
                // Oriente la balle vers la cible
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
            if (d < minDist)
            {
                minDist = d;
                closest = b.transform;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si ça touche le boss
        if (other.CompareTag(bossTag) || other.GetComponentInParent<Boss>() != null)
        {
            // On cherche le script Boss sur le parent
            var boss = other.GetComponentInParent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Optionnel: détruit la balle si elle touche le sol
            // Destroy(gameObject);
        }
    }
}