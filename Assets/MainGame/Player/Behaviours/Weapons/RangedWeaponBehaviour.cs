    using UnityEngine;
    using Unity.Netcode;
    using UnityEngine.VFX;
    using UnityEngine.Audio;

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

        [Header("FireMode")]
        public FireMode currentFiremode;
        public int firemodeindex = 0;
        private bool isHoldingTrigger;
        private int burstShotsRemaining;
        private float lastShotTime;
        private float FireInterval => 60f / baseitem.fireRate;
        public int currentShot = 0;

        [Header("Audio")]
        [SerializeField] AudioSource audioSource;

        public override void OnNetworkSpawn()
        {
            currentFiremode = baseitem.firemodes[0];
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
            currentShot++;

        }

        [ServerRpc]
        public override void Attack_ServerRpc(Vector3 point)
        {
            for (int i = 0; i < TypedItem.pelletCount; i++)
            {
                PlayMuzzleFlash_ClientRpc();

                Vector3 dir = (point - muzzleStartPoint.position).normalized;
                dir = ApplySpread(dir, TypedItem.bulletSpread);

                if (Physics.Raycast(muzzleStartPoint.position, dir, out RaycastHit hit))
                {
                    if (hit.transform.TryGetComponent(out IDamageable damageable))
                    {
                        DamageContext ctx = new DamageContext
                        {
                            damagingPlayerID = OwnerClientId,
                            hitpoint = hit.point,
                            hitnormal = hit.normal,
                            hitforce = baseitem.shellsize,
                            damager = TypedItem
                        };

                        damageable.takeDamage(ctx);
                    }
                }
            }

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
        void PlayMuzzleFlash_ClientRpc()
        {
            muzzleFlash.Play();
            audioSource.Play();
        }

        // Optional helper: CanFire for CombatController


    }
