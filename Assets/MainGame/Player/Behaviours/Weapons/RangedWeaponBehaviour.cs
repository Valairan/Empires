using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;
using System.Linq;
using Unity.Collections;

public class RangedWeaponBehaviour
    : WeaponBehaviour<RangedWeapon>, IWeaponTriggerable, IWeaponUpdatable
{

    public RangedWeapon baseitem
    {
        get => (RangedWeapon)base.baseitem;
        set => base.baseitem = value;
    }
    [Header("References")]
    public Transform muzzleStartPoint;
    public VisualEffect muzzleFlash;
    public VisualEffect[] tracer;

    [Header("FireMode")]
    public FireMode currentFiremode;
    public int firemodeindex = 0;
    private bool isHoldingTrigger;
    private int burstShotsRemaining;
    private float lastShotTime;
    private float FireInterval => 60f / baseitem.fireRate;
    public int currentShot = 0;
    public LayerMask raycastLayerMask;
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        currentFiremode = baseitem.firemodes[0];
        raycastLayerMask = ~(1 << 3);
    }

    public void TriggerPressed(Vector3 aimPoint)
    {
        switch (currentFiremode)
        {
            case FireMode.SemiAuto:
                TryShoot(aimPoint);
                break;

            case FireMode.FullAuto:
                isHoldingTrigger = true;
                break;

            case FireMode.Burst:
                burstShotsRemaining = 3; // configurable per weapon
                break;
        }
    }


    public void TriggerReleased()
    {
        isHoldingTrigger = false;
        currentShot = 0;
    }

    public bool CanFire()
    {
        return Time.time - lastShotTime >= FireInterval;
    }

    public void UpdateWeapon(Vector3 aimPoint)
    {

        if (currentFiremode == FireMode.FullAuto && isHoldingTrigger)
            TryShoot(aimPoint);

        if (currentFiremode == FireMode.Burst && burstShotsRemaining > 0)
        {
            if (Time.time - lastShotTime >= FireInterval)
            {
                burstShotsRemaining--;
                TryShoot(aimPoint);
            }
        }
    }
    public void SwitchFiremode()
    {
        firemodeindex++;
        if (firemodeindex >= baseitem.firemodes.Length) firemodeindex = 0;
        currentFiremode = baseitem.firemodes[firemodeindex];
    }
    public void Reload()
    {

    }
    private void TryShoot(Vector3 aimPoint)
    {
        if (Time.time - lastShotTime < FireInterval) return;

        lastShotTime = Time.time;
        Attack_ServerRpc(aimPoint);
        onAttack?.Invoke();
    }

    [ServerRpc]
    public override void Attack_ServerRpc(Vector3 point)
    {
        Vector3 dir;
        NativeArray<Vector3> tracers = new NativeArray<Vector3>(TypedItem.pelletCount, Allocator.TempJob);
        for (int i = 0; i < TypedItem.pelletCount; i++)
        {

            dir = (point - muzzleStartPoint.position).normalized;
            dir = ApplySpread(dir, TypedItem.bulletSpread);

            if (Physics.Raycast(muzzleStartPoint.position, dir, out RaycastHit hit, Mathf.Infinity, raycastLayerMask))
            {
                if (hit.transform.TryGetComponent(out IDamageable damageable))
                {
                    DamageContext ctx = new DamageContext
                    {
                        damagingPlayerID = OwnerClientId,
                        damage = calculateDamage(hit.transform),
                        hitpoint = hit.point,
                        hitnormal = hit.normal,
                        hitforce = baseitem.shellsize,
                        detectedLayer = hit.transform.gameObject.layer
                    };
                    damageable.takeDamage(ctx);
                }
            }
            currentShot++;
            tracers[i] = dir;
        }
        PlayMuzzleFlash_ClientRpc(tracers);

    }

    public float calculateDamage(Transform victim)
    {
        switch (victim.transform.gameObject.layer)
        {
            case int layer when layer == LayerMask.NameToLayer("Head"): return baseitem.headDamage;
            case int layer when layer == LayerMask.NameToLayer("Torso"): return baseitem.bodyDamage;
            case int layer when layer == LayerMask.NameToLayer("Legs"): return baseitem.legDamage;
        }
        if (victim.root.TryGetComponent(out ItemBehaviour<Machine> machine)) { return baseitem.machineDamage; }
        if (victim.root.TryGetComponent(out TreeResourceBehaviour tree)) { return baseitem.treeDamage; }
        if (victim.root.TryGetComponent(out TreeResourceBehaviour ore)) { return baseitem.oreDamage; }
        Debug.Log("The calculated damage is here: " + victim.transform.gameObject.layer);
        return 5f;
    }

    private Vector3 ApplySpread(Vector3 direction, float accuracy)
    {

        float spreadAmount = (1f - accuracy) * Mathf.Clamp(currentShot, 0, 5f);
        direction += new Vector3(
            Random.Range(-spreadAmount, spreadAmount),
            Random.Range(-spreadAmount, spreadAmount),
            Random.Range(-spreadAmount, spreadAmount)
        );
        return direction.normalized;
    }

    [ClientRpc]
    void PlayMuzzleFlash_ClientRpc(NativeArray<Vector3> tracerTo)
    {
        int index = 0;
        foreach (Vector3 dir in tracerTo)
        {
            tracer[index].transform.rotation = Quaternion.LookRotation(dir);
            tracer[index]?.SendEvent("OnPlay");
            index++;
        }
        muzzleFlash?.Play();
        audioSource?.Play();

    }


    // Optional helper: CanFire for CombatController


}
