using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class ShotgunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField, InspectorLabel("Hạt hiệu ứng bắn")] ParticleSystem shootParticleSystem;

        [SerializeField, InspectorLabel("Lớp mục tiêu bắn")] LayerMask targetLayers;
        [SerializeField, InspectorLabel("Thời gian hủy đạn")] float bulletDisableTime;

        private float attackDelay;
        private DuoFloat bulletSpeed;
        private float bulletSpreadAngle;

        private float nextShootTime;
        private float lastShootTime;

        private Pool bulletPool;

        private TweenCase shootTweenCase;
        private Vector3 shootDirection;

        private ShotgunUpgrade upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            upgrade = UpgradesController.GetUpgrade<ShotgunUpgrade>(data.UpgradeType);

            GameObject bulletObj = (upgrade.CurrentStage as BaseWeaponUpgradeStage).BulletPrefab;
            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            RecalculateDamage();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = upgrade.GetCurrentStage();

            float bonusDmgPercent = EquipmentController.Instance != null ? EquipmentController.GetTotalBonusStats().bonusDamagePercent : 0f;
            float multiplier = 1f + (bonusDmgPercent / 100f);
            damage = stage.Damage * multiplier;

            bulletSpreadAngle = stage.Spread;
            attackDelay = 1f / stage.FireRate;
            bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            AttackButtonBehavior.SetReloadFill(1 - (Time.timeSinceLevelLoad - lastShootTime) / (nextShootTime - lastShootTime));

            // Combat
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            if (nextShootTime >= Time.timeSinceLevelLoad) return;
            if (!characterBehaviour.IsAttackingAllowed) return;

            AttackButtonBehavior.SetReloadFill(0);

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                    {
                        characterBehaviour.SetTargetActive();

                        shootTweenCase.KillActive();

                        shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        shootParticleSystem.Play();

                        nextShootTime = Time.timeSinceLevelLoad + attackDelay;
                        lastShootTime = Time.timeSinceLevelLoad;

                        int bulletsNumber = upgrade.GetCurrentStage().BulletsPerShot.Random();

                        for (int i = 0; i < bulletsNumber; i++)
                        {
                            GameObject bulletObj = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(characterBehaviour.transform.eulerAngles));
                            PlayerBulletBehavior bullet = GetOrAddBulletComponent<ShotgunBulletBehavior>(bulletObj);
                            bullet.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                            bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f), 0f));
                        }

                        characterBehaviour.OnGunShooted();
                        characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                        AudioController.PlaySound(AudioController.Sounds.shotShotgun);
                    }
                }
                else
                {
                    characterBehaviour.SetTargetUnreachable();
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        private void OnDrawGizmos()
        {
            if (characterBehaviour == null)
                return;

            if (characterBehaviour.ClosestEnemyBehaviour == null)
                return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.red;

            Vector3 shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            Gizmos.DrawLine(shootPoint.position - shootDirection.normalized * 10f, characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y));

            Gizmos.color = defCol;
        }

        public override void OnGunUnloaded()
        {
            // Destroy bullets pool
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything();
        }
    }
}