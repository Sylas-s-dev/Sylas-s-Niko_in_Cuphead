using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerSunHealthBar : MonoBehaviour
{
    [Header("0 = gauche, 2 = droite")]
    public Image[] suns = new Image[3];
    public Sprite sunOn;
    public Sprite sunOff;
    public Sprite[] turnOffFrames;

    [Header("1 PV")]
    public Color lowHealthColor = new Color(1f, 0.6f, 0.6f, 1f);
    public Color normalColor = Color.white;
    Coroutine lowHpPulse;

    public void InitBar()
    {
        for (int i = 0; i < 3; i++)
        {
            suns[i].sprite = sunOn;
            suns[i].color = normalColor;
            suns[i].transform.localScale = Vector3.one;
        }
        if (lowHpPulse != null) StopCoroutine(lowHpPulse);
    }

    public void UpdateHealth(int currentHealth)
    {
        int index = currentHealth;
        if (index >= 0 && index < 3)
        {
            StartCoroutine(TurnOffAnim(index));
        }

        // 1 PV -> dernier soleil rouge fixe
        if (currentHealth == 1)
        {
            suns[0].color = lowHealthColor;
        }
    }

    IEnumerator TurnOffAnim(int index)
    {
        foreach (var frame in turnOffFrames)
        {
            suns[index].sprite = frame;
            yield return new WaitForSeconds(0.06f);
        }
        suns[index].sprite = sunOff;
        suns[index].color = normalColor; // le soleil mort reste normal
    }

    IEnumerator PulseRed()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * 4f;
            float scale = 1f + Mathf.Sin(t) * 0.15f; // petit gonflement Cuphead
            suns[0].transform.localScale = Vector3.one * scale;
            // petit clignotement rouge -> blanc rosé
            suns[0].color = Color.Lerp(lowHealthColor, Color.white, (Mathf.Sin(t) + 1f) / 2f * 0.4f);
            yield return null;
        }
    }
}