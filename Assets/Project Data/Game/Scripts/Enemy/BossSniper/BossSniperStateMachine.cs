using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    [RequireComponent(typeof(BossSniperBehavior))]
    public class BossSniperStateMachine : AbstractStateMachine<BossSniperStates>
    {
        private BossSniperBehavior enemy;

        private BossSniperChangingPositionState changePosState;
        private BossSniperAimState aimState;
        private BossSniperAttackState attackState;

        private void Awake()
        {
            enemy = GetComponent<BossSniperBehavior>();

            var changePosCase = new StateCase();
            changePosState = new BossSniperChangingPositionState(enemy);
            changePosCase.state = changePosState;
            changePosCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(ChangePosTransition, StateTransitionType.OnFinish)
            };

            var aimCase = new StateCase();
            aimState = new BossSniperAimState(enemy);
            aimCase.state = aimState;
            aimCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(AimTransition, StateTransitionType.OnFinish)
            };

            var shootCase = new StateCase();
            attackState = new BossSniperAttackState(enemy);
            shootCase.state = attackState;
            shootCase.transitions = new List<StateTransition<BossSniperStates>>()
            {
                new StateTransition<BossSniperStates>(ShootTransition, StateTransitionType.OnFinish)
            };

            states.Add(BossSniperStates.ChangingPosition, changePosCase);
            states.Add(BossSniperStates.Aiming, aimCase);
            states.Add(BossSniperStates.Shooting, shootCase);
        }

        private bool ChangePosTransition(out BossSniperStates nextState)
        {
            nextState = BossSniperStates.Aiming;
            return true;
        }

        private bool AimTransition(out BossSniperStates nextState)
        {
            nextState = BossSniperStates.Shooting;
            return true;
        }

        private int shootCount = 0;

        private bool ShootTransition(out BossSniperStates nextState)
        {
            shootCount++;
            if (shootCount == 1)
            {
                shootCount = 0;
                nextState = BossSniperStates.ChangingPosition;
            }
            else
            {
                nextState = BossSniperStates.Aiming;
            }

            return true;
        }
    }
}
