using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class DroneBulletBehavior : MonoBehaviour
    {
        protected float damage;
        protected float speed;
        private bool autoDisableOnHit;

        private TweenCase disableTweenCase;

        protected BaseEnemyBehavior currentTarget;
        protected Rigidbody rigidBody;
        private SimpleCallback disableBulletCallback;

        [SerializeField] GameObject hitParticlePrefab;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            disableBulletCallback = DisableBullet;
        }

        private void DisableBullet()
        {
            if (this != null && gameObject != null)
            {
                Destroy(gameObject); // Instead of SetActive(false) to prevent memory leaks since it's not pooled
            }
        }

        private void OnDestroy()
        {
            disableTweenCase.KillActive();
        }

        public virtual void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime = 5f, bool autoDisableOnHit = true)
        {
            this.damage = damage;
            this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;
            this.currentTarget = currentTarget;

            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, disableBulletCallback);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (speed != 0)
                rigidBody.MovePosition(rigidBody.position + transform.forward * speed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (other.TryGetComponent<BaseEnemyBehavior>(out var baseEnemyBehavior))
                {
                    if (!baseEnemyBehavior.IsDead)
                    {
                        disableTweenCase.KillActive();

                        OnEnemyHit(baseEnemyBehavior);

                        if (hitParticlePrefab != null)
                        {
                            Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
                        }

                        if (autoDisableOnHit)
                        {
                            DisableBullet();
                        }
                    }
                }
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                disableTweenCase.KillActive();

                if (hitParticlePrefab != null)
                {
                    Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
                }

                if (autoDisableOnHit)
                {
                    DisableBullet();
                }
            }
        }

        protected virtual void OnEnemyHit(BaseEnemyBehavior baseEnemyBehavior)
        {
            baseEnemyBehavior.TakeDamage(damage, transform.position, transform.forward);
        }
    }
}
