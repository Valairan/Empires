using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;

public class TestDamageable : MonoBehaviour, IDamageable
{
    [SerializeField] ParticleSystem damageParticles;
    public void takeDamage(DamageContext ctx)
    {
        Debug.Log("I got damaged");
        damageParticles.transform.position = ctx.hitpoint;
        damageParticles.transform.rotation = Quaternion.LookRotation(ctx.hitnormal);
        damageParticles.Play();
        if(TryGetComponent(out Rigidbody rb))
            rb.AddForce(- ctx.hitnormal * 3, ForceMode.Impulse);

    }
}
