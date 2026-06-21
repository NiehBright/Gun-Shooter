using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    using Enemy.BossSniper;

    public class BossSniperBehavior : BaseEnemyBehavior
    {
        [Header("Bullet")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;

        [SerializeField] Transform shootingPoint;
        [SerializeField] ParticleSystem gunFireParticle;
        
        [SerializeField] LayerMask collisionLayer;

        [Header("Laser")]
        [SerializeField] List<MeshRenderer> laserRenderers;
        private List<BossSniperLaserLine> lasers;

        [Space]
        [SerializeField] float yellowAimingDuration;
        public float YellowLaserAinimgDuration => yellowAimingDuration;

        [SerializeField] float redAimingDuration;
        public float RedLaserAimingDuration => redAimingDuration;

        [Space]
        [SerializeField] bool canAimDuringRedLaserStage;
        public bool CanAimDuringRedLaserStage => canAimDuringRedLaserStage;

        [Space]
        [SerializeField] float laserThickness;

        [Space]
        [SerializeField] Color yellowLaserColor;
        [SerializeField] Color redLaserColor;

        [Header("Other")]
        [SerializeField] GameObject auraParticle;

        private static Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            if(bulletPool == null) bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 4, true));

            lasers = new List<BossSniperLaserLine>();
            for (int i = 0; i < laserRenderers.Count; i++) 
            {
                var laser = new BossSniperLaserLine();
                laser.Init(laserRenderers[i]);

                lasers.Add(laser);
            }
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            healthbarBehaviour.FollowUpdate();
        }

        public override void Initialise()
        {
            base.Initialise();

            auraParticle.SetActive(true);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            auraParticle.SetActive(false);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(shootingPoint.position).SetEulerRotation(shootingPoint.eulerAngles)).GetComponent<BossSniperBulletBehavior>();
                    bullet.transform.forward = transform.forward;
                    bullet.InitialiseBullet(GetCurrentDamage(), bulletSpeed, 1000, lasetHitPoints);

                    gunFireParticle.Play();
                    AudioController.PlaySound(AudioController.Sounds.enemySniperShoot);

                    break;

                case EnemyCallbackType.HitFinish:

                    InvokeOnAttackFinished();
                    break;
            }
        }

        #region Aiming

        private List<Vector3> lasetHitPoints;

        public void MakeLaserYellow()
        {
            lasers.ForEach((laser) => laser.SetColor(yellowLaserColor));
        }

        public void MakeLaserRed()
        {
            lasers.ForEach((laser) => laser.SetColor(redLaserColor));
        }

        public void EnableLaser()
        {
            lasers.ForEach((laser) => laser.SetActive(true));
        }

        public void DisableLaser()
        {
            lasers.ForEach((laser) => laser.SetActive(false));
        }

        public void AimLaser()
        {
            var laserStartPos = shootingPoint.position;
            var laserMovementDirection = Rotation * Vector3.forward;

            lasetHitPoints = new List<Vector3>();

            for (int i = 0; i < lasers.Count; i++)
            {
                var laserObject = lasers[i];

                laserObject.SetActive(true);

                var shouldEndCalculations = false;
                if (Physics.Raycast(laserStartPos, laserMovementDirection, out var collisionHitInfo, 300f, collisionLayer))
                {
                    var distanceToCollisionPoint = Vector3.Distance(collisionHitInfo.point, laserStartPos);

                    laserObject.Initialise(
                        startPos: laserStartPos, 
                        hitPos: collisionHitInfo.point, 
                        scale: new Vector3(laserThickness, laserThickness, distanceToCollisionPoint));

                    laserStartPos = collisionHitInfo.point - laserMovementDirection * 0.2f;

                    var prevBulletMovementDirection = laserMovementDirection;

                    laserMovementDirection = Vector3.Reflect(laserMovementDirection, -collisionHitInfo.normal);

                    // Dot Products allows to evaluate the angle of ricochet
                    var dotProduct = Vector3.Dot(prevBulletMovementDirection, laserMovementDirection);

                    // Trimming laser if the ricochet angle is too pointy
                    if(Mathf.Abs(dotProduct) > 0.96f && i > 0)
                    {
                        shouldEndCalculations = true;
                        laserObject.SetActive(false);
                    } else
                    {
                        lasetHitPoints.Add(laserStartPos);

                        if (collisionHitInfo.collider.gameObject == Target.gameObject)
                            shouldEndCalculations = true;
                    }
                }
                else
                {
                    var laserEndPoint = laserStartPos + laserMovementDirection * 300;

                    lasetHitPoints.Add(laserEndPoint);

                    var distanceToEndPoint = Vector3.Distance(collisionHitInfo.point, laserStartPos);

                    laserObject.Initialise(
                        startPos: laserStartPos,
                        hitPos: laserEndPoint,
                        scale: new Vector3(laserThickness, laserThickness, distanceToEndPoint));

                    shouldEndCalculations = true;
                }

                if (shouldEndCalculations)
                {
                    for (int j = i + 1; j < lasers.Count; j++)
                    {
                        var laserToDisable = lasers[j];

                        laserToDisable.SetActive(false);
                    }

                    break;
                }
            }
        }

        #endregion
    }
}