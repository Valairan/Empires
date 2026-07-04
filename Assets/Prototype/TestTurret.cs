using System;
using Unity.Netcode;
using UnityEngine;

public class TestTurret : NetworkBehaviour
{
    public float lookradius = 5f;
    public float firerate = 5f;
    public float firetime = 0f;
    public Weapon damager;
    public LayerMask whattohit;
    void Update()
    {
        transform.LookAt(Physics.OverlapSphere(transform.position, lookradius, whattohit)[0].transform);
        if (firetime <= 0f)
        {
            firetime = firerate;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, lookradius, whattohit))
            {
                Debug.Log(hit.collider.gameObject.layer);
                if (hit.collider.transform.root.TryGetComponent(out IDamageable damageable))
                {
                    Debug.Log(damageable.GetType().FullName);
                    Debug.Log("Found: " + hit.collider.transform.root.name);
                    damageable.takeDamage(new DamageContext()
                    {
                        damagingPlayerID = OwnerClientId,
                        damage = calculateDamage(hit.transform),
                        hitpoint = hit.point,
                        hitnormal = hit.normal,
                        hitforce = 2f,
                        detectedLayer = hit.transform.gameObject.layer
                    });
                    Debug.DrawRay(transform.position, transform.forward * 20, Color.red, 1f);
                    Debug.Log(hit.transform.gameObject.layer + "<---");
                }
            }
        }
        firetime -= Time.deltaTime;

    }

    public float calculateDamage(Transform victim)
    {
        switch (victim.transform.gameObject.layer)
        {
            case int layer when layer == LayerMask.NameToLayer("Head"): return damager.headDamage;
            case int layer when layer == LayerMask.NameToLayer("Torso"): return damager.bodyDamage;
            case int layer when layer == LayerMask.NameToLayer("Legs"): return damager.legDamage;
        }
        if (victim.root.TryGetComponent(out ItemBehaviour<Machine> machine)) { return damager.machineDamage; }
        if (victim.root.TryGetComponent(out TreeResourceBehaviour tree)) { return damager.treeDamage; }
        if (victim.root.TryGetComponent(out TreeResourceBehaviour ore)) { return damager.oreDamage; }
        Debug.Log("The calculated damage is here: " + victim.transform.gameObject.layer);
        return 5f;
    }
}
