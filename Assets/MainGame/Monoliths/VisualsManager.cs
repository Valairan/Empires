// Inside monoliths.asmdef (References: worldgen, behaviours)
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class VisualsManager : NetworkBehaviour, IVfxService
{
    [SerializeField] private List<VfxDefinition> vfxPoolDefinitions;

    private readonly Dictionary<string, VfxDefinition> _definitions = new();
    private readonly Dictionary<string, Queue<VisualEffect>> _pools = new();

    // NEW: Fast string name translator ("TreeHit" -> "guid-12345-6789")
    private readonly Dictionary<string, string> _nameToIdLookup = new(System.StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        VfxService.Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var def in vfxPoolDefinitions)
        {
            if (def == null || string.IsNullOrEmpty(def.Id)) continue;

            _definitions[def.Id] = def;
            _pools[def.Id] = new Queue<VisualEffect>();

            // Map BOTH the asset's file name AND its DisplayName for safety
            _nameToIdLookup[def.name] = def.Id;
            if (!string.IsNullOrWhiteSpace(def.DisplayName))
            {
                _nameToIdLookup[def.DisplayName] = def.Id;
            }

            for (int i = 0; i < def.InitialPoolSize; i++)
            {
                CreateNewInstanceInPool(def);
            }
        }
    }

    public void RequestVfxServerByName(string vfxName, Vector3 position, Quaternion rotation)
    {
        if (_nameToIdLookup.TryGetValue(vfxName, out string id))
        {
            RequestVfxServer(id, position, rotation);
        }
        else
        {
            Debug.LogWarning($"[VFX] Request failed: No registered VFX name matches '{vfxName}'");
        }
    }

    public void PlayVfxLocalByName(string vfxName, Vector3 position, Quaternion rotation)
    {
        if (_nameToIdLookup.TryGetValue(vfxName, out string id))
        {
            PlayVfxLocal(id, position, rotation);
        }
    }


    public void RequestVfxServer(string vfxId, Vector3 position, Quaternion rotation)
    {
        PlayVfx_ServerRpc(vfxId, position, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayVfx_ServerRpc(string vfxId, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return;
        PlayVfx_ClientRpc(vfxId, position, rotation);
    }

    [ClientRpc]
    private void PlayVfx_ClientRpc(string vfxId, Vector3 position, Quaternion rotation)
    {
        PlayVfxLocal(vfxId, position, rotation);
    }

    public void PlayVfxLocal(string vfxId, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(vfxId)) return;

        var pool = _pools[vfxId];
        var def = _definitions[vfxId];

        if (pool.Count == 0 && def.GrowPoolOnDemand)
        {
            CreateNewInstanceInPool(def);
        }

        if (pool.Count > 0)
        {
            VisualEffect effect = pool.Dequeue();
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.gameObject.SetActive(true);
            effect.Play();

            if (def.AutoReturnWhenFinished)
            {
                StartCoroutine(ReturnToPoolRoutine(vfxId, effect, 2f));
            }
        }
    }

    private VisualEffect CreateNewInstanceInPool(VfxDefinition def)
    {
        VisualEffect effect = Instantiate(def.Prefab, transform);
        effect.gameObject.SetActive(false);
        _pools[def.Id].Enqueue(effect);
        return effect;
    }

    private System.Collections.IEnumerator ReturnToPoolRoutine(string id, VisualEffect effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Stop();
        effect.gameObject.SetActive(false);
        _pools[id].Enqueue(effect);
    }
}
