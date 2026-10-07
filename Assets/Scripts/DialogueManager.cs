using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public float letterDelay = 0.03f;

    public bool isWaitingForInput = false;

    public IEnumerator TypeDialogue(string line)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        isWaitingForInput = true;
        // attend un clic ou Espace
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
        isWaitingForInput = false;

        dialogueBox.SetActive(false);
    }
}