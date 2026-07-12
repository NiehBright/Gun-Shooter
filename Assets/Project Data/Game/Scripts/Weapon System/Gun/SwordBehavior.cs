using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class SwordBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField, InspectorLabel("Hiệu ứng chém lưỡi kiếm")] ParticleSystem slashTrailVFX;

        private Pool slashPool;
        private float nextAttackTime;
        private float attackDelay;
        private DuoFloat slashSpeed;
        private float slashLifetime = 0.35f;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            var upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(data.UpgradeType);
            var stage = upgrade.GetCurrentStage();

            GameObject slashPrefab = stage.BulletPrefab;
            slashPool = new Pool(new PoolSettings(slashPrefab.name, slashPrefab, 5, true));

            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(data.UpgradeType);
            var stage = upgrade.GetCurrentStage();
            damage = stage.Damage;

            attackDelay = 1f / stage.FireRate;
            slashSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (!characterBehaviour.IsCloseEnemyFound || !characterBehaviour.IsAttackingAllowed) 
                return;

            if (nextAttackTime >= Time.timeSinceLevelLoad) return;

            nextAttackTime = Time.timeSinceLevelLoad + attackDelay;

            if (slashTrailVFX != null) slashTrailVFX.Play();

            GameObject slashObj = slashPool.GetPooledObject(new PooledObjectSettings()
                .SetPosition(shootPoint.position)
                .SetEulerRotation(characterBehaviour.transform.eulerAngles));

            PlayerBulletBehavior slashWave = GetOrAddBulletComponent<MinigunBulletBehavior>(slashObj);
            
            slashWave.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, 
                                 slashSpeed.Random(), 
                                 characterBehaviour.ClosestEnemyBehaviour, 
                                 slashLifetime);

            characterBehaviour.OnGunShooted();
            
            AudioController.PlaySound(AudioController.Sounds.shotShotgun, 0.6f); 
        }

        public override void OnGunUnloaded()
        {
            if (slashPool != null)
            {
                slashPool.Clear();
                slashPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.SwordHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            slashPool?.ReturnToPoolEverything();
        }
    }
}
