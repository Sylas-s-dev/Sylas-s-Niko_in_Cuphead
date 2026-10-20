using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Refs")]
    public GameObject gameOverPanel;
    public CanvasGroup blackFade;
    public CanvasGroup blurPanel;
    public MonoBehaviour playerMovement;

    [Header("Timings")]
    public float blurDuration = 0.8f;
    public float fadeDuration = 1.2f;
    public float waitBeforeInput = 0.5f;

    private bool isDead = false;

    void Awake()
    {
        Instance = this;
        ResetVisuals();
        // Se réinitialise à chaque reload de scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetVisuals();
    }

    void ResetVisuals()
    {
        isDead = false;
        Time.timeScale = 1f;
        if (blackFade) blackFade.alpha = 0;
        if (blurPanel) blurPanel.alpha = 0;
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (isDead) return;

        // Si le boss est mort, on ne lance PAS le game over
        if (Boss.IsBossDead) return;

        // Sécurité supplémentaire si le boss a été détruit mais le flag a pas eu le temps
        Boss boss = FindFirstObjectByType<Boss>();
        if (boss == null) return;

        isDead = true;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        if (playerMovement) playerMovement.enabled = false;
        Time.timeScale = 0.4f;

        float t = 0;
        while (t < blurDuration)
        {
            t += Time.unscaledDeltaTime;
            if (blurPanel) blurPanel.alpha = Mathf.Lerp(0, 0.6f, t / blurDuration);
            yield return null;
        }

        Time.timeScale = 1f;
        gameOverPanel.SetActive(true);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (blackFade) blackFade.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(waitBeforeInput);

        while (!Input.GetKeyDown(KeyCode.R) && !Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        // On reset AVANT de reload pour être sûr
        ResetVisuals();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}