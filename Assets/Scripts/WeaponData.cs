using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Niko/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damageMultiplier = 1f;
    public float projectileSpeed = 10f;
    public float fireRate = 0.2f;
    public int projectileCount = 1;
    public float spreadAngle = 15f;
    public bool hasHoming = false;
    public float homingStrength = 2f;

    [Header("Animation de la balle")]
    public RuntimeAnimatorController bulletAnimator; // <-- Mets ton animation de plume/dé/médaillon ICI
    public Color bulletColor = Color.white;
}