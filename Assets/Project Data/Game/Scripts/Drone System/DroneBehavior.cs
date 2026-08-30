using UnityEngine;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class DroneBehavior : MonoBehaviour
    {
        [SerializeField] Transform shootPoint;
        [SerializeField] GameObject muzzleParticlePrefab;
        [SerializeField] ParticleSystem trailParticle; // Optional trail

        [Header("Exhaust VFX (kéo 2 cái FX_Drone_Exhaust vào đây)")]
        [SerializeField] ParticleSystem exhaustVFX1;
        [SerializeField] ParticleSystem exhaustVFX2;

        private CharacterBehaviour player;
        private BaseDroneUpgradeStage currentStage;
        
        private float lastShootTime;
        private BaseEnemyBehavior currentEnemyTarget;

        private Vector3 currentVelocity;
        [SerializeField] float smoothTime = 0.2f;
        [SerializeField] float followRadius = 3f; // Max distance from player

        private bool exhaustPlaying = false;
        
        private Pool bulletPool;
        private Pool muzzlePool;

        public void Initialise(CharacterBehaviour player, BaseDroneUpgradeStage stage)
        {
            this.player = player;
            this.currentStage = stage;
            
            // Teleport drone to correct position immediately
            transform.position = GetTargetPosition(null);
            
            // Set ALL children to Default layer (0) so gameplay camera renders everything
            SetLayerRecursive(gameObject, 0);
            
            // Force enable ALL renderers (MeshRenderer, SkinnedMeshRenderer, ParticleSystemRenderer)
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                r.enabled = true;
                r.gameObject.SetActive(true);
            }
            
            // Auto-find exhaust VFX by name if not assigned in Inspector
            if (exhaustVFX1 == null || exhaustVFX2 == null)
            {
                ParticleSystem[] allPS = GetComponentsInChildren<ParticleSystem>(true);
                int exhaustIndex = 0;
                foreach (var ps in allPS)
                {
                    if (ps.gameObject.name.Contains("Exhaust") || ps.gameObject.name.Contains("exhaust") || ps.gameObject.name.Contains("Trail") || ps.gameObject.name.Contains("trail"))
                    {
                        if (exhaustIndex == 0 && exhaustVFX1 == null) { exhaustVFX1 = ps; exhaustIndex++; }
                        else if (exhaustIndex == 1 && exhaustVFX2 == null) { exhaustVFX2 = ps; exhaustIndex++; }
                    }
                }
                // If still not found, just grab any particle systems
                if (exhaustVFX1 == null && allPS.Length > 0) exhaustVFX1 = allPS[0];
                if (exhaustVFX2 == null && allPS.Length > 1) exhaustVFX2 = allPS[1];
            }
            
            // Start exhaust VFX
            StartExhaust();

            // Init Pools
            if (currentStage.BulletPrefab != null)
            {
                if (PoolManager.PoolExists(currentStage.BulletPrefab.name))
                {
                    bulletPool = PoolManager.GetPoolByName(currentStage.BulletPrefab.name);
                }
                else
                {
                    bulletPool = PoolManager.AddPool(new PoolSettings(currentStage.BulletPrefab.name, currentStage.BulletPrefab, 10, true));
                }
            }

            if (muzzleParticlePrefab != null)
            {
                if (PoolManager.PoolExists(muzzleParticlePrefab.name))
                {
                    muzzlePool = PoolManager.GetPoolByName(muzzleParticlePrefab.name);
                }
                else
                {
                    muzzlePool = PoolManager.AddPool(new PoolSettings(muzzleParticlePrefab.name, muzzleParticlePrefab, 5, true));
                }
            }
        }

        private void StartExhaust()
        {
            if (exhaustVFX1 != null)
            {
                exhaustVFX1.gameObject.SetActive(true);
                exhaustVFX1.Play(true);
            }
            if (exhaustVFX2 != null)
            {
                exhaustVFX2.gameObject.SetActive(true);
                exhaustVFX2.Play(true);
            }
            exhaustPlaying = true;
        }

        private void StopExhaust()
        {
            if (exhaustVFX1 != null && exhaustVFX1.isPlaying)
                exhaustVFX1.Stop(true);
            if (exhaustVFX2 != null && exhaustVFX2.isPlaying)
                exhaustVFX2.Stop(true);
            exhaustPlaying = false;
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private void Update()
        {
            if (player == null || CharacterBehaviour.IsDead) return;

            currentEnemyTarget = player.ClosestEnemyBehaviour;

            Vector3 targetPosition = GetTargetPosition(currentEnemyTarget);

            // Check if drone is moving
            float distToTarget = Vector3.Distance(transform.position, targetPosition);
            bool isMoving = distToTarget > 0.1f;

            // Toggle exhaust VFX based on movement
            if (isMoving && !exhaustPlaying)
                StartExhaust();
            else if (!isMoving && exhaustPlaying)
                StopExhaust();

            // Move smoothly towards target
            float speed = currentStage.MovementSpeed > 0 ? currentStage.MovementSpeed : 10f;
            
            if (distToTarget > 15f)
            {
                transform.position = targetPosition; // Teleport if too far (e.g., room change)
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime, speed);
            }

            // Rotate and shoot
            if (currentEnemyTarget != null && !currentEnemyTarget.IsDead)
            {
                // Rotate towards enemy
                Vector3 direction = (currentEnemyTarget.transform.position - transform.position).SetY(0).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }

                // Check shooting range and fire rate
                float distanceToEnemy = Vector3.Distance(transform.position, currentEnemyTarget.transform.position);
                if (distanceToEnemy <= currentStage.RangeRadius)
                {
                    if (Time.time - lastShootTime >= (1f / currentStage.FireRate))
                    {
                        Shoot();
                    }
                }
            }
            else
            {
                // Rotate to match player if no enemy
                transform.rotation = Quaternion.Slerp(transform.rotation, player.transform.rotation, Time.deltaTime * 5f);
            }
        }

        private Vector3 GetTargetPosition(BaseEnemyBehavior enemy)
        {
            Vector3 anchorPoint = player.transform.position + Vector3.up * 1.5f;

            if (enemy == null || enemy.IsDead)
            {
                // Idle position (e.g. back right of the player)
                Vector3 idleOffset = player.transform.rotation * new Vector3(1f, 0, -1f);
                return anchorPoint + idleOffset;
            }
            else
            {
                // Move towards enemy but keep inside follow radius
                Vector3 directionToEnemy = (enemy.transform.position - anchorPoint).SetY(0);
                if (directionToEnemy.magnitude > followRadius)
                {
                    directionToEnemy = directionToEnemy.normalized * followRadius;
                }
                return anchorPoint + directionToEnemy;
            }
        }

        private void Shoot()
        {
            lastShootTime = Time.time;

            if (bulletPool != null)
            {
                GameObject bulletObj = bulletPool.GetPooledObject();
                bulletObj.transform.position = shootPoint.position;
                bulletObj.transform.rotation = shootPoint.rotation;

                if (bulletObj != null)
                {
                    DroneBulletBehavior bulletBehavior = bulletObj.GetComponent<DroneBulletBehavior>();
                    if (bulletBehavior != null)
                    {
                        float damage = currentStage.Damage.Random();
                        float speed = currentStage.BulletSpeed.Random();
                        bulletBehavior.Initialise(damage, speed, currentEnemyTarget);
                    }
                }
            }

            if (muzzlePool != null)
            {
                GameObject muzzleObj = muzzlePool.GetPooledObject();
                muzzleObj.transform.position = shootPoint.position;
                muzzleObj.transform.rotation = shootPoint.rotation;

                // Return to pool after 1 second
                Tween.DelayedCall(1.0f, () => {
                    if (muzzleObj != null) muzzleObj.SetActive(false);
                });
            }
        }
    }
}
