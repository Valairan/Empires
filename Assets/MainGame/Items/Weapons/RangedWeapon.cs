
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ranged Weapon", menuName = "Items/Weapons/New Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Gun Properties")]
    public FireMode[] firemodes;
    public float fireRate;                     // Shots per minute (or per second if you prefer)
    public int magSize;                      // Magazine size
    public int pelletCount;                  // For shotguns, number of pellets per shot
    public float recoil = 1f;                // Optional: 0-1 scale, higher = more recoil
    public Sprite scopeTexture;
    public float scopeZoom;
    public float bulletSpread = 0f;  // Degrees
    public Vector2 recoilPattern;     // Optional

    protected override void Awake()
    {
        base.Awake();
        if (firemodes == null)
        {
            firemodes = new FireMode[1];
            firemodes[0] = FireMode.FullAuto;
        }
    }

}
public enum FireMode
{
    SemiAuto,
    FullAuto,
    Burst
}

[Serializable]
public struct AvailableFiremodes
{
    FireMode firemode;
}