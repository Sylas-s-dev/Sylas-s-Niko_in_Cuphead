using UnityEngine;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float horizontalSpeed = 5f;
    public float minBounce = 4f;
    public float maxBounce = 9f;
    public int damage = 1;
    public float dir = -1f;
    private Rigidbody2D rb;
    private float nextJump;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        nextJump = Time.time + Random.Range(1f, 4f);
        if (rb != null) rb.freezeRotation = true;
    }

    void Start()
    {
        // On ignore les plateformes par leur NOM, pas par Tag = plus de crash
        var myCol = GetComponent<Collider2D>();
        if (myCol != null)
        {
            foreach (var col in FindObjectsOfType<Collider2D>())
            {
                if (col.gameObject.name.ToLower().Contains("plateform"))
                {
                    Physics2D.IgnoreCollision(myCol, col, true);
                }
            }
        }
        Destroy(gameObject, 12f);
    }

    public void Init(float direction, float speed = 5f)
    {
        dir = direction >= 0 ? 1f : -1f;
        horizontalSpeed = Mathf.Abs(speed);
        if (rb != null) rb.linearVelocity = new Vector2(dir * horizontalSpeed, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(dir * horizontalSpeed, rb.linearVelocity.y);
        if (isGrounded && Time.time > nextJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Random.Range(minBounce, maxBounce));
            nextJump = Time.time + Random.Range(0.7f, 2.2f);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground")) { isGrounded = true; return; }
        if (col.gameObject.name.ToLower().Contains("balle")) { Destroy(col.gameObject); return; }

        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
    void OnCollisionStay2D(Collision2D col) { if (col.gameObject.CompareTag("Ground")) isGrounded = true; }
    void OnCollisionExit2D(Collision2D col) { if (col.gameObject.CompareTag("Ground")) isGrounded = false; }
}