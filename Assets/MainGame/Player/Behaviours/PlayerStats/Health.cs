using System;
using Unity.Netcode;
using UnityEngine;

public class Health : RangeStat
{
    public Action die;

    // Make health a NetworkVariable to sync it
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f);

    public float maxHealth = 100f;

    public override void init()
    {
    }

    public void ReduceHealth(float delta)
    {
        if (!IsServer) return; // Only server should change health
        currentHealth.Value -= delta;

        if (currentHealth.Value <= 0f)
        {
            die?.Invoke();
        }
    }

    public void Heal(float delta)
    {
        if (!IsServer) return; // Only server should change health
        currentHealth.Value = Mathf.Min(currentHealth.Value + delta, maxHealth);
    }
}

public class RangeStat : NetworkBehaviour
{
    public float amount;
    public virtual void init()
    {

    }
}