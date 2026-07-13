using System;
using UnityEngine;
using UnityEngine.VFX;
[CreateAssetMenu(menuName = "Empires/VFX Definition")]
public class VfxDefinition : ScriptableObject
{
    [SerializeField, HideInInspector] private string id;

    public string Id => id;
    public string DisplayName;
    public VisualEffect Prefab;
    public int InitialPoolSize = 16;
    public bool AutoReturnWhenFinished = true;
    public bool GrowPoolOnDemand = true;

    private void OnCreate()
    {
        UpdateDefaultDisplayName();
    }
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
#if UNITY_EDITOR
            // Marks the asset as dirty so Unity saves the newly generated GUID
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    private void UpdateDefaultDisplayName()
    {

        if (string.IsNullOrWhiteSpace(DisplayName))
        {

            DisplayName = this.name;

#if UNITY_EDITOR

            if (!UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}

