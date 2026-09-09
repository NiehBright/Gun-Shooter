using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Melee
{
    public class MeleeFollowAttackState: StateBehavior<MeleeEnemyBehaviour>
    {
        public MeleeFollowAttackState(MeleeEnemyBehaviour melee): base(melee)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 cachedTargetPos;
        private bool isSlowed = false;
        private bool isAttacking = false;
        private float repathCooldown = 0f;

        public override void OnStart()
        {
            cachedTargetPos = Target.Target.position;

            isSlowed = Target.IsWalking;
            if (isSlowed)
            {
                Target.NavMeshAgent.speed = Target.Stats.PatrollingSpeed;
            }
            else
            {
                Target.NavMeshAgent.speed = Target.Stats.MoveSpeed;
            }

            Target.MoveToPoint(cachedTargetPos);
            repathCooldown = Random.Range(0.05f, 0.25f);
            attackSafetyTimer = 0f;
            isAttacking = false;
        }

        private float attackSafetyTimer = 0f;

        public override void OnUpdate()
        {
            float distToTarget = Vector3.Distance(Target.transform.position, Target.Target.position);

            repathCooldown -= Time.deltaTime;
            if (repathCooldown <= 0f)
            {
                // When close to player, always keep destination locked to player so enemy reaches attack distance (1.09m)!
                // When further, update if player moved > 0.2f
                if (distToTarget <= 3.5f || Vector3.Distance(Target.Target.position, cachedTargetPos) > 0.2f)
                {
                    cachedTargetPos = Target.Target.position;
                    Target.MoveToPoint(cachedTargetPos);
                }
                repathCooldown = distToTarget <= 3.5f ? 0.12f : Random.Range(0.2f, 0.3f);
            }

            if (isAttacking)
            {
                attackSafetyTimer += Time.deltaTime;
                if (attackSafetyTimer >= 1.2f)
                {
                    isAttacking = false;
                    attackSafetyTimer = 0f;
                }
            }
            else
            {
                attackSafetyTimer = 0f;
            }

            if (isSlowed && !Target.IsWalking)
            {
                Target.NavMeshAgent.speed = Target.Stats.MoveSpeed;
            }
            else if (!isSlowed && Target.IsWalking)
            {
                Target.NavMeshAgent.speed = Target.Stats.PatrollingSpeed;
            }

            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, Target.NavMeshAgent.velocity.magnitude / Target.NavMeshAgent.speed * (isSlowed ? Target.Stats.PatrollingMutliplier : 1f));

            if (Target.IsTargetInAttackRange && !isAttacking && !CharacterBehaviour.IsDead)
            {
                isAttacking = true;
                attackSafetyTimer = 0f;
                Target.Attack();
                Target.OnAttackFinished += OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            Target.OnAttackFinished -= OnAttackFinished;
            isAttacking = false;
            attackSafetyTimer = 0f;
        }

        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackFinished;
            Target.StopMoving();
        }
    }

    public enum State
    {
        Patrolling,
        Attacking,
    }
}
