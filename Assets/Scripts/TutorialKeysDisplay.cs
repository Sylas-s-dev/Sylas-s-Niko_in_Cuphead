using UnityEngine;
using System.Collections;

public class TutorialKeysDisplay : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 2f;

    private bool isHiding = false;

    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
    }

    public void HideNow()
    {
        if (!isHiding) StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        isHiding = true;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}