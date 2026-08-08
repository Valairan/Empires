using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;

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

    [Header("FireMode")]
    public FireModeProperties currentFiremode;
    public int firemodeindex = 0;
    private int ammoInGun;
    private int ammoInPocket;
    private bool isHoldingTrigger;
    private int burstShotsRemaining;
    private float lastShotTime;
    private float lastBurstEndTime; // Track when the last burst sequence ended
    private float FireInterval => 60f / baseitem.fireRate;
    public int currentShot = 0;
    public LayerMask raycastLayerMask;
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    public Action<int, int, int> onShoot;
    public Action<int, int, int> onReload;
    public Action<int> onSpecialToggle;

    public override void OnNetworkSpawn()
    {
        currentFiremode = baseitem.firemodes[0];
        raycastLayerMask = ~(1 << 3);
    }

    public void TriggerPressed(Vector3 aimPoint)
    {
        switch (currentFiremode.mode)
        {
            case FireMode.SemiAuto:
                TryShoot(aimPoint);
                break;

            case FireMode.FullAuto:
                isHoldingTrigger = true;
                break;

            case FireMode.Burst:
                // Only allow starting a new burst if we aren't currently bursting
                // AND enough time has passed since the last burst (burstDelay)
                if (burstShotsRemaining <= 0 && (Time.time - lastBurstEndTime >= (currentFiremode.burstDelay / 1000f)))
                {
                    burstShotsRemaining = currentFiremode.shots;
                }
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

        if (currentFiremode.mode == FireMode.FullAuto && isHoldingTrigger)
            TryShoot(aimPoint);

        if (currentFiremode.mode == FireMode.Burst && burstShotsRemaining > 0)
        {
            // Within a burst, we use FireInterval.
            if (Time.time - lastShotTime >= FireInterval)
            {
                TryShoot(aimPoint);
                burstShotsRemaining--;

                if (burstShotsRemaining <= 0)
                {
                    currentShot = 0;
                    lastBurstEndTime = Time.time; // Mark when the burst finished
                }
            }
        }
    }
    public void SwitchFiremode()
    {
        firemodeindex++;
        if (firemodeindex >= baseitem.firemodes.Length) firemodeindex = 0;
        currentFiremode = baseitem.firemodes[firemodeindex];
        onSpecialToggle.Invoke((int)currentFiremode.mode);
        burstShotsRemaining = 0;
        currentShot = 0;
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
            Debug.Log(dir);
            VfxService.Instance.RequestVfxServerByName(baseitem.muzzleflash, muzzleStartPoint.position, dir);
            VfxService.Instance.RequestVfxServerByName(baseitem.tracer, muzzleStartPoint.position, dir);
        }
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
            UnityEngine.Random.Range(-spreadAmount, spreadAmount),
            UnityEngine.Random.Range(-spreadAmount, spreadAmount),
            UnityEngine.Random.Range(-spreadAmount, spreadAmount)
        );
        return direction.normalized;
    }
    // Optional helper: CanFire for CombatController


}

