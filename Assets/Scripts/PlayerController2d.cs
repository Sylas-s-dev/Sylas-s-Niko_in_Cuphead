using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Invincibilité après hit")]
    public float hitInvincibilityDuration = 1f;
    public float blinkInterval = 0.1f;
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
    public bool isDashing = false;
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible || isDashing || isHitInvincible;

    [Header("Dash FX")]
    public GameObject dashPuffPrefab;
    public float invisibleTime = 0.15f;
    public GameObject scarfDashGhostPrefab; // ton écharpe rouge
    public GameObject nikoGhostPrefab;
    public int ghostCount = 4;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Vie")]
    public int maxHealth = 5;
    public int currentHealth;
    public Slider healthBar;
    public Image healthBarFill;

    [Header("Deplacement")]
    public float speed = 7f;

    [Header("Saut Variable")]
    public float jumpForce = 10f;
    public float jumpTime = 0.20f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.5f;

    [Header("Tir Multi")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int bulletCount = 1;
    public float spreadAngle = 15f;

    [Header("Scarf Gun")]
    public ScarfGunAim scarfGun;

    [Header("Scarf Dash")]
    public Transform neckPoint;
    public Vector2 neckOffset = new Vector2(0f, 0.35f);
    public float scarfGhostDuration = 0.25f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private float nextFireTime;
    private bool isGrounded;
    private float jumpTimeCounter;
    private bool isJumping;
    private LayerMask originalExcludeLayers;
    private Camera cam;

    [Header("Dash Collision")]
    public LayerMask groundLayer;
    LayerMask savedExclude;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        cam = Camera.main;
        originalGravityScale = rb.gravityScale;
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
                spriteRenderer.flipX = move < 0;
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                StartCoroutine(Dash());
        }

        if (anim != null)
        {
            bool falling = !isGrounded && rb.linearVelocity.y < -0.1f;
            anim.SetBool("IsFalling", falling);
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }

        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < -0.5f)
            && Input.GetButtonDown("Jump") && isGrounded)
        {
            Collider2D platform = Physics2D.OverlapBox(transform.position + Vector3.down * 0.8f, new Vector2(1f, 0.2f), 0f, LayerMask.GetMask("Platform"));
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

        if (isDashing) return;
        if ((Input.GetKey(KeyCode.X) || Input.GetMouseButton(0)) && Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        if (!isDashing)
        {
            if (isSlowed)
                rb.gravityScale = (rb.linearVelocity.y > 0.1f) ? originalGravityScale : originalGravityScale * 0.15f;
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

    public void ApplySlow(float factor, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(factor, duration));
    }

    IEnumerator SlowRoutine(float factor, float duration)
    {
        isSlowed = true;
        slowMultiplier = factor;
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

    void SpawnGhost()
    {
        if (nikoGhostPrefab == null || spriteRenderer == null) return;
        GameObject g = Instantiate(nikoGhostPrefab, transform.position, Quaternion.identity);
        g.transform.localScale = transform.localScale;
        var ghost = g.GetComponent<NikoGhost>();
        if (ghost != null) ghost.Init(spriteRenderer.sprite, spriteRenderer.flipX, spriteRenderer.sortingOrder - 1, 0.3f);
    }

    IEnumerator SpawnGhostsRoutine()
    {
        for (int i = 0; i < ghostCount; i++)
        {
            SpawnGhost();
            yield return new WaitForSeconds(invisibleTime / ghostCount);
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;
        if (anim != null) anim.SetTrigger("Dash");

        Vector3 startPos = transform.position;
        float dashDir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
        if (Input.GetAxisRaw("Horizontal") != 0) dashDir = Mathf.Sign(Input.GetAxisRaw("Horizontal"));
        Vector3 endPos = startPos + new Vector3(dashDir * dashSpeed * dashDuration, 0, 0);

        // bloque le mouvement
        Vector2 savedVel = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // on ignore tout sauf le sol pendant le dash
        savedExclude = rb.excludeLayers;
        rb.excludeLayers = ~groundLayer; // exclut tout sauf Ground
        if (scarfGun != null) scarfGun.isDashing = true;

        if (scarfGun != null) scarfGun.ForceHideInstant();
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (dashPuffPrefab != null) Instantiate(dashPuffPrefab, startPos, Quaternion.identity);
        if (dashTrail != null) dashTrail.emitting = true;

        if (scarfDashGhostPrefab != null)
        {
            Transform neck = neckPoint != null ? neckPoint : transform;
            GameObject scarf = Instantiate(scarfDashGhostPrefab, neck.position, Quaternion.identity);
            var attached = scarf.GetComponent<ScarfAttached>();
            if (attached != null) attached.Attach(neck, scarfGhostDuration, dashDir);
        }

        StartCoroutine(SpawnGhostsRoutine());

        // on se déplace progressivement, pas en téléportation à la fin
        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            float progress = t / dashDuration;
            rb.MovePosition(Vector3.Lerp(startPos, endPos, progress));
            yield return null;
        }
        rb.MovePosition(endPos);

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (scarfGun != null) scarfGun.ResetAfterDash();
        if (dashPuffPrefab != null) Instantiate(dashPuffPrefab, endPos, Quaternion.identity);
        if (dashTrail != null) dashTrail.emitting = false;

        rb.gravityScale = isSlowed ? originalGravityScale * 0.15f : originalGravityScale;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.excludeLayers = isHitInvincible ? savedExclude : originalExcludeLayers;
        if (scarfGun != null) scarfGun.isDashing = false;

        isDashing = false;
        isInvincible = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Shoot()
    {
        if (scarfGun == null || scarfGun.currentFirePoint == null) return;
        Transform fp = scarfGun.currentFirePoint;
        int dirIndex = scarfGun.currentIndex;
        float baseAngle = dirIndex * 45f;
        scarfGun.ShowScarf();
        Quaternion rot = Quaternion.Euler(0, 0, baseAngle);
        GameObject b = Instantiate(bulletPrefab, fp.position, rot);
        var rbBullet = b.GetComponent<Rigidbody2D>();
        if (rbBullet != null) rbBullet.linearVelocity = rot * Vector2.right * 15f;
        Collider2D bulletCol = b.GetComponent<Collider2D>();
        Collider2D playerCol = GetComponent<Collider2D>();
        if (bulletCol && playerCol) Physics2D.IgnoreCollision(bulletCol, playerCol);
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