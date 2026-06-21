using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class BossSniperBulletBehavior : EnemyBulletBehavior
    {
        private static readonly int PARTICLE_WAll_HIT_HASH = ParticlesController.GetHash("Minigun Wall Hit");

        [SerializeField] LayerMask collisionLayer;

        private List<Vector3> hitPoints;

        private int nextHitPointId = 0;
        private Vector3 NextHitPoint => hitPoints[nextHitPointId];

        public void InitialiseBullet(float damage, float speed, float selfDestroyDistance, List<Vector3> hitPoints)
        {
            Initialise(damage, speed, selfDestroyDistance);

            this.hitPoints = new List<Vector3>(hitPoints.ToArray());
            nextHitPointId = 0;
        }

        protected override void FixedUpdate()
        {
            var distanceTraveledDuringThisFrame = speed * Time.fixedDeltaTime;
            var distanceToNextHitPoint = (NextHitPoint - transform.position).magnitude;

            if (distanceTraveledDuringThisFrame > distanceToNextHitPoint)
            {
                transform.position = NextHitPoint;

                nextHitPointId++;

                if (nextHitPointId >= hitPoints.Count)
                {
                    ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
                    SelfDestroy();
                }
                else
                {
                    ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
                    transform.forward = (NextHitPoint - transform.position).normalized;
                }
            }
            else
            {
                var directionToNextHitPoint = (NextHitPoint - transform.position).normalized;

                transform.position += directionToNextHitPoint * distanceTraveledDuringThisFrame;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                var character = other.GetComponent<CharacterBehaviour>();
                if (character != null)
                {
                    character.TakeDamage(damage);

                    SelfDestroy();
                }
            }
        }
    }
}