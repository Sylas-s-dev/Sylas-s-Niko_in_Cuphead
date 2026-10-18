using UnityEngine;

public class BossActivatorOrb : MonoBehaviour
{
    [Header("Refs")]
    public Boss boss; // glisse ton Boss ici
    public GameObject tutorialCanvas; // glisse ton Canvas Tuto ici

    [Header("Option")]
    public bool destroyCanvas = true; // si false, il fait juste SetActive(false)
    public string autoFindTag = "Tutorial"; // si tu n'as rien assigné, il cherche par Tag

    private bool used = false;

    void Start()
    {
        // auto-find si t'as rien assigné dans l'inspector
        if (boss == null) boss = FindFirstObjectByType<Boss>();

        if (tutorialCanvas == null)
        {
            // cherche par Tag
            if (!string.IsNullOrEmpty(autoFindTag))
            {
                var tagged = GameObject.FindGameObjectWithTag(autoFindTag);
                if (tagged != null) tutorialCanvas = tagged;
            }
            // cherche par nom en fallback
            if (tutorialCanvas == null)
            {
                var found = GameObject.Find("TutorialCanvas");
                if (found == null) found = GameObject.Find("CanvasTuto");
                if (found == null) found = GameObject.Find("TutoCanvas");
                tutorialCanvas = found;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;

        // 1. Active le boss
        if (boss != null) boss.ActivateBoss();

        // 2. Cache / détruit le canvas tuto
        if (tutorialCanvas != null)
        {
            if (destroyCanvas) Destroy(tutorialCanvas);
            else tutorialCanvas.SetActive(false);
        }

        // 3. Détruit l'orbe
        Destroy(gameObject);
    }
}