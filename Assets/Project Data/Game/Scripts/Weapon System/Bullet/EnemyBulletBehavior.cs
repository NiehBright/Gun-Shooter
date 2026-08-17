using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyBulletBehavior : MonoBehaviour
    {
        [SerializeField] TrailRenderer trailRenderer;

        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");

        protected float damage;
        protected float speed;

        protected float selfDestroyDistance;
        protected float distanceTraveled = 0;

        protected TweenCase disableTweenCase;
        protected Rigidbody rigidBody;
        private SimpleCallback trailRendererCallback;
        private float trailTimeCache;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            trailRendererCallback = ResetTrailRenderer;
        }

        private void ResetTrailRenderer()
        {
            trailRenderer.Clear();
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.Clear();
            trailRenderer.time = trailTimeCache;
        }

        public virtual void Initialise(float damage, float speed, float selfDestroyDistance)
        {
            this.damage = damage;
            this.speed = speed;

            this.selfDestroyDistance = selfDestroyDistance;
            distanceTraveled = 0;

            trailRenderer.Clear();
            trailTimeCache = trailRenderer.time;
            trailRenderer.time = 0;

            gameObject.SetActive(true);
            Tween.NextFrame(trailRendererCallback);
        }

        protected virtual void FixedUpdate()
        {
            rigidBody.MovePosition(rigidBody.position + transform.forward * speed * Time.fixedDeltaTime);

            if (selfDestroyDistance != -1)
            {
                distanceTraveled += speed * Time.fixedDeltaTime;

                if (distanceTraveled >= selfDestroyDistance)
                {
                    SelfDestroy();
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                if (other.TryGetComponent<CharacterBehaviour>(out var characterBehaviour))
                {
                    // Deal damage to enemy
                    characterBehaviour.TakeDamage(damage);

                    SelfDestroy();
                }

                ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                SelfDestroy();

                ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            }
        }

        public void SelfDestroy()
        {
            // Disable bullet
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}