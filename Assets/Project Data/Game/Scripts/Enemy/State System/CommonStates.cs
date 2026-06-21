using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy
{
    public class PatrollingState: StateBehavior<BaseEnemyBehavior>
    {
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private bool isStationary = false;

        private Vector3 FirstPoint => Target.PatrollingPoints[0];
        private int pointId = 0;

        private TweenCase idleCase;

        public PatrollingState(BaseEnemyBehavior enemy): base(enemy)
        {

        }

        public override void OnStart()
        {
            idleCase.KillActive();

            if (Target.PatrollingPoints.Length == 0 || (Target.PatrollingPoints.Length == 1 && Vector3.Distance(Position, FirstPoint) < 1))
            {
                isStationary = true;
            }
            else
            {
                Target.NavMeshAgent.speed = Target.Stats.PatrollingSpeed;

                pointId = 0;
                GoToPoint();
            }
        }

        private void GoToPoint()
        {
            var point = Target.PatrollingPoints[pointId];

            Target.MoveToPoint(point);

            isStationary = false;
        }

        public override void OnUpdate()
        {
            if (!isStationary && Vector3.Distance(Position, Target.PatrollingPoints[pointId]) < 1)
            {
                if (Target.PatrollingPoints.Length == 1)
                {
                    isStationary = true;
                    Target.NavMeshAgent.isStopped = true;
                }
                else
                {
                    pointId++;
                    if (pointId == Target.PatrollingPoints.Length) pointId = 0;

                    idleCase.KillActive();
                    idleCase = Tween.DelayedCall(Target.Stats.PatrollingIdleDuration, GoToPoint);
                }
            }

            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, Target.NavMeshAgent.velocity.magnitude / Target.NavMeshAgent.speed * Target.Stats.PatrollingMutliplier);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
            idleCase.KillActive();
        }
    }

    public class FollowingState : StateBehavior<BaseEnemyBehavior>
    {
        public FollowingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 cachedTargetPos;
        private bool isSlowed = false;

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
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.Target.position, cachedTargetPos) > 0.5f)
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
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }
    }

    public class FleeingState : StateBehavior<BaseEnemyBehavior>
    {
        public FleeingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 fleePoint;

        public override void OnStart()
        {
            Target.NavMeshAgent.speed = Target.Stats.MoveSpeed;
            fleePoint = GetRandomPointOnLevel();

            Target.MoveToPoint(fleePoint);
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Target.transform.position, fleePoint) < 5f || Vector3.Distance(Target.TargetPosition, fleePoint) < Target.Stats.FleeDistance)
            {
                fleePoint = GetRandomPointOnLevel();
                Target.MoveToPoint(fleePoint);
            }

            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, Target.NavMeshAgent.velocity.magnitude / Target.Stats.MoveSpeed);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }

        public Vector3 GetRandomPointOnLevel()
        {
            int counter = 0;
            while (true)
            {
                counter++;

                var testPoint = Target.Position + Random.onUnitSphere.SetY(0) * Random.Range(10, 100);

                if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out var hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    if (Vector3.Distance(Target.Target.position, testPoint) > Target.Stats.AttackDistance)
                    {
                        return testPoint;
                    }
                }

                if (counter > 1000)
                {
                    return Target.Position;

                }
            }
        }
    }

    public class AttackingState : StateBehavior<BaseEnemyBehavior>
    {
        public AttackingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        public bool IsFinished { get; private set; }

        public override void OnStart()
        {
            IsFinished = false;

            Target.OnAttackFinished += OnAttackFinished;

            Target.Attack();
        }

        public override void OnUpdate()
        {
            Target.transform.rotation = Quaternion.LookRotation((Target.TargetPosition - Target.Position).SetY(0).normalized);
        }

        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackFinished;
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }

    public class AimAndAttackState : StateBehavior<BaseEnemyBehavior>
    {
        public AimAndAttackState(BaseEnemyBehavior enemy) : base(enemy)
        {
            rangeEnemy = enemy as RangeEnemyBehaviour;
        }

        private RangeEnemyBehaviour rangeEnemy;

        public bool IsFinished { get; private set; }

        private bool AimHash { set => Target.Animator.SetBool("Aim", value); }

        private float nextAttackTime = 0;
        private bool hasAttacked = false;

        public override void OnStart()
        {
            AimHash = true;
            IsFinished = false;

            Target.CanMove = false;
            nextAttackTime = Time.time + Target.Stats.AimDuration;

            if(rangeEnemy != null && rangeEnemy.CanReload)
            {
                Target.OnReloadFinished += OnAttackFinished;
            } else
            {
                Target.OnAttackFinished += OnAttackFinished;
            }
            

            hasAttacked = false;
        }

        public override void OnUpdate()
        {
            Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, Quaternion.LookRotation((Target.TargetPosition - Position).normalized), Time.deltaTime * 10);

            if (!hasAttacked && Time.time > nextAttackTime)
            {
                Target.Attack();
                hasAttacked = true;
            }
        }

        public override void OnEnd()
        {
            AimHash = false;

            if (rangeEnemy != null && rangeEnemy.CanReload)
            {
                Target.OnReloadFinished -= OnAttackFinished;
            }
            else
            {
                Target.OnAttackFinished -= OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }
}