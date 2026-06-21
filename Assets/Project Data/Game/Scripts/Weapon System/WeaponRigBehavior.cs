using UnityEngine;
using UnityEngine.UIElements;

namespace Watermelon.SquadShooter
{
    [ExecuteAlways]
    public class WeaponRigBehavior : MonoBehaviour
    {
        [SerializeField] WeaponRigType rigType;
        [SerializeField] PrimaryHandType primaryHandType;

        [SerializeField] Transform primaryHandAnchor;
        [SerializeField, ShowIf("NeedsOffHand")] Transform offHandAnchor;

        [Space]
        [SerializeField] bool stickToPrimaryHand;

        [SerializeField, HideInInspector] Transform primaryHandBone;
        [SerializeField, HideInInspector] Transform offHandBone;

        [Header("Dev")]
        [SerializeField] bool enableRigWeaponInEditor = true;

        [SerializeField, HideInInspector] BaseEnemyBehavior enemy;

        public void Initialise(BaseEnemyBehavior enemy)
        {
            this.enemy = enemy;

            if (primaryHandType == PrimaryHandType.Right)
            {
                primaryHandBone = enemy.RightHandBone;
                offHandBone = enemy.LeftHandBone;
            }
            else
            {
                primaryHandBone = enemy.LeftHandBone;
                offHandBone = enemy.RightHandBone;
            }
        }

        private void Update()
        {
            if (!(Application.isPlaying || enableRigWeaponInEditor)) return;

            if (primaryHandBone == null || primaryHandAnchor == null) return;
            if (rigType == WeaponRigType.TwoHanded && (offHandBone == null || offHandAnchor == null)) return;

            if (rigType == WeaponRigType.OneHanded)
            {
                OneHandedUpdate();
            }
            else
            {
                TwoHandedUpdate();
            }
        }

        private void OneHandedUpdate()
        {
            Quaternion desiredRotation = primaryHandBone.rotation;

            Quaternion rotationCorrection = desiredRotation * Quaternion.Inverse(primaryHandAnchor.localRotation);

            transform.rotation = rotationCorrection;

            Vector3 positionCorrection = primaryHandBone.position - primaryHandAnchor.position;

            transform.position = transform.position + positionCorrection;
        }

        private void TwoHandedUpdate()
        {
            var parent = transform;

            var scale = Vector3.one;

            while (parent != null)
            {
                scale = scale.Mult(parent.localScale);

                parent = parent.parent;
            }

            if (stickToPrimaryHand)
            {
                transform.position = primaryHandBone.position - transform.rotation * primaryHandAnchor.localPosition.Mult(scale);
            }
            else
            {
                var middleBetweenAnchors = (primaryHandAnchor.localPosition + offHandAnchor.localPosition) / 2;
                var middleBetweenHands = (offHandBone.position + primaryHandBone.position) / 2;

                transform.position = middleBetweenHands - transform.rotation * middleBetweenAnchors.Mult(scale);
            }

            var direction = (primaryHandBone.position - offHandBone.position).normalized;
            var up = Vector3.up;
            transform.rotation = Quaternion.LookRotation(direction, up);
        }

        [Button("Reset Anchor Position")]
        public void ResetAnchorPosition()
        {
            if (enemy == null)
            {
                Debug.LogError("The weapon is not assigned to an enemy!");
                return;
            }

            if (primaryHandBone == null)
            {
                Debug.LogError($"The enemy does not have the {primaryHandType} bone assigned!");
                return;
            }

            if (primaryHandAnchor == null)
            {
                Debug.LogError("The weapon does not have 'Primary Hand Anchor' transform assigned!");
                return;
            }

            if (rigType == WeaponRigType.TwoHanded)
            {
                if (offHandBone == null)
                {
                    var offHand = primaryHandType == PrimaryHandType.Right ? "Left" : "Right";
                    Debug.LogError($"The enemy does not have the {offHand} bone assigned!");

                    return;
                }

                if (offHandAnchor == null)
                {
                    Debug.LogError("The weapon does not have 'Off Hand Anchor' transform assigned!");
                    return;
                }
            }

            primaryHandAnchor.position = primaryHandBone.position;
            primaryHandAnchor.rotation = primaryHandBone.rotation;

            if (rigType == WeaponRigType.TwoHanded)
            {
                offHandAnchor.position = offHandBone.position;
                offHandAnchor.rotation = offHandBone.rotation;
            }
        }

        private void OnValidate()
        {
            if (enemy != null) Initialise(enemy);
        }

        private bool NeedsOffHand()
        {
            return rigType == WeaponRigType.TwoHanded;
        }

        public enum WeaponRigType
        {
            OneHanded,
            TwoHanded,
        }

        public enum PrimaryHandType
        {
            Left,
            Right,
        }
    }
}