using UnityEngine;

public class ScarfGunAim : MonoBehaviour
{
    public Sprite[] tirSprites; // tes 8 sprites base droite
    public Vector3[] scarfOffsets; // 8 offsets base droite
    public Transform[] firePointsRight; // 8 firepoints quand Niko regarde à DROITE
    public Transform[] firePointsLeft; // 8 firepoints quand Niko regarde à GAUCHE (à placer à la main)
    public Transform currentFirePoint;

    SpriteRenderer sr;
    Transform player;
    SpriteRenderer playerSr;
    [HideInInspector] public int currentIndex;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        player = transform.parent;
        playerSr = player.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int i = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        currentIndex = i;

        if (playerSr.flipX) // NIKO A GAUCHE
        {
            int mirrored = (4 - i + 8) % 8; // Est -> Ouest etc...
            sr.sprite = tirSprites[mirrored];
            sr.flipX = true;
            transform.localPosition = new Vector3(-scarfOffsets[mirrored].x, scarfOffsets[mirrored].y, scarfOffsets[mirrored].z);
            currentFirePoint = firePointsLeft[mirrored];
        }
        else // NIKO A DROITE
        {
            sr.sprite = tirSprites[i];
            sr.flipX = false;
            transform.localPosition = scarfOffsets[i];
            currentFirePoint = firePointsRight[i];
        }
    }
}