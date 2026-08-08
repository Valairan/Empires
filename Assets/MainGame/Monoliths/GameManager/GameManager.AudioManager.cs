using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public partial class GameManager : NetworkBehaviour, IAudioService
{
    [Header("Audio Manager\n------------------------------")]
    [SerializeField] public List<AudioClip> AudioDefinitions;

    private readonly Dictionary<string, AudioClip> _AudioDefinitions = new();
    private readonly Dictionary<string, Queue<AudioClip>> _AudioPools = new();

    // NEW: Fast string name translator ("TreeHit" -> "guid-12345-6789")
    private readonly Dictionary<string, string> _ClipNameToIdLookup = new(System.StringComparer.OrdinalIgnoreCase);



    public void RequestAidioClipOnServerByName(string ClipName, Vector3 position, Vector3 rotation)
    {
        throw new System.NotImplementedException();
    }

    public void PlayAudioClipLocalByName(string ClipName, Vector3 position, Vector3 rotation)
    {
        throw new System.NotImplementedException();
    }

    public void RequestAudioClipOnServer(string ClipId, Vector3 position, Vector3 rotation)
    {
        throw new System.NotImplementedException();
    }

    public void PlayAudioClipLocal(string ClipId, Vector3 position, Vector3 rotation)
    {
        throw new System.NotImplementedException();
    }
}