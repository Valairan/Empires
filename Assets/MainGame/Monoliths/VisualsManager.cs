using System.Collections.Generic;
using Empires.VFX;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class VisualsManager : NetworkBehaviour
{
    public static VisualsManager Singleton;

    [SerializeField] private List<VfxDefinition> effectDefinitions = new List<VfxDefinition>();

    private readonly Dictionary<string, VfxPool> pools = new Dictionary<string, VfxPool>(System.StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else if (Singleton != this)
        {
            enabled = false;
            return;
        }

        InitializePools();
    }

    public override void OnNetworkSpawn()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        pools.Clear();

        for (int i = 0; i < effectDefinitions.Count; i++)
        {
            VfxDefinition definition = effectDefinitions[i];
            if (definition == null || definition.Prefab == null || string.IsNullOrWhiteSpace(definition.Id))
            {
                continue;
            }

            pools[definition.Id] = new VfxPool(this, definition);
        }
    }

    private void Update()
    {
        foreach (VfxPool pool in pools.Values)
        {
            pool.Update();
        }
    }

    public bool PlayEffect(string effectId, Vector3 position, Quaternion rotation = default, Transform parent = null, float autoReturnAfterSeconds = -1f)
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            Debug.LogWarning("VisualsManager.PlayEffect called without a valid effect id.");
            return false;
        }

        if (!pools.TryGetValue(effectId, out VfxPool pool))
        {
            Debug.LogWarning($"VisualsManager does not have a pooled VFX entry for '{effectId}'.");
            return false;
        }

        return pool.Play(position, rotation == default ? Quaternion.identity : rotation, parent, autoReturnAfterSeconds);
    }

    [ServerRpc]
    public void RequestPlayEffect_ServerRpc(string effectId, Vector3 position, Quaternion rotation)
    {
        PlayEffect_ClientRpc(effectId, position, rotation);
    }

    [ClientRpc]
    public void PlayEffect_ClientRpc(string effectId, Vector3 position, Quaternion rotation)
    {
        PlayEffect(effectId, position, rotation);
    }

    private sealed class VfxPool
    {
        private readonly VisualsManager owner;
        private readonly VfxDefinition definition;
        private readonly Queue<PooledVisualEffect> available = new Queue<PooledVisualEffect>();
        private readonly List<PooledVisualEffect> active = new List<PooledVisualEffect>();

        public VfxPool(VisualsManager owner, VfxDefinition definition)
        {
            this.owner = owner;
            this.definition = definition;

            for (int i = 0; i < definition.InitialPoolSize; i++)
            {
                CreateInstance();
            }
        }

        public bool Play(Vector3 position, Quaternion rotation, Transform parent, float autoReturnAfterSeconds)
        {
            if (available.Count == 0)
            {
                if (!definition.GrowPoolOnDemand)
                {
                    Debug.LogWarning($"VFX pool exhausted for '{definition.Id}'.");
                    return false;
                }

                CreateInstance();
            }

            PooledVisualEffect pooledEffect = available.Dequeue();
            pooledEffect.Play(position, rotation, parent ?? owner.transform, autoReturnAfterSeconds);
            active.Add(pooledEffect);
            return true;
        }

        public void Update()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                PooledVisualEffect pooledEffect = active[i];
                if (pooledEffect.IsFinished(Time.deltaTime, definition.AutoReturnWhenFinished))
                {
                    pooledEffect.Reset();
                    available.Enqueue(pooledEffect);
                    active.RemoveAt(i);
                }
            }
        }

        private void CreateInstance()
        {
            VisualEffect instance = Instantiate(definition.Prefab, owner.transform);
            instance.Stop();
            instance.gameObject.SetActive(false);
            available.Enqueue(new PooledVisualEffect(instance));
        }
    }

    private sealed class PooledVisualEffect
    {
        private readonly VisualEffect visualEffect;
        private readonly Transform originalParent;
        private float remainingAutoReturnTime = -1f;
        private bool isActive;

        public PooledVisualEffect(VisualEffect visualEffect)
        {
            this.visualEffect = visualEffect;
            originalParent = visualEffect.transform.parent;
        }

        public void Play(Vector3 position, Quaternion rotation, Transform parent, float autoReturnAfterSeconds)
        {
            visualEffect.transform.SetParent(parent, worldPositionStays: true);
            visualEffect.transform.SetPositionAndRotation(position, rotation);
            visualEffect.gameObject.SetActive(true);
            visualEffect.Play();
            isActive = true;
            remainingAutoReturnTime = autoReturnAfterSeconds >= 0f ? autoReturnAfterSeconds : -1f;
        }

        public bool IsFinished(float deltaTime, bool autoReturnWhenFinished)
        {
            if (!isActive)
            {
                return true;
            }

            if (remainingAutoReturnTime >= 0f)
            {
                remainingAutoReturnTime -= deltaTime;
                if (remainingAutoReturnTime <= 0f)
                {
                    return true;
                }
            }

            if (!autoReturnWhenFinished)
            {
                return false;
            }

            return visualEffect.aliveParticleCount <= 0;
        }

        public void Reset()
        {
            visualEffect.Stop();
            visualEffect.gameObject.SetActive(false);
            visualEffect.transform.SetParent(originalParent, worldPositionStays: true);
            visualEffect.transform.localPosition = Vector3.zero;
            visualEffect.transform.localRotation = Quaternion.identity;
            visualEffect.transform.localScale = Vector3.one;
            isActive = false;
            remainingAutoReturnTime = -1f;
        }
    }
}