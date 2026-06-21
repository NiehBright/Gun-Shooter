using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyAnimationCallback : MonoBehaviour
    {
        private BaseEnemyBehavior baseEnemyBehavior;

        public void Initialise(BaseEnemyBehavior baseEnemyBehavior)
        {
            this.baseEnemyBehavior = baseEnemyBehavior;
        }

        public void OnCallbackInvoked(EnemyCallbackType enemyCallbackType)
        {
            baseEnemyBehavior.OnAnimatorCallback(enemyCallbackType);
        }

        public void OnHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.Hit);
        }

        public void OnLeftHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.LeftHit);
        }

        public void OnRightHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.RightHit);
        }

        public void OnHitFinishCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.HitFinish);
        }

        public void OnBossLeftStepCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossLeftStep);
        }

        public void OnBossRightStepCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossRightStep);
        }

        public void OnBossDeathFallCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossDeathFall);
        }

        public void OnBossEnterFallCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossEnterFall);
        }

        public void OnBossKickCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossKick);
        }

        public void OnBossEnterFallFinishedCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossEnterFallFinished);
        }

        public void OnReloadFinishedCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.ReloadFinished);
        }

        [SerializeField] float ragdollSizeMultiplier = 1;

        [Button("Multiply Ragdoll Size")]
        private void MultiplyRagdollWidth()
        {
            if (Application.isPlaying) return;

            MultiplyRagdollWidthRecursively(transform);
        }

        private void MultiplyRagdollWidthRecursively(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                {
                    var sphereCollider = child.gameObject.GetComponent<SphereCollider>();
                    var capsuleCollider = child.gameObject.GetComponent<CapsuleCollider>();
                    var boxCollider = child.gameObject.GetComponent<BoxCollider>();

                    if (sphereCollider != null) sphereCollider.radius *= ragdollSizeMultiplier;
                    if (capsuleCollider != null) capsuleCollider.radius *= ragdollSizeMultiplier;

                    if(boxCollider != null)
                    {
                        boxCollider.size = boxCollider.size.MultX(ragdollSizeMultiplier).MultZ(ragdollSizeMultiplier);
                    }
                }

                MultiplyRagdollWidthRecursively(child);
            }
        }

        [Button("Clear Ragdoll")]
        private void ClearRagdoll()
        {
            if (Application.isPlaying) return;

            ClearRagdollRecursively(transform);
        }

        private void ClearRagdollRecursively(Transform parent)
        {
            for(int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.gameObject.layer == LayerMask.NameToLayer("Ragdoll"))
                {
                    var characterJoint = child.gameObject.GetComponent<CharacterJoint>();

                    if(characterJoint != null)
                    {
                        DestroyImmediate(characterJoint);
                    }

                    var rigidbody = child.gameObject.GetComponent<Rigidbody>();

                    if (rigidbody != null)
                    {
                        DestroyImmediate(rigidbody);
                    }

                    var collider = child.gameObject.GetComponent<Collider>();

                    while(collider != null)
                    {
                        DestroyImmediate(collider);
                        collider = child.gameObject.GetComponent<Collider>();
                    }
                }

                ClearRagdollRecursively(child);
            }
        }
    }

    public enum EnemyCallbackType
    {
        Hit = 0,
        HitFinish = 1,
        BossLeftStep = 2,
        BossRightStep = 3,
        BossDeathFall = 4,
        BossEnterFall = 5,
        BossKick = 6,
        BossEnterFallFinished = 7,
        ReloadFinished = 8,
        LeftHit = 9,
        RightHit = 10,
    }
}
