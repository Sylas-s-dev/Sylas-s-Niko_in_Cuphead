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
    public GameObject scarfDashGhostPrefab;
    public GameObject nikoGhostPrefab;
    public int ghostCount = 4;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Vie")]
    public int maxHealth = 3;
    public int currentHealth;
    public PlayerSunHealthBar sunBar;

    [Header("Deplacement")]
    public float speed = 7f;

    [Header("Saut Variable")]
    public float jumpForce = 10f;
    public float jumpTime = 0.20f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.5f;
    [SerializeField] float runJumpThreshold = 3f;

    [Header("Armes - Nouveau Système")]
    public WeaponData currentWeapon; // Glisse ta Plume ici par défaut
    public GameObject bulletPrefab;
    public Transform firePoint; // fallback si pas de ScarfGun
    public float fireRate = 0.2f; // sera écrasé par WeaponData si présent

    [Header("Scarf Gun")]
    public ScarfGunAim scarfGun;
    [Header("Scarf Dash")]
    public Transform neckPoint;
    public Vector2 neckOffset = new Vector2(0f, 0.35f);
    public float scarfGhostDuration = 0.25f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;
    private float nextFireTime;
    private bool isGrounded;
    private float jumpTimeCounter;
    private bool isJumping;
    private LayerMask originalExcludeLayers;
    private Camera cam;
    [Header("Dash Collision")]
    public LayerMask groundLayer;
    LayerMask savedExclude;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
        cam = Camera.main;
        originalGravityScale = rb.gravityScale;
        originalExcludeLayers = rb.excludeLayers;
        slowMultiplier = 1f;
        currentHealth = maxHealth;
        if (sunBar != null) sunBar.InitBar();

        if (currentWeapon != null) fireRate = currentWeapon.fireRate;
    }

    // APPELÉ PAR TON MENU OPTION
    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        fireRate = newWeapon.fireRate;
        if (newWeapon.weaponAnimator != null && animator != null)
        {
            animator.runtimeAnimatorController = newWeapon.weaponAnimator;
        }
        Debug.Log("Arme équipée: " + newWeapon.weaponName);
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");
        Collider2D col = GetComponent<Collider2D>();
        LayerMask solMask = groundLayer;
        if (solMask == 0) solMask = LayerMask.GetMask("Ground", "Platform");
        Bounds b = col.bounds;
        Vector2 feetPos = new Vector2(b.center.x, b.min.y - 0.05f);
        isGrounded = Physics2D.OverlapBox(feetPos, new Vector2(b.size.x * 0.8f, 0.1f), 0f, solMask) != null;

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        bool isRunningJump = Mathf.Abs(move) > 0.1f;
        animator.SetBool("IsRunningJump", isRunningJump);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(move * speed * slowMultiplier, rb.linearVelocity.y);
            if (move != 0 && spriteRenderer != null) spriteRenderer.flipX = move < 0;
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) StartCoroutine(Dash());
        }

        if (animator != null)
        {
            bool falling = !isGrounded && rb.linearVelocity.y < -0.1f;
            animator.SetBool("IsFalling", falling);
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
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

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            coyoteTimeCounter = 0f;
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

    void LateUpdate()
    {
        if (isDashing) return;
        float ppu = 40f;
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x * ppu) / ppu;
        pos.y = Mathf.Round(pos.y * ppu) / ppu;
        transform.position = pos;
    }

    // --- TIR MODIFIÉ ---
    void Shoot()
    {
        Transform fp = null;
        int dirIndex = 0;
        float baseAngle = 0f;

        if (scarfGun != null && scarfGun.currentFirePoint != null)
        {
            fp = scarfGun.currentFirePoint;
            dirIndex = scarfGun.currentIndex;
            baseAngle = dirIndex * 45f;
            scarfGun.ShowScarf();
        }
        else if (firePoint != null)
        {
            fp = firePoint;
            baseAngle = spriteRenderer.flipX ? 180f : 0f;
        }
        else return;

        // Si pas d'arme assignée -> comportement de base (ta Plume)
        int count = currentWeapon != null ? currentWeapon.projectileCount : 1;
        float spread = currentWeapon != null ? currentWeapon.spreadAngle : 0f;
        float projSpeed = currentWeapon != null ? currentWeapon.projectileSpeed : 15f;

        for (int i = 0; i < count; i++)
        {
            bool isRedDice = false;
            float dmgMult = 1f;

            if (currentWeapon != null)
            {
                dmgMult = currentWeapon.damageMultiplier;
                // Dé : 5% de chance rouge x2 dégats
                if (currentWeapon.weaponName == "Dé" && Random.value < 0.05f)
                {
                    isRedDice = true;
                    dmgMult *= 2f;
                }
            }

            float angleOffset = 0f;
            if (count > 1)
            {
                angleOffset = -spread / 2f + (spread / (count - 1)) * i;
            }

            Quaternion rot = Quaternion.Euler(0, 0, baseAngle + angleOffset);
            GameObject b = Instantiate(bulletPrefab, fp.position, rot);

            // On essaye d'init ta balle si elle a le script Balle
            var balleScript = b.GetComponent<Bullet>(); // ou Balle selon ton nom
            if (balleScript != null)
            {
                // Il faudra ajouter une méthode Init dans BalleP1
                balleScript.Init(dmgMult, projSpeed, currentWeapon != null && currentWeapon.hasHoming, currentWeapon != null ? currentWeapon.homingStrength : 0f, isRedDice);
            }
            else
            {
                var rbBullet = b.GetComponent<Rigidbody2D>();
                if (rbBullet != null) rbBullet.linearVelocity = rot * Vector2.right * projSpeed;
            }

            Collider2D bulletCol = b.GetComponent<Collider2D>();
            Collider2D playerCol = GetComponent<Collider2D>();
            if (bulletCol && playerCol) Physics2D.IgnoreCollision(bulletCol, playerCol);
        }
    }

    //... Le reste de ton code (Slow, Ghost, Dash, TakeDamage) reste identique...
    public void ApplySlow(float factor, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(factor, duration));
    }
    IEnumerator SlowRoutine(float factor, float duration)
    {
        isSlowed = true; slowMultiplier = factor; float t = 0f;
        while (t < duration)
        {
            if (spriteRenderer != null && !isHitInvincible)
                spriteRenderer.color = (Mathf.FloorToInt(t * 10f) % 2 == 0) ? Color.cyan : baseColor;
            t += 0.1f; yield return new WaitForSeconds(0.1f);
        }
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
        slowMultiplier = 1f; isSlowed = false;
    }
    void SpawnGhost()
    {
        if (nikoGhostPrefab == null || spriteRenderer == null) return;
        GameObject g = Instantiate(nikoGhostPrefab, transform.position, Quaternion.identity);
        g.transform.localScale = transform.localScale;
        var ghost = g.GetComponent<NikoGhost>();
        if (ghost != null) ghost.Init(spriteRenderer.sprite, spriteRenderer.flipX, spriteRenderer.sortingOrder - 1, 0.3f);
        if (scarfDashGhostPrefab != null && neckPoint != null)
        {
            GameObject scarfG = Instantiate(scarfDashGhostPrefab, neckPoint.position, neckPoint.rotation);
            var sr = scarfG.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = spriteRenderer.flipX;
            Destroy(scarfG, scarfGhostDuration);
        }
    }
    IEnumerator SpawnGhostsRoutine()
    {
        SpawnGhost(); yield return null;
        for (int i = 1; i < ghostCount; i++) { SpawnGhost(); yield return new WaitForSeconds(invisibleTime / ghostCount); }
    }
    IEnumerator Dash()
    {
        canDash = false; isDashing = true; isInvincible = true;
        if (animator != null) animator.SetTrigger("Dash");
        float dashDir = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(dashDir) < 0.1f) dashDir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
        if (dashDir == 0) dashDir = 1f;
        float savedGravity = rb.gravityScale; rb.gravityScale = 0f; Vector2 savedVel = rb.linearVelocity;
        savedExclude = rb.excludeLayers; rb.excludeLayers = originalExcludeLayers | layersToIgnoreDuringDash;
        rb.excludeLayers &= ~groundLayer;
        if (scarfGun != null) { scarfGun.isDashing = true; scarfGun.ForceHideInstant(); }
        if (dashPuffPrefab != null) Instantiate(dashPuffPrefab, transform.position, Quaternion.identity);
        if (dashTrail != null) dashTrail.emitting = true;
        StartCoroutine(SpawnGhostsRoutine()); yield return null;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = new Vector2(0f, savedVel.y * 0.2f);
        rb.gravityScale = isSlowed ? originalGravityScale * 0.15f : savedGravity;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (scarfGun != null) scarfGun.ResetAfterDash();
        if (dashPuffPrefab != null) Instantiate(dashPuffPrefab, transform.position, Quaternion.identity);
        if (dashTrail != null) dashTrail.emitting = false;
        if (isHitInvincible) rb.excludeLayers = originalExcludeLayers | layersToIgnoreDuringDash;
        else rb.excludeLayers = originalExcludeLayers;
        rb.excludeLayers &= ~groundLayer;
        if (scarfGun != null) scarfGun.isDashing = false; isDashing = false; isInvincible = false;
        yield return new WaitForSeconds(dashCooldown); canDash = true;
    }
    public void TakeDamage(int damage)
    {
        if (IsInvincible) return; currentHealth -= damage;
        if (sunBar != null) sunBar.UpdateHealth(currentHealth);
        if (currentHealth <= 0) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else { if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine); invincibilityCoroutine = StartCoroutine(HitInvincibility()); }
    }
    IEnumerator HitInvincibility()
    {
        isHitInvincible = true; rb.excludeLayers = originalExcludeLayers | layersToIgnoreDuringDash;
        rb.excludeLayers &= ~groundLayer; float timer = 0f;
        while (timer < hitInvincibilityDuration)
        {
            if (spriteRenderer != null && !isSlowed) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval); timer += blinkInterval;
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (!isDashing) rb.excludeLayers = originalExcludeLayers; isHitInvincible = false;
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