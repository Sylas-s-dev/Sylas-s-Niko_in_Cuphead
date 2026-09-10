using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Invincibilité après hit")]
    public float hitInvincibilityDuration = 1f;
    public float blinkInterval = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Coroutine invincibilityCoroutine;
    private bool isHitInvincible = false;

    [Header("Freeze Debuff - P3")]
    private bool isSlowed = false;
    private float slowMultiplier = 1f;
    private Coroutine slowCoroutine;
    private Color baseColor = Color.white;
    private float originalGravityScale;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;
    public TrailRenderer dashTrail;
    public LayerMask layersToIgnoreDuringDash;
    private bool canDash = true;
    private bool isDashing = false;
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible || isDashing || isHitInvincible;

    [Header("Dash FX")]
    public GameObject dashPuffPrefab;
    public float invisibleTime = 0.15f;

    [Header("Coyote Time")] public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    [Header("Vie")] public int maxHealth = 5; public int currentHealth; public Slider healthBar; public Image healthBarFill;
    [Header("Deplacement")] public float speed = 7f;
    [Header("Saut Variable")] public float jumpForce = 10f; public float jumpTime = 0.20f; public float fallMultiplier = 2.5f; public float lowJumpMultiplier = 2.5f;

    [Header("Tir Multi")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int bulletCount = 1;
    public float spreadAngle = 15f;

    private Rigidbody2D rb;
    private float nextFireTime;
    private bool isGrounded;
    private float jumpTimeCounter;
    private bool isJumping;
    private LayerMask originalExcludeLayers;
    private Camera cam;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        cam = Camera.main;
        originalExcludeLayers = rb.excludeLayers;
        currentHealth = maxHealth;
        slowMultiplier = 1f;
        if (healthBarFill == null && healthBar != null && healthBar.fillRect != null)
            healthBarFill = healthBar.fillRect.GetComponent<Image>();
        if (healthBar != null) { healthBar.maxValue = maxHealth; healthBar.value = currentHealth; }
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;
    }

    void Update()
    {
        if (!isDashing)
        {
            float move = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(move * speed * slowMultiplier, rb.linearVelocity.y);

            if (move != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = move < 0;
            }

            // ANIMATION
            if (anim != null)
            {
                anim.SetFloat("Speed", Mathf.Abs(move));
            }
        }

        if (firePoint != null && cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 rawDir = mouseWorld - firePoint.position;
            float rawAngle = Mathf.Atan2(rawDir.y, rawDir.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(rawAngle / 45f) * 45f;
            firePoint.rotation = Quaternion.Euler(0, 0, snappedAngle);
        }

        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < -0.5f)
            && Input.GetButtonDown("Jump") && isGrounded)
        {
            Collider2D platform = Physics2D.OverlapBox(
                transform.position + Vector3.down * 0.8f,
                new Vector2(1f, 0.2f),
                0f,
                LayerMask.GetMask("Platform"));

            if (platform != null)
            {
                StartCoroutine(DisablePlatformTemporarily(platform));
                isGrounded = false;
                coyoteTimeCounter = 0f;
            }
        }

        if (isGrounded) coyoteTimeCounter = coyoteTime; else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0) jumpTimeCounter -= Time.deltaTime;
            else isJumping = false;
        }
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            if (rb.linearVelocity.y > 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) StartCoroutine(Dash());

        if ((Input.GetKey(KeyCode.X) || Input.GetMouseButton(0)) && Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // --- GRAVITÉ LUNAIRE SEULEMENT À LA CHUTE ---
        if (!isDashing) // <-- IMPORTANT: on touche pas la gravité pendant le dash
        {
            if (isSlowed)
            {
                if (rb.linearVelocity.y > 0.1f)
                    rb.gravityScale = originalGravityScale; // montée = normal
                else
                    rb.gravityScale = originalGravityScale * 0.15f; // chute = lune
            }
            else
            {
                rb.gravityScale = originalGravityScale;

                if (rb.linearVelocity.y < 0)
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    // === NOUVEAU PATTERN FREEZE ===
    public void ApplySlow(float factor, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(factor, duration));
    }

    IEnumerator SlowRoutine(float factor, float duration)
    {
        isSlowed = true;
        slowMultiplier = factor; // ça c'est pour ton déplacement horizontal (0.4)

        // GRAVITÉ LUNE -> quasi nul
        if (rb != null) rb.gravityScale = originalGravityScale * 0.15f;

        float t = 0f;
        while (t < duration)
        {
            if (spriteRenderer != null && !isHitInvincible)
                spriteRenderer.color = (Mathf.FloorToInt(t * 10f) % 2 == 0) ? Color.cyan : baseColor;
            t += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null) spriteRenderer.color = baseColor;
        slowMultiplier = 1f;
        isSlowed = false;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;

        var allPlatforms = FindObjectsOfType<PlatformEffector2D>();
        Collider2D playerCol = GetComponent<Collider2D>();
        foreach (var eff in allPlatforms)
        {
            Collider2D platCol = eff.GetComponent<Collider2D>();
            if (platCol) Physics2D.IgnoreCollision(playerCol, platCol, true);
        }
        rb.excludeLayers = layersToIgnoreDuringDash;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (dashPuffPrefab != null) Instantiate(dashPuffPrefab, transform.position, Quaternion.identity);
        if (dashTrail != null) dashTrail.emitting = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
        if (Input.GetAxisRaw("Horizontal") != 0) dashDir = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        yield return new WaitForSeconds(invisibleTime);
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        yield return new WaitForSeconds(dashDuration - invisibleTime);

        if (dashTrail != null) dashTrail.emitting = false;
        if (isSlowed)
            rb.gravityScale = originalGravityScale * 0.15f;
        else
            rb.gravityScale = originalGravityScale;

        foreach (var eff in allPlatforms)
        {
            Collider2D platCol = eff.GetComponent<Collider2D>();
            if (platCol) Physics2D.IgnoreCollision(playerCol, platCol, false);
        }
        if (!isHitInvincible) rb.excludeLayers = originalExcludeLayers;

        isDashing = false;
        isInvincible = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Shoot()
    {
        float startAngle = -spreadAngle * (bulletCount - 1) / 2f;
        for (int i = 0; i < bulletCount; i++)
        {
            float offset = startAngle + spreadAngle * i;
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, offset);
            GameObject b = Instantiate(bulletPrefab, firePoint.position, rot);
            Collider2D bulletCol = b.GetComponent<Collider2D>();
            Collider2D playerCol = GetComponent<Collider2D>();
            if (bulletCol && playerCol) Physics2D.IgnoreCollision(bulletCol, playerCol);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.layer == LayerMask.NameToLayer("Platform"))
        { isGrounded = true; coyoteTimeCounter = coyoteTime; }
    }
    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.layer == LayerMask.NameToLayer("Platform"))
            isGrounded = false;
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible) return;
        currentHealth -= damage;
        if (healthBar != null) healthBar.value = currentHealth;
        if (healthBarFill != null) healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        if (currentHealth <= 0) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
        {
            if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = StartCoroutine(HitInvincibility());
        }
    }

    IEnumerator HitInvincibility()
    {
        isHitInvincible = true;
        rb.excludeLayers = layersToIgnoreDuringDash;
        float timer = 0f;
        while (timer < hitInvincibilityDuration)
        {
            if (spriteRenderer != null && !isSlowed) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (!isDashing) rb.excludeLayers = originalExcludeLayers;
        isHitInvincible = false;
    }

    IEnumerator DisablePlatformTemporarily(Collider2D platformCol)
    {
        Collider2D playerCol = GetComponent<Collider2D>();
        PlatformEffector2D effector = platformCol.GetComponent<PlatformEffector2D>();
        if (effector != null) effector.enabled = false;
        Physics2D.IgnoreCollision(playerCol, platformCol, true);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
        yield return new WaitForSeconds(0.4f);
        Physics2D.IgnoreCollision(playerCol, platformCol, false);
        if (effector != null) effector.enabled = true;
    }
}