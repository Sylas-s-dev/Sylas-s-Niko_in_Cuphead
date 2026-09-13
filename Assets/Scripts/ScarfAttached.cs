using UnityEngine;
public class ScarfAttached : MonoBehaviour
{
    Transform follow;
    float duration;
    float timer;
    Vector3 baseScale;
    float dashDir;

    public void Attach(Transform neck, float dur, float dir)
    {
        follow = neck;
        duration = dur;
        timer = dur;
        dashDir = dir;
        baseScale = transform.localScale;
        transform.SetParent(neck);
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        float progress = 1f - (timer / duration);
        float stretch = Mathf.Lerp(0.8f, 3f, progress);

        // dash à droite (1) = écharpe à gauche, dash à gauche (-1) = écharpe à droite
        transform.localScale = new Vector3(baseScale.x * Mathf.Sign(dashDir) * stretch * 1f, baseScale.y, baseScale.z);

        if (timer <= 0) Destroy(gameObject);
    }
}