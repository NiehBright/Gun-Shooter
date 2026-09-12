using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class OrbitalLaserBehaviour : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private float burstDamage = 150f;
        [SerializeField] private float burnDamagePerTick = 30f;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private float duration = 4f;
        [SerializeField] private float slowMultiplier = 0.5f;

        public float Radius { get => radius; set => radius = value; }
        public float BurstDamage { get => burstDamage; set => burstDamage = value; }
        public float BurnDamagePerTick { get => burnDamagePerTick; set => burnDamagePerTick = value; }
        public float TickInterval { get => tickInterval; set => tickInterval = value; }
        public float Duration { get => duration; set => duration = value; }

        private float lifetime;
        private float nextDamageTime;
        private bool isInitialised = false;
        private readonly HashSet<BaseEnemyBehavior> slowedEnemies = new HashSet<BaseEnemyBehavior>();

        private void Start()
        {
            if (!isInitialised)
            {
                // Giảm kích thước 50% (visual scale ~1.2)
                float visualScale = Mathf.Clamp(radius * 0.8f, 0.9f, 1.5f);
                transform.localScale = new Vector3(visualScale, 1f, visualScale);
            }
        }

        public void Initialise(float radius, float burstDamage, float burnDamagePerTick, float tickInterval, float duration, float slowMultiplier = 0.5f)
        {
            this.radius = radius;
            this.burstDamage = burstDamage;
            this.burnDamagePerTick = burnDamagePerTick;
            this.tickInterval = tickInterval;
            this.duration = duration;
            this.slowMultiplier = slowMultiplier;

            this.lifetime = 0f;
            this.nextDamageTime = Time.time; // Bắt đầu gây sát thương liên tục ngay lập tức
            this.isInitialised = true;

            // Giảm kích thước 50% (visual scale ~1.2) theo yêu cầu người dùng
            float visualScale = Mathf.Clamp(radius * 0.8f, 0.9f, 1.5f);
            transform.localScale = new Vector3(visualScale, 1f, visualScale);

            // Rung màn hình khi laser dội xuống
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null && player.MainCameraCase != null)
            {
                player.MainCameraCase.Shake(0.08f, 0.08f, 0.35f, 1.2f);
            }

            // Đòn nổ ban đầu (Impact Burst)
            ExecuteBurstImpact();
        }

        private void ExecuteBurstImpact()
        {
            float hitRadius = Mathf.Max(radius, 2.0f);
            Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                    if (pushDir == Vector3.zero) pushDir = Vector3.forward;

                    enemy.TakeDamage(burstDamage, transform.position, pushDir);
                    ApplySlow(enemy);
                }
            }
        }

        private void Update()
        {
            lifetime += Time.deltaTime;
            if (lifetime >= duration)
            {
                ResetAllSlows();
                Destroy(gameObject);
                return;
            }

            // 1. Quét tìm và duy trì hiệu ứng làm chậm (Slow) cho quái vật trong vùng Laser
            float hitRadius = Mathf.Max(radius, 2.0f);
            Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
            HashSet<BaseEnemyBehavior> currentEnemiesInArea = new HashSet<BaseEnemyBehavior>();

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    currentEnemiesInArea.Add(enemy);
                    ApplySlow(enemy);
                }
            }

            // Khôi phục tốc độ cho những quái vật đã rời khỏi vùng Laser
            List<BaseEnemyBehavior> exitedEnemies = null;
            foreach (var slowed in slowedEnemies)
            {
                if (slowed == null || slowed.IsDead || !currentEnemiesInArea.Contains(slowed))
                {
                    if (exitedEnemies == null) exitedEnemies = new List<BaseEnemyBehavior>();
                    exitedEnemies.Add(slowed);
                }
            }

            if (exitedEnemies != null)
            {
                foreach (var exited in exitedEnemies)
                {
                    ResetSlow(exited);
                    slowedEnemies.Remove(exited);
                }
            }

            // 2. Gây sát thương bức xạ định kỳ (Lingering Burn Damage)
            if (Time.time >= nextDamageTime)
            {
                nextDamageTime = Time.time + tickInterval;

                foreach (var enemy in currentEnemiesInArea)
                {
                    if (enemy != null && !enemy.IsDead)
                    {
                        Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                        if (pushDir == Vector3.zero) pushDir = Vector3.forward;

                        enemy.TakeDamage(burnDamagePerTick, transform.position, pushDir);
                    }
                }
            }
        }

        private void ApplySlow(BaseEnemyBehavior enemy)
        {
            if (enemy == null || enemy.IsDead) return;

            if (enemy.NavMeshAgent != null && enemy.NavMeshAgent.enabled)
            {
                float targetSpeed = enemy.RunningSpeed * slowMultiplier;
                if (enemy.NavMeshAgent.speed > targetSpeed)
                {
                    enemy.NavMeshAgent.speed = targetSpeed;
                }
            }

            slowedEnemies.Add(enemy);
        }

        private void ResetSlow(BaseEnemyBehavior enemy)
        {
            if (enemy == null) return;

            if (enemy.NavMeshAgent != null && enemy.NavMeshAgent.enabled)
            {
                enemy.NavMeshAgent.speed = enemy.RunningSpeed;
            }
        }

        private void ResetAllSlows()
        {
            foreach (var enemy in slowedEnemies)
            {
                ResetSlow(enemy);
            }
            slowedEnemies.Clear();
        }

        private void OnDestroy()
        {
            ResetAllSlows();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f); // Cyan laser sphere
            Gizmos.DrawWireSphere(transform.position, radius);

            UnityEditor.Handles.color = new Color(0f, 0.8f, 1f, 0.12f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
        }
#endif
    }
}
