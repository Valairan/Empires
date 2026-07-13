using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;

public class TestDamageable : MonoBehaviour, IDamageable
{
    [SerializeField] UnityEngine.VFX.VisualEffect damageParticles;
    [SerializeField] UnityEngine.VFX.VisualEffect holeParticles;
    [SerializeField] materialType type;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip damageSound;

    public void takeDamage(DamageContext ctx)
    {
        damageParticles.transform.position = ctx.hitpoint;
        damageParticles.transform.rotation = Quaternion.LookRotation(ctx.hitnormal);
        damageParticles.Play();
        holeParticles.transform.position = ctx.hitpoint;
        holeParticles.transform.rotation = Quaternion.LookRotation(ctx.hitnormal);
        holeParticles.Play();
        if (TryGetComponent(out Rigidbody rb))
            rb.AddForce(-ctx.hitnormal * ctx.hitforce, ForceMode.Impulse);
        source.Play();
    }
}

public enum materialType
{
    wood,
    metal,
    dirt,
    water,
}
