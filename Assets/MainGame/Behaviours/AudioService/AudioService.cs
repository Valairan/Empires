using UnityEngine;
public interface IAudioService
{
    // Request via unique string names ("TreeHit", "MuzzleFlash")
    void RequestAidioClipOnServerByName(string ClipName, Vector3 position, Vector3 rotation);
    void PlayAudioClipLocalByName(string ClipName, Vector3 position, Vector3 rotation);

    // Keep these intact for asset-to-asset direct lookups
    void RequestAudioClipOnServer(string ClipId, Vector3 position, Vector3 rotation);
    void PlayAudioClipLocal(string ClipId, Vector3 position, Vector3 rotation);
}

public static class AudioService
{
    public static IAudioService Instance { get; set; }
}