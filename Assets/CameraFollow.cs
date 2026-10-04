using UnityEngine;

public class CupheadCamera : MonoBehaviour
{
    public Transform player;
    public Transform boss;
    public Transform arenaCenter; // mets un empty au centre de ton arène P3
    public float smooth = 3f;
    public Vector2 maxOffset = new Vector2(1.5f, 1f); // petit mouvement Cuphead

    private Vector3 basePos;

    void Start()
    {
        if (arenaCenter != null) basePos = arenaCenter.position;
        else basePos = transform.position;
        basePos.z = -10f;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // On vise le centre de l'arène, pas le joueur
        Vector3 target = basePos;

        // On ajoute un TOUT PETIT offset vers le joueur pour donner de la vie
        // comme Cuphead, max 1.5 unités, pas plus
        Vector3 playerOffset = player.position - basePos;
        playerOffset.x = Mathf.Clamp(playerOffset.x * 0.15f, -maxOffset.x, maxOffset.x);
        playerOffset.y = Mathf.Clamp(playerOffset.y * 0.15f, -maxOffset.y, maxOffset.y);
        playerOffset.z = 0;

        target += playerOffset;

        transform.position = Vector3.Lerp(transform.position, target, smooth * Time.deltaTime);
    }
}