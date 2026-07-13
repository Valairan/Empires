using UnityEngine;
public interface IVfxService
{
    // Request via unique string names ("TreeHit", "MuzzleFlash")
    void RequestVfxServerByName(string vfxName, Vector3 position, Quaternion rotation);
    void PlayVfxLocalByName(string vfxName, Vector3 position, Quaternion rotation);

    // Keep these intact for asset-to-asset direct lookups
    void RequestVfxServer(string vfxId, Vector3 position, Quaternion rotation);
    void PlayVfxLocal(string vfxId, Vector3 position, Quaternion rotation);
}

public static class VfxService
{
    public static IVfxService Instance { get; set; }
}