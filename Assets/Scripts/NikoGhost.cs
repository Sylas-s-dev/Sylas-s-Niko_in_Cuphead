using UnityEngine;
public class NikoGhost : MonoBehaviour
{
    SpriteRenderer sr;
    float timer = 0.3f;
    Color startColor = new Color(0.08f, 0.18f, 0.5f, 0.5f);

    public void Init(Sprite sprite, bool flipX, int sortingOrder, float duration)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipX = flipX;
        sr.sortingOrder = sortingOrder;
        sr.color = startColor;
        timer = duration;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0, startColor.a, timer / 0.3f);
            sr.color = c;
        }
        if (timer <= 0) Destroy(gameObject);
    }
}