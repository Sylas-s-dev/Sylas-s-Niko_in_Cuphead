using UnityEngine;

public class OptionsMenuManager : MonoBehaviour
{
    public GameObject menuOption;
    public PlayerController2D player; // glisse ton Player ici

    [Header("Tes 3 armes")]
    public WeaponData plumeData;
    public WeaponData deData;
    public WeaponData medaillonData;

    void Start()
    {
        menuOption.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            menuOption.SetActive(!menuOption.activeSelf);
    }

    // Ces 3 fonctions sont celles que tu as mis dans tes boutons OnClick
    public void EquipPlume()
    {
        player.EquipWeapon(plumeData);
        menuOption.SetActive(false);
    }

    public void EquipDe()
    {
        player.EquipWeapon(deData);
        menuOption.SetActive(false);
    }

    public void EquipMedaillon()
    {
        player.EquipWeapon(medaillonData);
        menuOption.SetActive(false);
    }
}