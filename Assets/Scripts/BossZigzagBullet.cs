using UnityEngine;

public class BossZigzagBullet : MonoBehaviour
{
    [Header("Zigzag")]
    public float speed = 6f;
    public float amplitude = 1.8f; // hauteur du zigzag
    public float frequency = 6f;   // vitesse du zigzag

    private Rigidbody2D rb;
    private float dirX = -1f; // le boss est à droite donc ça part à gauche

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // On détruit après 5 sec pour pas polluer
        Destroy(gameObject, 5f);

        // Si ton boss est à gauche parfois, on détecte la direction auto
        if (transform.localScale.x < 0) dirX = 1f;
    }

    void FixedUpdate()
    {
        // Mouvement de base vers le joueur
        float xVel = dirX * speed;
        // Mouvement en sinus pour le zigzag vertical
        float yVel = Mathf.Sin(Time.time * frequency) * amplitude;

        // On combine, ça passe entre les plateformes
        rb.linearVelocity = new Vector2(xVel, yVel);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si tu veux qu'elles rebondissent un peu sur les plateformes au lieu de passer à travers
        // décommente ça
         //if (other.CompareTag("Ground")) {
             //amplitude = -amplitude;
         //}

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController2D>().TakeDamage(1);
            Destroy(gameObject);
        }
    }
}