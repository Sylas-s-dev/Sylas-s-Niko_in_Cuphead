using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DreamIntroAuto : MonoBehaviour
{
    public DialogueManager dm;
    public CanvasGroup blackScreen;
    public Animator nikoSleepAnimator; // Animator de Niko couché avec 2 states: EyesOpen / EyesClosing
    public string bossSceneName = "BossDreamFight";
    public string playerName = "Player";

    void Start()
    {
        // s'assure qu'on commence dans le noir puis on s'éclaircit sur Niko au lit
        blackScreen.alpha = 1f;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {      
        // 1. Fondu d'ouverture sur Niko déjà au lit, yeux ouverts
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime * 0.6f;
            blackScreen.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Dialogue vers le joueur - 4e mur
        yield return dm.TypeDialogue($"Niko : {playerName}... t'es encore là ?");
        yield return dm.TypeDialogue($"Niko : J'arrive plus à garder les yeux ouverts...");
        yield return dm.TypeDialogue($"Niko : Bonne nuit. A tout à l'heure dans mon rêve...");

        // 3. Il s'endort - anim yeux qui se ferment
        nikoSleepAnimator.SetTrigger("FallAsleep");
        yield return new WaitForSeconds(1.2f);

        // 4. Fondu au noir + effet rêve
        t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * 0.4f; // plus lent = plus onirique
            blackScreen.alpha = t;
            // optionnel: petit scale sur la cam pour effet vertige
            Camera.main.orthographicSize = Mathf.Lerp(5f, 5.5f, t);
            yield return null;
        }

        yield return dm.TypeDialogue($"* ... *");
        yield return new WaitForSeconds(0.5f);

        // 5. Lancement de ton fight
        GameManager.Instance.isComingFromDream = false;
        SceneManager.LoadScene(bossSceneName);
    }
}