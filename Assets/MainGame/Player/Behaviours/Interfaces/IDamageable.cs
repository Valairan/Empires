
using UnityEngine;

public interface IDamageable
{
    public void takeDamage(DamageContext ctx);
}

public struct DamageContext
{
    public Weapon damager;
    public Vector3 hitpoint;
    public Vector3 hitnormal;
    public ulong damagingPlayerID;
}