using UnityEngine;

[CreateAssetMenu(fileName = "Throwable Weapon", menuName = "Items/Weapons/New Throwable Weapon")]
public class ThrowableWeapon : Weapon
{
    void Awake()
    {
        isThrowable = true;          // Mark as throwable
        hasAreaEffect = true;        // Explosives usually have AoE
        isAutomatic = false;         // Throwables are single-use
        magSize = 1;                 // Usually one per throw
        fireRate = 0;                // Not automatic
        pelletCount = 1;             // For AoE logic if needed
    }
}
