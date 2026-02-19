using UnityEngine;

public class RagdollController : MonoBehaviour, IDamageable
{
    public GameObject graphicsRoot; // assign Graphics in inspector
    public ParticleSystem damageParticles;

    private Rigidbody[] ragdollBodies;
    private Animator animator;
    private Collider mainCollider;

    private bool isRagdolled;

    void Awake()
    {
        ragdollBodies = graphicsRoot.GetComponentsInChildren<Rigidbody>();
        animator = graphicsRoot.GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();

        SetRagdoll(false);
    }

    public void Die()
    {
        animator.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        SetRagdoll(true);
        isRagdolled = true;
    }

    void SetRagdoll(bool enabled)
    {
        foreach (var rb in ragdollBodies)
            rb.isKinematic = !enabled;
    }

    public void takeDamage(DamageContext ctx)
    {
        Die();
        damageParticles.transform.position = ctx.hitpoint;
        damageParticles.transform.rotation = Quaternion.LookRotation(ctx.hitnormal);
        damageParticles.Play();

        ApplyForce(ctx);
    }

    void ApplyForce(DamageContext ctx)
    {
        if (!isRagdolled)
            return;

        Rigidbody closest = GetClosestBody(ctx.hitpoint);

        Vector3 force = -ctx.hitnormal * 25f; // Increase this if needed

        closest.AddForceAtPosition(force, ctx.hitpoint, ForceMode.Impulse);
    }

    Rigidbody GetClosestBody(Vector3 point)
    {
        Rigidbody closest = ragdollBodies[0];
        float minDist = Vector3.Distance(point, closest.worldCenterOfMass);

        foreach (var rb in ragdollBodies)
        {
            float dist = Vector3.Distance(point, rb.worldCenterOfMass);
            if (dist < minDist)
            {
                minDist = dist;
                closest = rb;
            }
        }

        return closest;
    }
}
