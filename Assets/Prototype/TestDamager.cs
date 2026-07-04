using UnityEngine;

public class TestDamager : MonoBehaviour
{   
    public Weapon weapon;
    void OnTriggerEnter(Collider col)
    {
        if (col.transform.TryGetComponent(out IDamageable victim))
        {
            DamageContext ctx = new DamageContext()
            {
                detectedLayer = col.gameObject.layer
            };
            
            victim.takeDamage(ctx);
        }
    }
}
