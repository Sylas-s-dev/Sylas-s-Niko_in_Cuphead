using UnityEngine;
public class ScarfDashGhost : MonoBehaviour
{
    public float life = 0.25f;
    SpriteRenderer sr;
    void Awake() { sr = GetComponent<SpriteRenderer>(); }
    void Start() { Destroy(gameObject, life); }
    public void Init(bool flipX) { if (sr) sr.flipX = flipX; }
    void Update()
    {
        if (sr) { var c = sr.color; c.a -= Time.deltaTime * 3f; sr.color = c; }
    }
}