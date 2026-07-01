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
        transform.LookAt(Physics.OverlapSphere(transform.position, lookradius)[0].transform);
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
                        damager = damager,
                    });
                }
            }
        }
        firetime -= Time.deltaTime;

    }
}
