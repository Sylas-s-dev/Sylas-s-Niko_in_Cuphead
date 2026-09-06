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
    [Header("Tir P3 - Prefab propre")] public GameObject p3BulletPrefab;
    [Header("Tir P3 - Equilibrage")] public float p3BulletSpeed = 2.5f; public float p3FireRate = 1.4f;
    [Header("Laser")] public GameObject warningFlash; public GameObject laserBeam; public float warningTime = 0.8f; public float laserDuration = 1.5f; public float laserFollowDuration = 1.2f; public Transform player;
    [Header("Boulders P1/P2")] public GameObject boulderPrefab; public Transform boulderSpawnPoint; public float delayBetweenBouldersMin = 0.35f; public float delayBetweenBouldersMax = 0.75f;
    [Header("Phase 3 - Transition")] public Transform centerPosition; public float moveToCenterSpeed = 3f; public Collider2D bossContactCollider; public float phase3Pause = 4f; public Animator animator;
    [Header("Mur P3")] public float wallBulletScale = 2.5f; public float wallSpeed = 1.2f; public float wallLifetime = 15f;
    private bool specialIsActive = false; private bool isInvulnerable = false; private bool phase3Paused = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) { healthBar.maxValue = maxHealth; healthBar.value = currentHealth; }
        StartCoroutine(BulletLoop());
        StartCoroutine(SpecialLoopP1P2());
    }

    void CleanWallBullet(GameObject b)
    {
        foreach (var comp in b.GetComponentsInChildren<MonoBehaviour>(true))
        {
            string n = comp.GetType().Name.ToLower();
            if (n.Contains("zigzag")) Destroy(comp);
        }
    }

    IEnumerator BulletLoop()
    {
        while (true)
        {
            if (phase3Paused) { yield return null; continue; }

            if (!secondBarActive)
            {
                if (!specialIsActive)
                {
                    if (firePoints.Length > 0) Instantiate(bossBulletPrefab, firePoints[currentPoint].position, Quaternion.identity);
                    currentPoint = (currentPoint + 1) % Mathf.Max(1, firePoints.Length);
                    yield return new WaitForSeconds(fireRate);
                }
                else yield return null;
            }
            else
            {
                bool isLeftSpawn = Random.value > 0.5f;
                Transform[] pool = isLeftSpawn ? bulletSpawnsLeft : bulletSpawnsRight;

                if (pool != null && pool.Length > 0)
                {
                    Transform spawn = pool[Random.Range(0, pool.Length)];
                    if (spawn != null)
                    {
                        // On utilise la prefab propre si elle existe
                        GameObject prefabToUse = p3BulletPrefab != null ? p3BulletPrefab : bossBulletPrefab;
                        GameObject b = Instantiate(prefabToUse, spawn.position, Quaternion.identity);

                        // Si c'est l'ancienne prefab qui a encore le script Bullet, on le nettoie
                        if (prefabToUse == bossBulletPrefab)
                        {
                            foreach (var c in b.GetComponentsInChildren<MonoBehaviour>(true))
                            {
                                string n = c.GetType().Name.ToLower();
                                if (n.Contains("zigzag")) continue;
                                if (n.Contains("boulder") || n.Contains("bullet")) Destroy(c);
                            }
                            foreach (var col in b.GetComponentsInChildren<Collider2D>(true)) { col.enabled = true; col.isTrigger = true; }
                            if (b.GetComponent<SimpleDamage>() == null) b.AddComponent<SimpleDamage>();
                        }

                        Vector2 dir = isLeftSpawn ? Vector2.right : Vector2.left;
                        StartCoroutine(ForceVelocityPermanent(b, dir * Mathf.Abs(p3BulletSpeed)));
                        Destroy(b, 7f);
                    }
                }
                yield return new WaitForSeconds(p3FireRate + Random.Range(0.1f, 0.4f));
            }
        }
    }

    IEnumerator SpecialLoopP1P2()
    {
        while (!secondBarActive)
        {
            if (phase3Paused) { yield return null; continue; }

            // P1 = pas de capacités spéciales
            if (phase == 1)
            {
                yield return null;
                continue;
            }

            // P2 seulement
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            if (secondBarActive) break;
            if (specialIsActive) continue;
            int r = Random.Range(0, 3);
            if (r == 0) yield return StartCoroutine(HorizontalLaserAttack());
            else if (r == 1) yield return StartCoroutine(BouncingBoulderAttack(3));
            else yield return StartCoroutine(SideBoulderAttack());
        }
        if (secondBarActive) StartCoroutine(SpecialLoopP3());
    }

    IEnumerator SpecialLoopP3()
    {
        while (secondBarActive)
        {
            if (phase3Paused) { yield return null; continue; }
            yield return new WaitForSeconds(Random.Range(4f, 6f));
            int r = Random.Range(0, 3);
            if (r == 0) yield return StartCoroutine(SideBulletBurstAttack());
            else if (r == 1) yield return StartCoroutine(LaserFollowPlayerAttack());
            else yield return StartCoroutine(SideBoulderAttack());
        }
    }

    IEnumerator HorizontalLaserAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.1f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.1f); } yield return new WaitForSeconds(warningTime); if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }
    IEnumerator LaserFollowPlayerAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); float t = 0; while (t < laserFollowDuration) { if (player != null && warningFlash != null) warningFlash.transform.position = player.position; t += Time.deltaTime; yield return null; } for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.08f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.08f); } if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null && warningFlash != null) laserBeam.transform.position = warningFlash.transform.position; if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }
    IEnumerator BouncingBoulderAttack(int c) { specialIsActive = true; for (int i = 0; i < c; i++) { Transform spawn = boulderSpawnRight != null ? boulderSpawnRight : boulderSpawnPoint; GameObject b = Instantiate(boulderPrefab, spawn.position, Quaternion.identity); var bScript = b.GetComponent<Boulder>(); if (bScript != null) bScript.Init(-1f, boulderSpeed); else b.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * boulderSpeed; if (i < c - 1) yield return new WaitForSeconds(Random.Range(delayBetweenBouldersMin, delayBetweenBouldersMax)); } yield return new WaitForSeconds(1f); specialIsActive = false; }
    IEnumerator SideBoulderAttack() { specialIsActive = true; bool fromRight = Random.value > 0.5f; Transform spawn = fromRight ? boulderSpawnRight : boulderSpawnLeft; if (spawn != null) { GameObject b = Instantiate(boulderPrefab, spawn.position, Quaternion.identity); float direction = fromRight ? -1f : 1f; var bScript = b.GetComponent<Boulder>(); if (bScript != null) bScript.Init(direction, boulderSpeed); else b.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * boulderSpeed, 0f); } yield return new WaitForSeconds(1.2f); specialIsActive = false; }

    IEnumerator SideBulletBurstAttack()
    {
        specialIsActive = true;
        bool fromRight = Random.value > 0.5f;
        Transform[] wallSpawns = fromRight ? bulletSpawnsRight : bulletSpawnsLeft;
        Vector2 center = centerPosition != null ? (Vector2)centerPosition.position : Vector2.zero;
        foreach (var spawn in wallSpawns)
        {
            if (spawn == null) continue;
            var b = Instantiate(bossBulletPrefab, spawn.position, Quaternion.identity);
            CleanWallBullet(b);
            b.transform.localScale *= wallBulletScale;
            foreach (var bulletScript in b.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (bulletScript.GetType().Name.ToLower().Contains("bullet"))
                {
                    var field = bulletScript.GetType().GetField("isWallBullet");
                    if (field != null) field.SetValue(bulletScript, true);
                    var field2 = bulletScript.GetType().GetField("pierce");
                    if (field2 != null) field2.SetValue(bulletScript, true);
                }
            }
            Vector2 dir = (spawn.position.x > center.x) ? Vector2.left : Vector2.right;
            StartCoroutine(ForceVelocityPermanent(b, dir * wallSpeed));
            Destroy(b, wallLifetime);
        }
        yield return new WaitForSeconds(1.5f);
        specialIsActive = false;
    }

    IEnumerator ForceVelocityPermanent(GameObject b, Vector2 vel)
    {
        while (b != null)
        {
            var rb = b.GetComponentInChildren<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.bodyType = RigidbodyType2D.Dynamic;
                // On force le X, on laisse le Y au zigzag
                rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void TakeDamage(int dmg) { if (isInvulnerable) return; if (!secondBarActive) { currentHealth -= dmg; if (healthBar != null) healthBar.value = currentHealth; if (currentHealth <= maxHealth / 2 && phase == 1) { phase = 2; fireRate = 0.4f; GetComponent<SpriteRenderer>().color = Color.red; } if (currentHealth <= 0) StartCoroutine(EnterPhase3()); } else { currentHealthPhase3 -= dmg; if (healthBarPhase3 != null) healthBarPhase3.value = currentHealthPhase3; if (currentHealthPhase3 <= 0) Die(); } }
    IEnumerator EnterPhase3() { secondBarActive = true; isInvulnerable = true; phase3Paused = true; specialIsActive = true; phase = 3; Vector3 targetPos = centerPosition != null ? centerPosition.position : transform.position; if (healthBar != null) healthBar.gameObject.SetActive(false); if (healthBarPhase3_GO != null) healthBarPhase3_GO.SetActive(true); currentHealthPhase3 = maxHealthPhase3; if (healthBarPhase3 != null) { healthBarPhase3.maxValue = maxHealthPhase3; healthBarPhase3.value = currentHealthPhase3; } Rigidbody2D rbBoss = GetComponent<Rigidbody2D>(); if (rbBoss != null) rbBoss.linearVelocity = Vector2.zero; while (Vector2.Distance(transform.position, targetPos) > 0.1f) { transform.position = Vector2.MoveTowards(transform.position, targetPos, moveToCenterSpeed * Time.deltaTime); yield return null; } transform.position = targetPos; if (bossContactCollider != null) bossContactCollider.enabled = false; if (animator != null) animator.SetTrigger("Enrage"); yield return new WaitForSeconds(phase3Pause); isInvulnerable = false; phase3Paused = false; specialIsActive = false; }
    void Die() { StopAllCoroutines(); Destroy(gameObject); }
}