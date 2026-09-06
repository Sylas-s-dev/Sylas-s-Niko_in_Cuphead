using UnityEngine;

public class ZigzagOnlyY : MonoBehaviour
{
    public float amplitude = 2f;
    public float frequency = 4f;
    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        if (rb == null) return;
        // On garde le X qu'on a forcé dans Boss.cs, on ne change que le Y
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Sin(Time.time * frequency) * amplitude);
    }
}