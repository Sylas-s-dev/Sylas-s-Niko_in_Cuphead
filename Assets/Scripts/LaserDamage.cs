using UnityEngine;

public class LaserDamage : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 0.5f;
    float lastHit;
    BoxCollider2D col;

    void Awake() { col = GetComponent<BoxCollider2D>(); }

    void Update()
    {
        if (Time.time < lastHit + cooldown) return;

        // Check une boite autour du laser
        var hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var p = h.GetComponent<PlayerController2D>();
                if (p != null)
                {
                    Debug.Log("DEGATS APPLIQUÉS - Overlap");
                    p.TakeDamage(damage);
                    lastHit = Time.time;
                    break;
                }
            }
        }
    }

    void OnDrawGizmos() // pour voir la vraie zone de dégâts en rouge dans la Scene
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}