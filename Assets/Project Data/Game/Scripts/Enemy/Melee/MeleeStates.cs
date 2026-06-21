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

            isAttacking = false;
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.Target.position, cachedTargetPos) > 0.1f)
            {
                cachedTargetPos = Target.Target.position;
                Target.MoveToPoint(cachedTargetPos);
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
                Target.Attack();
                Target.OnAttackFinished += OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            Target.OnAttackFinished -= OnAttackFinished;
            isAttacking = false;
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
