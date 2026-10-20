using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public DialogueManager dm;
    public CanvasGroup blackScreen;

    void Start()
    {
        blackScreen.alpha = 1f;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1. Fondu d'ouverture
        float t = 3f;
        while (t > 0)
        {
            t -= Time.deltaTime * 0.6f;
            blackScreen.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Dialogue 4e mur
        yield return dm.TypeDialogue($"God... What is...");
        yield return dm.TypeDialogue($"[...]");
        yield return dm.TypeDialogue($"So it was just a dream?");
        yield return dm.TypeDialogue($"God, are you there?");
        yield return dm.TypeDialogue($"[...]");
        yield return dm.TypeDialogue($"I have a bad feeling about what's next...");

        // 3. Fondu au noir
        t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * 0.4f;
            blackScreen.alpha = t;
            Camera.main.orthographicSize = Mathf.Lerp(5f, 5.5f, t);
            yield return null;
        }

        yield return dm.TypeDialogue($"*So Niko continued their adventure, forgetting today's dream...*");
    }
}