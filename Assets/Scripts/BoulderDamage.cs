using UnityEngine;

public class Boulder : MonoBehaviour
{
    [Header("Glissade")]
    public float slideSpeed = 4.5f; // vers la gauche
    public float gravity = 3f;

    [Header("Saut")]
    public int minJumps = 1;
    public int maxJumps = 2;
    public float minJumpHeight = 2.5f; // hauteur de ta 1ere plateforme bleue
    public float maxJumpHeight = 4.5f; // hauteur de ta 2eme plateforme bleue
    public float timeBetweenJumpsMin = 0.8f;
    public float timeBetweenJumpsMax = 2f;

    public int damage = 1;

    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpsDone = 0;
    private int jumpsToDo;
    private float nextJumpTime;
    private float groundCheckDist = 0.6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        rb.freezeRotation = true; // reste carré, il glisse, il tourne pas
        rb.linearDamping = 0f;

        // 0 friction pour bien glisser
        var col = GetComponent<BoxCollider2D>();
        var mat = new PhysicsMaterial2D();
        mat.friction = 0f;
        mat.bounciness = 0f;
        col.sharedMaterial = mat;

        jumpsToDo = Random.Range(minJumps, maxJumps + 1);
        nextJumpTime = Time.time + Random.Range(timeBetweenJumpsMin, timeBetweenJumpsMax);
    }

    void FixedUpdate()
    {
        // check sol
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDist, LayerMask.GetMask("Ground"));

        // glissade constante de droite à gauche
        float yVel = rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(-slideSpeed, yVel);

        // saut hasardeux
        if (isGrounded && jumpsDone < jumpsToDo && Time.time >= nextJumpTime)
        {
            Jump();
            jumpsDone++;
            nextJumpTime = Time.time + Random.Range(timeBetweenJumpsMin, timeBetweenJumpsMax);
        }

        // détruit si sorti à gauche
        if (transform.position.x < -15f) Destroy(gameObject);
    }

    void Jump()
    {
        float heightWanted = Random.Range(minJumpHeight, maxJumpHeight);
        float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float jumpVel = Mathf.Sqrt(2f * g * heightWanted);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.TryGetComponent<PlayerController2D>(out var player))
        {
            if (!player.IsInvincible) player.TakeDamage(damage);
        }
    }
}