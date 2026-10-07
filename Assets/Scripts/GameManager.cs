using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Tes variables qui survivent entre les scènes
    public bool isComingFromDream = false;
    public string playerName = "Player";

    void Awake()
    {
        // Singleton + DontDestroy
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // si un autre GameManager existe déjà, on supprime celui-là
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // <- C'EST CETTE LIGNE qui le fait survivre
    }
}