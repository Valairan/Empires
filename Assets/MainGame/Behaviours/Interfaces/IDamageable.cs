
using Unity.Netcode;
using UnityEngine;

public interface IDamageable
{
    public void takeDamage(DamageContext ctx);
}

public struct DamageContext : INetworkSerializable
{
    public ulong damagingPlayerID;
    public Vector3 hitpoint;
    public Vector3 hitnormal;
    public WeaponType type;
    public float damage;
    public float hitforce;
    public int detectedLayer;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize primitive types and Unity structs
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref damagingPlayerID);
        serializer.SerializeValue(ref hitpoint);
        serializer.SerializeValue(ref hitnormal);
        serializer.SerializeValue(ref hitforce);
        serializer.SerializeValue(ref detectedLayer);

        // Handle the Weapon field
        // serializer.SerializeValue(ref damager); 
    }
}