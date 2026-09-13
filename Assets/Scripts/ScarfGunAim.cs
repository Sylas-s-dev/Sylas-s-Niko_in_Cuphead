using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ScarfGunAim : MonoBehaviour
{
    public Sprite[] tirSprites;
    [FormerlySerializedAs("scarfOffsets")]
    public Vector3[] scarfOffsets;
    [FormerlySerializedAs("firePoints")]
    public Transform[] firePointsRight;
    public Transform[] firePointsLeft;
    public Transform currentFirePoint;
    [HideInInspector] public bool isDashing = false;

    SpriteRenderer sr;
    Transform player;
    SpriteRenderer playerSr;
    [HideInInspector] public int currentIndex;
    Coroutine scarfAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        player = transform.parent;
        playerSr = player.GetComponent<SpriteRenderer>();
        sr.enabled = false;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int i = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        currentIndex = i;

        if (playerSr.flipX)
        {
            int mirrored = (4 - i + 8) % 8;
            sr.sprite = tirSprites[mirrored];
            sr.flipX = true;
            transform.localPosition = new Vector3(-scarfOffsets[mirrored].x, scarfOffsets[mirrored].y, scarfOffsets[mirrored].z);
            currentFirePoint = firePointsLeft.Length > mirrored ? firePointsLeft[mirrored] : null;
        }
        else
        {
            sr.sprite = tirSprites[i];
            sr.flipX = false;
            transform.localPosition = scarfOffsets[i];
            currentFirePoint = firePointsRight[i];
        }
    }
    float hideAt;
    bool isVisible = false;

    public void ShowScarf()
    {
        if (isDashing) return; // bloque le tir pendant le dash

        hideAt = Time.time + 0.4f;
        if (!isVisible)
        {
            if (scarfAnim != null) StopCoroutine(scarfAnim);
            scarfAnim = StartCoroutine(ScarfPop());
        }
    }

    IEnumerator ScarfPop()
    {
        isVisible = true;
        sr.enabled = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.08f;
            float eased = 1f - Mathf.Pow(1f - t, 3f); // easeOutCubic propre
            transform.localScale = new Vector3(eased, eased, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;

        // attend que tu arrêtes de tirer
        while (Time.time < hideAt)
            yield return null;

        // RETRACT seulement maintenant
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.1f;
            transform.localScale = new Vector3(1f - t, 1f - t, 1f);
            yield return null;
        }

        sr.enabled = false;
        transform.localScale = Vector3.one;
        isVisible = false;
    }
    public void ForceHideInstant()
    {
        if (scarfAnim != null) StopCoroutine(scarfAnim);
        scarfAnim = null;
        sr.enabled = false;
        transform.localScale = Vector3.one;
        isVisible = false;
    }

    public void ResetAfterDash()
    {
        ForceHideInstant();
    }
}