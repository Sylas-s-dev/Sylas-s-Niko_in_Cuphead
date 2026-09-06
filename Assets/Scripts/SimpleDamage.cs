using UnityEngine;

public class SimpleDamage : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            other.SendMessage("TakeDamagePlayer", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}