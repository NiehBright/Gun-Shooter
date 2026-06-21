using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    public class BossSniperChangingPositionState : StateBehavior<BossSniperBehavior>
    {
        private int positionId = 0;
        private Vector3 nextPosition;

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        public BossSniperChangingPositionState(BossSniperBehavior enemy) : base(enemy)
        {

        }

        public override void OnStart()
        {
            positionId++;
            if (positionId >= Target.PatrollingPoints.Length) positionId = 0;

            nextPosition = Target.PatrollingPoints[positionId];
            Target.MoveToPoint(nextPosition);
        }

        public override void OnUpdate()
        {
            if(Vector3.Distance(Target.Position, nextPosition) < 1f && !CharacterBehaviour.IsDead)
            {
                InvokeOnFinished();
            }

            Target.Animator.SetFloat(ANIMATOR_SPEED_HASH, Target.NavMeshAgent.velocity.magnitude / Target.Stats.MoveSpeed);
        }

        public override void OnEnd()
        {
            Target.StopMoving();
        }
    }

    public class BossSniperAimState: StateBehavior<BossSniperBehavior>
    {
        private BossSniperBehavior boss;

        public BossSniperAimState(BossSniperBehavior enemy) : base(enemy)
        {
            this.boss = enemy;
        }

        private bool isYellow;
        private TweenCase delayedCase;

        private float startAimingTime;

        private bool isAimingFinished = false;

        public override void OnStart()
        {
            isAimingFinished = false;
            isYellow = true;

            boss.MakeLaserYellow();

            delayedCase = Tween.DelayedCall(boss.YellowLaserAinimgDuration, () => {
                isYellow = false;
                boss.MakeLaserRed();

                delayedCase = Tween.DelayedCall(boss.RedLaserAimingDuration, () => {
                    isAimingFinished = true;
                });
            });

            startAimingTime = Time.time;

            boss.Animator.SetBool("Aim", true);

        }

        public override void OnUpdate()
        {
            if (isYellow || (!isYellow && boss.CanAimDuringRedLaserStage))
            {
                boss.Rotation = Quaternion.Lerp(boss.Rotation, Quaternion.LookRotation((Target.TargetPosition - Position).normalized), Time.deltaTime * 5);
            }

            if (Time.time > startAimingTime + 0.25f)
            {
                boss.AimLaser();
            }

            if (isAimingFinished)
            {
                if(!CharacterBehaviour.IsDead) InvokeOnFinished();
            }
        }

        public override void OnEnd()
        {
            delayedCase.KillActive();

            boss.DisableLaser();

            boss.Animator.SetBool("Aim", false);
        }
    }

    public class BossSniperAttackState : StateBehavior<BossSniperBehavior>
    {
        public BossSniperAttackState(BossSniperBehavior enemy) : base(enemy)
        {

        }

        public override void OnStart()
        {
            Target.Attack();
            Target.OnAttackFinished += OnAttackEnded;
        }

        private void OnAttackEnded()
        {
            InvokeOnFinished();
        }

        public override void OnEnd()
        {
            Target.OnAttackFinished -= OnAttackEnded;
        }
    }

    public enum BossSniperStates
    {
        ChangingPosition,
        Aiming,
        Shooting
    }
}