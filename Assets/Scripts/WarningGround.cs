using UnityEngine;
using System.Collections;

public class WarningGround : MonoBehaviour
{
    public float blinkSpeed = 0.15f;
    public float lifetime = 2.5f;
    public AudioClip tickSound;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        StartCoroutine(BlinkAndDie());
    }

    IEnumerator BlinkAndDie()
    {
        float t = 0f;
        while (t < lifetime)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            // optionnel: petit son tick
            // if (tickSound) AudioSource.PlayClipAtPoint(tickSound, transform.position);

            // accélère à la fin
            float wait = (t > lifetime * 0.7f) ? blinkSpeed * 0.5f : blinkSpeed;
            t += wait;
            yield return new WaitForSeconds(wait);
        }
        Destroy(gameObject);
    }
}