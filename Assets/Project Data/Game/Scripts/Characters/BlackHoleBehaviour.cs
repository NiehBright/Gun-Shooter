using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class BlackHoleBehaviour : MonoBehaviour
    {
        private float radius;
        private float pullSpeed;
        private float damagePerTick;
        private float tickInterval;
        private float duration;

        private float lifetime;
        private float nextDamageTime;

        public void Initialise(float radius, float pullSpeed, float damagePerTick, float tickInterval, float duration)
        {
            this.radius = radius;
            this.pullSpeed = pullSpeed;
            this.damagePerTick = damagePerTick;
            this.tickInterval = tickInterval;
            this.duration = duration;

            this.lifetime = 0f;
            this.nextDamageTime = 0f;

            // Đồng bộ kích thước VFX theo bán kính
            transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
        }

        private void Update()
        {
            lifetime += Time.deltaTime;
            if (lifetime >= duration)
            {
                Destroy(gameObject);
                return;
            }

            // 1. Quét tìm và kéo quái vật về tâm
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    // Hút quái vật về tâm bằng Warp (an toàn cho NavMeshAgent)
                    if (enemy.NavMeshAgent != null && enemy.NavMeshAgent.enabled)
                    {
                        Vector3 newPos = Vector3.MoveTowards(enemy.transform.position, transform.position, pullSpeed * Time.deltaTime);
                        enemy.NavMeshAgent.Warp(newPos);
                    }
                    else
                    {
                        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, transform.position, pullSpeed * Time.deltaTime);
                    }
                }
            }

            // 2. Gây sát thương định kỳ
            if (Time.time >= nextDamageTime)
            {
                nextDamageTime = Time.time + tickInterval;

                foreach (var hit in hits)
                {
                    var enemy = hit.GetComponent<BaseEnemyBehavior>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                        enemy.TakeDamage(damagePerTick, transform.position, pushDir);
                    }
                }
            }
        }
    }
}
