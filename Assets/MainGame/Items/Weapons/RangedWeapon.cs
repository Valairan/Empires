
using UnityEngine;

[CreateAssetMenu(fileName = "Ranged Weapon", menuName = "Items/Weapons/New Ranged Weapon")]
public class RangedWeapon : Weapon
{
    public Sprite scopeTexture;
    public float scopeZoom;
    public float bulletSpread = 0f;  // Degrees
    public Vector2 recoilPattern;     // Optional

    new void Awake()
    {

    }

}
