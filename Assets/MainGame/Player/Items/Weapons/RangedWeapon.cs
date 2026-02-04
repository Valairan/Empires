
using UnityEngine;

[CreateAssetMenu(fileName = "Ranged Weapon", menuName = "Items/Weapons/New Ranged Weapon")]
public class RangedWeapon : Weapon
{
    public Sprite ScopeTexture;
    public float ScopeZoom;

    public int firerate;
    public int magsize;
    public float accuracy;
    public int pelletCount;
}