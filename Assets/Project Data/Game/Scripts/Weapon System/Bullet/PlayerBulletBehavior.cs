using UnityEngine;

namespace Watermelon.SquadShooter
{
    // base class for player bullets
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        protected float damage;
        protected float speed;
        private bool autoDisableOnHit;

        private TweenCase disableTweenCase;

        protected BaseEnemyBehavior currentTarget;

        protected Rigidbody rigidBody;
        private SimpleCallback disableBulletCallback;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            disableBulletCallback = DisableBullet;
        }

        private void DisableBullet()
        {
            gameObject.SetActive(false);
        }

        public virtual void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
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

                        // Disable bullet
                        if (autoDisableOnHit)
                            gameObject.SetActive(false);

                        // Deal damage to enemy
                        baseEnemyBehavior.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damage, transform.position, transform.forward);

                        // Call hit callback
                        OnEnemyHitted(baseEnemyBehavior);
                    }
                }
            }
            else
            {
                // Bỏ qua va chạm với Player để đạn không tự hủy ngay khi vừa rời nòng súng
                if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
                    return;

                OnObstacleHitted();
            }
        }

        private void OnDisable()
        {
            disableTweenCase.KillActive();
        }

        private void OnDestroy()
        {
            disableTweenCase.KillActive();
        }

        protected abstract void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior);

        protected virtual void OnObstacleHitted()
        {
            disableTweenCase.KillActive();

            gameObject.SetActive(false);
        }
    }
}