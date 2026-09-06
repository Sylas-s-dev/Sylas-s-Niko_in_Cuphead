using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Vie Phase 1 & 2")] public int maxHealth = 20; private int currentHealth; private int phase = 1;
    [Header("Vie Phase 3")] public int maxHealthPhase3 = 30; private int currentHealthPhase3; public bool secondBarActive = false;
    [Header("UI")] public Slider healthBar; public Slider healthBarPhase3; public GameObject healthBarPhase3_GO;
    [Header("Tir P1/P2")] public GameObject bossBulletPrefab; public Transform[] firePoints; public float fireRate = 0.6f; private int currentPoint = 0;
    [Header("Tir P3")] public Transform[] bulletSpawnsLeft; public Transform[] bulletSpawnsRight; public Transform boulderSpawnLeft; public Transform boulderSpawnRight; public float sideBulletSpeed = 4f; public float sideFireRate = 0.9f; public float boulderSpeed = 5f;
    [Header("Laser")] public GameObject warningFlash; public GameObject laserBeam; public float warningTime = 0.8f; public float laserDuration = 1.5f; public float laserFollowDuration = 1.2f; public Transform player;
    [Header("Boulders P1/P2")] public GameObject boulderPrefab; public Transform boulderSpawnPoint; public float delayBetweenBouldersMin = 0.35f; public float delayBetweenBouldersMax = 0.75f;
    [Header("Phase 3 - Transition")] public Transform centerPosition; public float moveToCenterSpeed = 3f; public Collider2D bossContactCollider; public float phase3Pause = 4f; public Animator animator;
    private bool specialIsActive = false; private bool isInvulnerable = false; private bool phase3Paused = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) { healthBar.maxValue = maxHealth; healthBar.value = currentHealth; }
        if (healthBarPhase3_GO != null) healthBarPhase3_GO.SetActive(false);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(BulletLoop());
        StartCoroutine(SpecialLoop());
    }

    // Fonction qui nettoie une balle de tous ses scripts de mouvement sans connaitre leur nom
    void CleanBullet(GameObject b)
    {
        foreach (var comp in b.GetComponents<MonoBehaviour>())
        {
            string n = comp.GetType().Name.ToLower();
            if (n.Contains("bullet") || n.Contains("boulder") || n.Contains("zigzag")) Destroy(comp);
        }
    }

    IEnumerator BulletLoop()
    {
        while (true)
        {
            if (phase3Paused) { yield return null; continue; }
            if (!specialIsActive && !secondBarActive)
            {
                if (firePoints.Length > 0) Instantiate(bossBulletPrefab, firePoints[currentPoint].position, Quaternion.identity);
                currentPoint = (currentPoint + 1) % Mathf.Max(1, firePoints.Length);
                yield return new WaitForSeconds(fireRate);
            }
            else if (secondBarActive && !specialIsActive)
            {
                bool fromRight = Random.value > 0.5f;
                Transform[] pool = fromRight ? bulletSpawnsRight : bulletSpawnsLeft;
                if (pool != null && pool.Length > 0)
                {
                    Transform spawn = pool[Random.Range(0, pool.Length)];
                    if (spawn != null)
                    {
                        GameObject b = Instantiate(bossBulletPrefab, spawn.position, Quaternion.identity);
                        CleanBullet(b);
                        var rb = b.GetComponent<Rigidbody2D>();
                        if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Dynamic; rb.linearVelocity = (fromRight ? Vector2.left : Vector2.right) * Mathf.Abs(sideBulletSpeed); }
                        Destroy(b, 6f);
                    }
                }
                yield return new WaitForSeconds(sideFireRate);
            }
            else yield return null;
        }
    }

    IEnumerator SpecialLoop()
    {
        yield return new WaitForSeconds(3f);
        while (true)
        {
            if (phase3Paused) { yield return null; continue; }
            if (!specialIsActive)
            {
                if (phase == 2 && !secondBarActive)
                {
                    int rand = Random.Range(0, 3);
                    if (rand == 0) yield return StartCoroutine(HorizontalLaserAttack());
                    else yield return StartCoroutine(BouncingBoulderAttack(Random.Range(1, 3)));
                }
                else if (secondBarActive)
                {
                    int rand = Random.Range(0, 3);
                    if (rand == 0) yield return StartCoroutine(LaserFollowPlayerAttack());
                    else if (rand == 1) yield return StartCoroutine(SideBoulderAttack());
                    else yield return StartCoroutine(SideBulletBurstAttack());
                }
            }
            yield return new WaitForSeconds(Random.Range(2f, 3.5f));
        }
    }

    IEnumerator HorizontalLaserAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.1f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.1f); } yield return new WaitForSeconds(warningTime); if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }
    IEnumerator BouncingBoulderAttack(int c) { specialIsActive = true; for (int i = 0; i < c; i++) { Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity); if (i < c - 1) yield return new WaitForSeconds(Random.Range(delayBetweenBouldersMin, delayBetweenBouldersMax)); } yield return new WaitForSeconds(1f); specialIsActive = false; }
    IEnumerator LaserFollowPlayerAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); float t = 0; while (t < laserFollowDuration) { if (player != null && warningFlash != null) warningFlash.transform.position = player.position; t += Time.deltaTime; yield return null; } for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.08f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.08f); } if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null && warningFlash != null) laserBeam.transform.position = warningFlash.transform.position; if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }

    IEnumerator SideBoulderAttack()
    {
        specialIsActive = true;
        bool fromRight = Random.value > 0.5f;
        Transform spawn = fromRight ? boulderSpawnRight : boulderSpawnLeft;
        if (spawn != null)
        {
            GameObject boulder = Instantiate(boulderPrefab, spawn.position, Quaternion.identity);
            foreach (var comp in boulder.GetComponents<MonoBehaviour>()) { if (comp.GetType().Name.ToLower().Contains("boulder")) Destroy(comp); }
            Rigidbody2D rb = boulder.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Dynamic; rb.linearVelocity = (fromRight ? Vector2.left : Vector2.right) * Mathf.Abs(boulderSpeed); rb.angularVelocity = fromRight ? -360f : 360f; }
        }
        yield return new WaitForSeconds(1.2f);
        specialIsActive = false;
    }

    IEnumerator SideBulletBurstAttack()
    {
        specialIsActive = true;
        int max = Mathf.Max(bulletSpawnsLeft != null ? bulletSpawnsLeft.Length : 0, bulletSpawnsRight != null ? bulletSpawnsRight.Length : 0);
        for (int i = 0; i < max; i++)
        {
            if (bulletSpawnsRight != null && i < bulletSpawnsRight.Length && bulletSpawnsRight[i] != null)
            {
                var b = Instantiate(bossBulletPrefab, bulletSpawnsRight[i].position, Quaternion.identity);
                CleanBullet(b);
                var rb = b.GetComponent<Rigidbody2D>(); if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Dynamic; rb.linearVelocity = Vector2.left * Mathf.Abs(sideBulletSpeed); }
                Destroy(b, 6f);
            }
            if (bulletSpawnsLeft != null && i < bulletSpawnsLeft.Length && bulletSpawnsLeft[i] != null)
            {
                var b = Instantiate(bossBulletPrefab, bulletSpawnsLeft[i].position, Quaternion.identity);
                CleanBullet(b);
                var rb = b.GetComponent<Rigidbody2D>(); if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Dynamic; rb.linearVelocity = Vector2.right * Mathf.Abs(sideBulletSpeed); }
                Destroy(b, 6f);
            }
            yield return new WaitForSeconds(0.2f);
        }
        specialIsActive = false;
    }

    public void TakeDamage(int dmg)
    {
        if (isInvulnerable) return;
        if (!secondBarActive) { currentHealth -= dmg; if (healthBar != null) healthBar.value = currentHealth; if (currentHealth <= maxHealth / 2 && phase == 1) { phase = 2; fireRate = 0.8f; GetComponent<SpriteRenderer>().color = Color.red; } if (currentHealth <= 0) StartCoroutine(EnterPhase3()); }
        else { currentHealthPhase3 -= dmg; if (healthBarPhase3 != null) healthBarPhase3.value = currentHealthPhase3; if (currentHealthPhase3 <= 0) Die(); }
    }
    IEnumerator EnterPhase3() { secondBarActive = true; isInvulnerable = true; phase3Paused = true; specialIsActive = true; phase = 3; Vector3 targetPos = centerPosition != null ? centerPosition.position : transform.position; if (healthBar != null) healthBar.gameObject.SetActive(false); if (healthBarPhase3_GO != null) healthBarPhase3_GO.SetActive(true); currentHealthPhase3 = maxHealthPhase3; if (healthBarPhase3 != null) { healthBarPhase3.maxValue = maxHealthPhase3; healthBarPhase3.value = currentHealthPhase3; } Rigidbody2D rbBoss = GetComponent<Rigidbody2D>(); if (rbBoss != null) rbBoss.linearVelocity = Vector2.zero; while (Vector2.Distance(transform.position, targetPos) > 0.1f) { transform.position = Vector2.MoveTowards(transform.position, targetPos, moveToCenterSpeed * Time.deltaTime); yield return null; } transform.position = targetPos; if (bossContactCollider != null) bossContactCollider.enabled = false; if (animator != null) animator.SetTrigger("Enrage"); yield return new WaitForSeconds(phase3Pause); isInvulnerable = false; phase3Paused = false; specialIsActive = false; }
    void Die() { StopAllCoroutines(); Destroy(gameObject); }
}