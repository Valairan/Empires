using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Empires.VFX
{
    [CreateAssetMenu(menuName = "Empires/VFX Definition")]
    public class VfxDefinition : ScriptableObject
    {
    [SerializeField, HideInInspector] private string id;

    public string Id => id;

    public VisualEffect Prefab;
    public int InitialPoolSize = 16;
    public bool AutoReturnWhenFinished = true;
    public bool GrowPoolOnDemand = true;

    private void OnEnable()
    {
        EnsureId();
    }

    private void Reset()
    {
        EnsureId();
    }

        private void EnsureId()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("D");
            }
        }
    }
}
