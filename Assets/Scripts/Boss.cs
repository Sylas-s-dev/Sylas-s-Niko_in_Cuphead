using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Vie")]
    public int maxHealth = 20;
    private int currentHealth;
    private int phase = 1;

    [Header("UI")] public Slider healthBar;

    [Header("Tir")]
    public GameObject bossBulletPrefab;
    public Transform[] firePoints;
    public float fireRate = 0.6f;
    private int currentPoint = 0;

    [Header("Laser")]
    public GameObject warningFlash;
    public GameObject laserBeam;
    public float warningTime = 0.8f;
    public float laserDuration = 1.5f;

    [Header("Boulders")]
    public GameObject boulderPrefab;
    public Transform boulderSpawnPoint;
    public float delayBetweenBouldersMin = 0.35f; // délai aléatoire
    public float delayBetweenBouldersMax = 0.75f;

    private bool specialIsActive = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) { healthBar.maxValue = maxHealth; healthBar.value = currentHealth; }

        StartCoroutine(BulletLoop());
        StartCoroutine(SpecialLoop());
    }

    IEnumerator BulletLoop()
    {
        while (currentHealth > 0)
        {
            if (!specialIsActive || phase == 2)
            {
                if (bossBulletPrefab != null && firePoints.Length > 0)
                {
                    Instantiate(bossBulletPrefab, firePoints[currentPoint].position, Quaternion.identity);
                    currentPoint = (currentPoint + 1) % firePoints.Length;
                }
            }
            yield return new WaitForSeconds(fireRate);
        }
    }

    IEnumerator SpecialLoop()
    {
        yield return new WaitForSeconds(3f);
        while (currentHealth > 0)
        {
            if (phase == 2 && !specialIsActive)
            {
                int rand = Random.Range(0, 3);
                if (rand == 0) yield return StartCoroutine(HorizontalLaserAttack());
                else if (rand == 1) yield return StartCoroutine(BouncingBoulderAttack(1));
                else yield return StartCoroutine(BouncingBoulderAttack(2));
            }
            yield return new WaitForSeconds(Random.Range(2f, 3.5f));
        }
    }

    IEnumerator HorizontalLaserAttack()
    {
        specialIsActive = true;
        warningFlash.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            warningFlash.SetActive(false); yield return new WaitForSeconds(0.1f);
            warningFlash.SetActive(true); yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(warningTime);
        warningFlash.SetActive(false);
        laserBeam.SetActive(true);
        yield return new WaitForSeconds(laserDuration);
        laserBeam.SetActive(false);
        specialIsActive = false;
    }

    IEnumerator BouncingBoulderAttack(int count)
    {
        specialIsActive = true;
        for (int i = 0; i < count; i++)
        {
            Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity);

            // pas de délai après le dernier
            if (i < count - 1)
            {
                float randomDelay = Random.Range(delayBetweenBouldersMin, delayBetweenBouldersMax);
                yield return new WaitForSeconds(randomDelay);
            }
        }
        yield return new WaitForSeconds(1f);
        specialIsActive = false;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (healthBar != null) healthBar.value = currentHealth;
        if (currentHealth <= maxHealth / 2 && phase == 1)
        {
            phase = 2;
            fireRate = 0.8f;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        StopAllCoroutines();
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}