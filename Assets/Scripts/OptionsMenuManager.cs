using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject menuPanel; // Le Panel que tu vas créer
    public PlayerController2D player;
    public WeaponData[] allWeapons; // Glisse tes 3 WeaponData ici

    [Header("Pour plus tard")]
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    public void EquipPlume() { EquipWeapon("Plume"); }
    public void EquipMedaillon() { EquipWeapon("Medaillon"); }
    public void EquipDe() { EquipWeapon("De"); }

    void EquipWeapon(string name)
    {
        Debug.Log("Equipé : " + name);
        // ici tu mettras ta logique d'arme plus tard
        menuPanel.SetActive(false); // ferme le menu après
    }

    private bool isOpen = false;

    void Start()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f; // Pause le jeu

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    // Appelé par les boutons
    public void OnWeaponButtonClicked(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length) return;
        player.EquipWeapon(allWeapons[weaponIndex]);
        // Optionnel: tu peux fermer direct ou laisser ouvert pour tester
        // ToggleMenu();
    }

    // Pour plus tard
    public void OnVolumeChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void Resume()
    {
        if (isOpen) ToggleMenu();
    }
}