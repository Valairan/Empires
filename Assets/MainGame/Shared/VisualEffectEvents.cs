using System;
using UnityEngine;

public readonly struct VisualEffectRequest
{
    public VisualEffectRequest(string effectName, Vector3 position, Quaternion rotation, Transform parent = null, float autoReturnAfterSeconds = -1f)
    {
        EffectName = effectName;
        Position = position;
        Rotation = rotation;
        Parent = parent;
        AutoReturnAfterSeconds = autoReturnAfterSeconds;
    }

    public string EffectName { get; }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    public Transform Parent { get; }
    public float AutoReturnAfterSeconds { get; }
}

public static class VisualEffectEvents
{
    public static event Action<VisualEffectRequest> EffectRequested;

    public static void RaiseEffectRequested(VisualEffectRequest request)
    {
        EffectRequested?.Invoke(request);
    }
}
