using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Boss : MonoBehaviour
{
    [Header("Vie Phase 1 & 2")] public int maxHealth = 20; private int currentHealth; private int phase = 1;
    [Header("Vie Phase 3")] public int maxHealthPhase3 = 30; private int currentHealthPhase3; public bool secondBarActive = false;
    [Header("UI")] public Slider healthBar; public Slider healthBarPhase3; public GameObject healthBarPhase3_GO;
    [Header("Tir P1/P2")] public GameObject bossBulletPrefab; public Transform[] firePoints; public float fireRate = 0.6f; private int currentPoint = 0;
    [Header("Tir P2 - Equilibrage Continu")] public float p2Lifetime = 15f;
    [Header("Tir P3")] public Transform[] bulletSpawnsLeft; public Transform[] bulletSpawnsRight; public Transform boulderSpawnLeft; public Transform boulderSpawnRight; public float sideBulletSpeed = 4f; public float sideFireRate = 0.9f; public float boulderSpeed = 5f;
    [Header("Tir P3 - Prefab propre")] public GameObject p3BulletPrefab;
    [Header("Tir P2 - Equilibrage Continu")] public float p2BulletSpeed = 2f; public float p2FireRate = 1.0f;
    [Header("Tir P3 - Equilibrage")] public float p3BulletSpeed = 2.5f; public float p3FireRate = 1.4f; public float p3Lifetime = 10f;
    [Header("Laser")] public GameObject warningFlash; public GameObject laserBeam; public float warningTime = 0.8f; public float laserDuration = 1.5f; public float laserFollowDuration = 1.2f; public Transform player;
    [Header("Boulders P1/P2")] public GameObject boulderPrefab; public Transform boulderSpawnPoint; public float delayBetweenBouldersMin = 0.35f; public float delayBetweenBouldersMax = 0.75f;
    [Header("Phase 3 - Transition")] public Transform centerPosition; public float moveToCenterSpeed = 3f; public Collider2D bossContactCollider; public float phase3Pause = 4f; public Animator animator;
    [Header("Mur P3")] public float wallBulletScale = 2.5f; public float wallSpeed = 1.2f; public float wallLifetime = 15f;
    private bool specialIsActive = false; private bool isInvulnerable = false; private bool phase3Paused = false;
    [Header("P3 - Arena Aérienne")] public GameObject groundToCollapse; public GameObject platformPurplePrefab; public GameObject[] aerialPlatforms; public GameObject warningGroundPrefab; public Transform[] warningSpawnPoints; public float warningDuration = 2.5f; public float groundCollapseSpeed = 5f;
    [Header("P3 - Homing Orb")] public GameObject homingOrbPrefab; public Transform homingOrbSpawnPoint; public float homingOrbSpeed = 2.2f; public float homingOrbLifetime = 5f;
    [Header("P3 - Freeze Pattern")]
    public GameObject freezeTimerUI;
    public TextMeshProUGUI freezeTimerText;
    public float freezeCountdown = 5f;
    public float mustStopDuration = 0.2f;
    public float slowDuration = 4f;
    [Range(0.1f, 1f)] public float slowFactor = 0.4f;
    public float moveThreshold = 0.2f;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) { healthBar.maxValue = maxHealth; healthBar.value = currentHealth; }
        if (freezeTimerUI != null) freezeTimerUI.SetActive(false);
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
                if (phase == 1)
                {
                    if (firePoints.Length > 0)
                    {
                        int randomIndex;
                        do { randomIndex = Random.Range(0, firePoints.Length); }
                        while (firePoints.Length > 1 && randomIndex == currentPoint);
                        currentPoint = randomIndex;
                        Instantiate(bossBulletPrefab, firePoints[currentPoint].position, Quaternion.identity);
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
                            GameObject prefabToUse = p3BulletPrefab != null ? p3BulletPrefab : bossBulletPrefab;
                            GameObject b = Instantiate(prefabToUse, spawn.position, Quaternion.identity);
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
                            StartCoroutine(ForceVelocityPermanent(b, dir * Mathf.Abs(p2BulletSpeed)));
                            Destroy(b, 15f);
                        }
                    }
                    yield return new WaitForSeconds(p2FireRate + Random.Range(0.1f, 0.3f));
                }
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
                        GameObject prefabToUse = p3BulletPrefab != null ? p3BulletPrefab : bossBulletPrefab;
                        GameObject b = Instantiate(prefabToUse, spawn.position, Quaternion.identity);
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
                        Destroy(b, 15f);
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
            if (phase == 1)
            {
                yield return new WaitForSeconds(Random.Range(5f, 7f));
                if (secondBarActive) break;
                if (specialIsActive) continue;
                yield return StartCoroutine(BouncingBoulderAttack(2));
                continue;
            }
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            if (secondBarActive) break;
            if (specialIsActive) continue;
            int r = Random.Range(0, 3);
            if (r == 0) yield return StartCoroutine(HorizontalLaserAttack());
            else if (r == 1) yield return StartCoroutine(BouncingBoulderAttack(3));
            else yield return StartCoroutine(SideBoulderAttack());
        }
    }

    IEnumerator SpecialLoopP3()
    {
        while (secondBarActive)
        {
            if (phase3Paused) { yield return null; continue; }
            yield return new WaitForSeconds(Random.Range(4f, 6f));
            int r = Random.Range(0, 4);
            if (r == 0) yield return StartCoroutine(SideBulletBurstAttack());
            else if (r == 1) yield return StartCoroutine(LaserFollowPlayerAttack());
            else if (r == 2) yield return StartCoroutine(HomingOrbAttack());
            else yield return StartCoroutine(FreezeStatueAttack());
        }
    }

    IEnumerator FreezeStatueAttack()
    {
        specialIsActive = true;
        if (freezeTimerUI != null) freezeTimerUI.SetActive(true);
        float t = freezeCountdown;
        while (t > 0)
        {
            if (freezeTimerText != null) freezeTimerText.text = Mathf.Ceil(t).ToString();
            if (freezeTimerText != null && t < 2f) freezeTimerText.color = Color.red;
            t -= Time.deltaTime;
            yield return null;
        }
        if (freezeTimerText != null)
        {
            freezeTimerText.text = "!";
            freezeTimerText.fontSize = 150f;
        }

        float stopCheck = 0;
        bool hasMoved = false;
        Rigidbody2D playerRb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        var playerCtrl = player != null ? player.GetComponent<PlayerController2D>() : null;

        while (stopCheck < mustStopDuration)
        {
            float vel = 0;
            if (playerRb != null) vel = playerRb.linearVelocity.magnitude;
            else if (player != null) vel = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
            if (vel > moveThreshold) hasMoved = true;
            stopCheck += Time.deltaTime;
            yield return null;
        }

        if (freezeTimerUI != null) freezeTimerUI.SetActive(false);
        if (freezeTimerText != null) freezeTimerText.color = Color.white;

        if (hasMoved && playerCtrl != null)
        {
            playerCtrl.ApplySlow(slowFactor, slowDuration);
        }
        else
        {
            if (player != null)
            {
                var sr = player.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) StartCoroutine(FlashColor(sr, Color.green, 0.1f));
            }
        }
        specialIsActive = false;
    }

    IEnumerator FlashColor(SpriteRenderer sr, Color c, float dur)
    {
        Color baseC = sr.color;
        sr.color = c;
        yield return new WaitForSeconds(dur);
        sr.color = baseC;
    }

    IEnumerator HomingOrbAttack()
    {
        specialIsActive = true;
        if (warningFlash != null && homingOrbSpawnPoint != null)
        {
            warningFlash.transform.position = homingOrbSpawnPoint.position;
            warningFlash.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            warningFlash.SetActive(false);
        }
        if (homingOrbPrefab != null && homingOrbSpawnPoint != null)
        {
            GameObject orb = Instantiate(homingOrbPrefab, homingOrbSpawnPoint.position, Quaternion.identity);
            var script = orb.GetComponent<HomingOrb>();
            if (script != null) { script.speed = homingOrbSpeed; script.lifetime = homingOrbLifetime; }
        }
        yield return new WaitForSeconds(1f);
        specialIsActive = false;
    }

    IEnumerator HorizontalLaserAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.1f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.1f); } yield return new WaitForSeconds(warningTime); if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }
    IEnumerator LaserFollowPlayerAttack() { specialIsActive = true; if (warningFlash != null) warningFlash.SetActive(true); float t = 0; while (t < laserFollowDuration) { if (player != null && warningFlash != null) warningFlash.transform.position = player.position; t += Time.deltaTime; yield return null; } for (int i = 0; i < 4; i++) { if (warningFlash != null) warningFlash.SetActive(false); yield return new WaitForSeconds(0.08f); if (warningFlash != null) warningFlash.SetActive(true); yield return new WaitForSeconds(0.08f); } if (warningFlash != null) warningFlash.SetActive(false); if (laserBeam != null && warningFlash != null) laserBeam.transform.position = warningFlash.transform.position; if (laserBeam != null) laserBeam.SetActive(true); yield return new WaitForSeconds(laserDuration); if (laserBeam != null) laserBeam.SetActive(false); specialIsActive = false; }
    IEnumerator BouncingBoulderAttack(int c) { if (secondBarActive) yield break; specialIsActive = true; for (int i = 0; i < c; i++) { Transform spawn = boulderSpawnRight != null ? boulderSpawnRight : boulderSpawnPoint; GameObject b = Instantiate(boulderPrefab, spawn.position, Quaternion.identity); var bScript = b.GetComponent<Boulder>(); if (bScript != null) bScript.Init(-1f, boulderSpeed); else b.GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * boulderSpeed; if (i < c - 1) yield return new WaitForSeconds(Random.Range(delayBetweenBouldersMin, delayBetweenBouldersMax)); } yield return new WaitForSeconds(1f); specialIsActive = false; }
    IEnumerator SideBoulderAttack() { if (secondBarActive) yield break; specialIsActive = true; bool fromRight = Random.value > 0.5f; Transform spawn = fromRight ? boulderSpawnRight : boulderSpawnLeft; if (spawn != null) { GameObject b = Instantiate(boulderPrefab, spawn.position, Quaternion.identity); float direction = fromRight ? -1f : 1f; var bScript = b.GetComponent<Boulder>(); if (bScript != null) bScript.Init(direction, boulderSpeed); else b.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * boulderSpeed, 0f); } yield return new WaitForSeconds(1.2f); specialIsActive = false; }
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
            if (rb != null) { rb.gravityScale = 0; rb.bodyType = RigidbodyType2D.Dynamic; rb.linearVelocity = new Vector2(vel.x, rb.linearVelocity.y); }
            yield return new WaitForFixedUpdate();
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isInvulnerable) return;

        if (!secondBarActive)
        {
            currentHealth -= dmg;
            if (healthBar != null) healthBar.value = currentHealth;

            if (currentHealth <= maxHealth / 2 && phase == 1)
            {
                phase = 2;
                fireRate = 0.4f;
                GetComponent<SpriteRenderer>().color = Color.red;
            }

            if (currentHealth <= 0)
            {
                StopAllCoroutines();
                StartCoroutine(EnterPhase3());
            }
        }
        else
        {
            currentHealthPhase3 -= dmg;
            if (healthBarPhase3 != null) healthBarPhase3.value = currentHealthPhase3;
            if (currentHealthPhase3 <= 0) Die();
        }
    }

    IEnumerator EnterPhase3()
    {
        secondBarActive = true; isInvulnerable = true; phase3Paused = true; specialIsActive = true; phase = 3;
        foreach (var d in FindObjectsOfType<SimpleDamage>()) Destroy(d.gameObject);
        foreach (var b in FindObjectsOfType<Boulder>()) Destroy(b.gameObject);
        foreach (var o in FindObjectsOfType<HomingOrb>()) Destroy(o.gameObject);
        var srBoss = GetComponent<SpriteRenderer>();
        if (srBoss != null)
        {
            Color baseColor = srBoss.color; srBoss.color = Color.white; Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.15f); Time.timeScale = 1f; srBoss.color = baseColor;
        }
        else yield return new WaitForSeconds(0.15f);
        if (platformPurplePrefab != null && centerPosition != null)
        {
            Vector3 pos = centerPosition.position; pos.y -= 1.2f;
            Instantiate(platformPurplePrefab, pos, Quaternion.identity);
        }
        GameObject[] warnings = new GameObject[0];
        if (warningGroundPrefab != null && warningSpawnPoints.Length > 0)
        {
            warnings = new GameObject[warningSpawnPoints.Length];
            for (int i = 0; i < warningSpawnPoints.Length; i++)
                warnings[i] = Instantiate(warningGroundPrefab, warningSpawnPoints[i].position + Vector3.up * 0.8f, Quaternion.identity);
            float t = 0;
            while (t < warningDuration)
            {
                foreach (var w in warnings) if (w != null) w.SetActive(!w.activeSelf);
                t += 0.15f; yield return new WaitForSeconds(0.15f);
            }
            foreach (var w in warnings) if (w != null) Destroy(w);
        }
        if (groundToCollapse != null)
        {
            var col = groundToCollapse.GetComponent<Collider2D>(); if (col) col.enabled = false;
            float fall = 0;
            while (fall < 2f)
            {
                groundToCollapse.transform.position += Vector3.down * groundCollapseSpeed * Time.deltaTime;
                fall += Time.deltaTime; yield return null;
            }
            groundToCollapse.SetActive(false);
        }
        foreach (var plat in aerialPlatforms) if (plat != null) plat.SetActive(true);
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (healthBarPhase3_GO != null) healthBarPhase3_GO.SetActive(true);
        currentHealthPhase3 = maxHealthPhase3;
        if (healthBarPhase3 != null) { healthBarPhase3.maxValue = maxHealthPhase3; healthBarPhase3.value = currentHealthPhase3; }
        Vector3 targetPos = centerPosition != null ? centerPosition.position : transform.position;
        Rigidbody2D rbBoss = GetComponent<Rigidbody2D>();
        if (rbBoss != null) rbBoss.linearVelocity = Vector2.zero;
        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveToCenterSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        if (bossContactCollider != null) bossContactCollider.enabled = false;
        if (animator != null) animator.SetTrigger("Enrage");
        yield return new WaitForSeconds(phase3Pause);
        isInvulnerable = false;
        phase3Paused = false;
        specialIsActive = false;
        StartCoroutine(BulletLoop());
        StartCoroutine(SpecialLoopP3());
    }

    void Die() { StopAllCoroutines(); Destroy(gameObject); }
}