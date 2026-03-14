using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class HospitalBehaviour : MachineBehaviour
{
    public float InteractionDuration => 1f;
    public float healPerSecond = 10f;

    private readonly List<Health> playersInRange = new List<Health>();

    private void OnTriggerEnter(Collider other)
    {
        Health h = other.GetComponent<Health>();
        Debug.Log(other.gameObject.name);
        if (h != null && !playersInRange.Contains(h))
        {
            playersInRange.Add(h);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Health h = other.GetComponent<Health>();
        if (h != null)
        {
            playersInRange.Remove(h);
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Only server should modify health
        if (state == MachineState.preview) return;
        foreach (var player in playersInRange)
        {
            player.Heal(healPerSecond * Time.deltaTime);
        }
    }
}