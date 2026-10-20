using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DreamIntroAuto : MonoBehaviour
{
    public DialogueManager dm;
    public CanvasGroup blackScreen;
    public Animator nikoSleepAnimator;

    [Header("Séquence Rêve")]
    public GameObject dreamRoot; // Un parent avec tout ton décor de rêve (à désactiver au début)
    public Animator dreamAnimator; // Animator avec une anim "DreamLoop" : yeux qui flottent, glitch, etc.
    public float dreamDuration = 4f;
    public AudioSource dreamAudio; // petit son onirique / basse

    public string bossSceneName = "BossDreamFight";
    public string playerName = "God";

    void Start()
    {
        blackScreen.alpha = 1f;
        if (dreamRoot) dreamRoot.SetActive(false);
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        
        blackScreen.alpha = 1;
        Camera.main.orthographicSize = Mathf.Lerp(5f, 5.5f, 1);
        yield return null;
        

        yield return dm.TypeDialogue($"*The Refuge - Niko is resting on their bed in the room with the empty flowerpots.*");
        // 1. Fondu d'ouverture
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime * 0.6f;
            blackScreen.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Dialogue 4e mur
        yield return dm.TypeDialogue($"{playerName}... you're still here?");
        yield return dm.TypeDialogue($"[...]");
        yield return dm.TypeDialogue($"I can't keep my eyes open anymore...");
        yield return dm.TypeDialogue($"Good night. See you later...");

        // 3. Il s'endort
        nikoSleepAnimator.SetTrigger("FallAsleep");
        yield return new WaitForSeconds(1.2f);

        // 4. Fondu au noir
        t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * 0.4f;
            blackScreen.alpha = t;
            Camera.main.orthographicSize = Mathf.Lerp(5f, 5.5f, t);
            yield return null;
        }

        yield return dm.TypeDialogue($"* ... *");

        // --- 4.5 NOUVELLE SÉQUENCE RÊVE ---
        // On est dans le noir total, on lance le rêve
        if (dreamRoot) dreamRoot.SetActive(true);
        if (dreamAudio) dreamAudio.Play();
        if (dreamAnimator) dreamAnimator.SetTrigger("StartDream");

        // On ré-ouvre légèrement les yeux DANS le rêve
        t = 1f;
        while (t > 0f) // on ne revient pas à 0, on reste dans un noir laiteux
        {
            t -= Time.deltaTime * 0.5f;
            blackScreen.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(dreamDuration); // c'est ton animation qui tourne

        // Fin du rêve - on replonge au noir brutal
        if (dreamAnimator) dreamAnimator.SetTrigger("EndDream");
        t = 0.3f;
        while (t < 1)
        {
            t += Time.deltaTime * 0.8f; // plus rapide = réveil brutal
            blackScreen.alpha = t;
            yield return null;
        }

        // 5. Lancement du bossfight
        GameManager.Instance.isComingFromDream = false;
        SceneManager.LoadScene(bossSceneName);
    }
}