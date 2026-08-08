
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ranged Weapon", menuName = "Empires/Weapons/New Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Gun Properties")]
    public FireModeProperties[] firemodes;
    public float shellsize;                      // Magazine size
    public float fireRate;                     // Shots per minute (or per second if you prefer)
    public int magSize;                      // Magazine size
    public int pelletCount;                  // For shotguns, number of pellets per shot
    public bool canReload;                  // For shotguns, number of pellets per shot
    public float reloadTime;                  // For shotguns, number of pellets per shot
    public Sprite scopeTexture;
    public float scopeZoom;
    public AudioClip shotSound;
    public AudioClip reloadSound;
    [Header("Recoil")]
    public float recoil = 1f;                // Optional: 0-1 scale, higher = more recoil
    public Vector2 recoilPattern;     // Optional
    public float maxRecoilPitch = 20f;
    public float maxRecoilYaw = 10f;
    public bool returnToCenter = false;
    public float recoilRecovery = 8f;
    public float bulletSpread = 0f;  // Degrees
    [Header("VFX")]
    public string muzzleflash;
    public string tracer;

    protected void OnCreate()
    {
        if (firemodes == null)
        {
            firemodes = new FireModeProperties[1];
            firemodes[0].mode = FireMode.FullAuto;
        }
    }
}
public enum FireMode
{
    FullAuto,
    SemiAuto,
    Burst
}

[Serializable]
public struct FireModeProperties
{
    public FireMode mode;
    public int shots;

    public float burstDelay;
}

[Serializable]
public struct AvailableFiremodes
{
    FireMode firemode;
}