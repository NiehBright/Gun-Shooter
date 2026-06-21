using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Gun Upgrade");

        [Header("Animations")]
        [SerializeField] AnimationClip characterShootAnimation;

        [Space]
        [SerializeField] Transform leftHandHolder;
        [SerializeField] Transform rightHandHolder;

        [Space]
        [SerializeField] 
        protected Transform shootPoint;

        [Header("Upgrade")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [SerializeField] float upgradeParticleSize = 1.0f;

        protected CharacterBehaviour characterBehaviour;
        protected WeaponData data;

        protected DuoInt damage;
        public DuoInt Damage => damage;

        private Transform leftHandRigController;
        private Vector3 leftHandExtraRotation;

        private Transform rightHandRigController;
        private Vector3 rightHandExtraRotation;

        public virtual void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            this.characterBehaviour = characterBehaviour;
            this.data = data;
        }

        public void InitialiseCharacter(BaseCharacterGraphics characterGraphics)
        {
            leftHandRigController = characterGraphics.LeftHandRig.data.target;
            rightHandRigController = characterGraphics.RightHandRig.data.target;

            leftHandExtraRotation = characterGraphics.LeftHandExtraRotation;
            rightHandExtraRotation = characterGraphics.RightHandExtraRotation;

            characterGraphics.SetShootingAnimation(characterShootAnimation);
        }

        public virtual void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public virtual void GunUpdate()
        {

        }

        public void UpdateHandRig()
        {
            leftHandRigController.position = leftHandHolder.position;
            rightHandRigController.position = rightHandHolder.position;

#if UNITY_EDITOR
            if(characterBehaviour != null && characterBehaviour.Graphics != null)
            {
                leftHandExtraRotation = characterBehaviour.Graphics.LeftHandExtraRotation;
                rightHandExtraRotation = characterBehaviour.Graphics.RightHandExtraRotation;
            }
#endif

            leftHandRigController.rotation = Quaternion.Euler(leftHandHolder.eulerAngles + leftHandExtraRotation);
            rightHandRigController.rotation = Quaternion.Euler(rightHandHolder.eulerAngles + rightHandExtraRotation);
        }

        public abstract void Reload();
        public abstract void OnGunUnloaded();
        public abstract void PlaceGun(BaseCharacterGraphics characterGraphics);

        public abstract void RecalculateDamage();

        public AnimationClip GetShootAnimationClip()
        {
            return characterShootAnimation;
        }

        public virtual void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void SetDamage(DuoInt damage)
        {
            this.damage = damage;
        }

        public void SetDamage(int minDamage, int maxDamage)
        {
            damage = new DuoInt(minDamage, maxDamage);
        }

        public void PlayUpgradeParticle()
        {
            ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + upgradeParticleOffset).SetScale(upgradeParticleSize.ToVector3());
            particleCase.ParticleSystem.transform.rotation = CameraController.MainCamera.transform.rotation;
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + upgradeParticleOffset, upgradeParticleSize.ToVector3());
        }

#if UNITY_EDITOR
        [Button("Prepare Weapon")]
        private void PrepareWeapon()
        {
            if(leftHandHolder == null)
            {
                GameObject leftHandHolderObject = new GameObject("Left Hand Holder");
                leftHandHolderObject.transform.SetParent(transform);
                leftHandHolderObject.transform.ResetLocal();
                leftHandHolderObject.transform.localPosition = new Vector3(-0.4f, 0, 0);

                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_3");
                UnityEditor.EditorGUIUtility.SetIconForObject(leftHandHolderObject, (Texture2D)iconContent.image);

                leftHandHolder = leftHandHolderObject.transform;
            }

            if (rightHandHolder == null)
            {
                GameObject rightHandHolderObject = new GameObject("Right Hand Holder");
                rightHandHolderObject.transform.SetParent(transform);
                rightHandHolderObject.transform.ResetLocal();
                rightHandHolderObject.transform.localPosition = new Vector3(0.4f, 0, 0);

                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_4");
                UnityEditor.EditorGUIUtility.SetIconForObject(rightHandHolderObject, (Texture2D)iconContent.image);

                rightHandHolder = rightHandHolderObject.transform;
            }

            if(shootPoint == null)
            {
                GameObject shootingPointObject = new GameObject("Shooting Point");
                shootingPointObject.transform.SetParent(transform);
                shootingPointObject.transform.ResetLocal();
                shootingPointObject.transform.localPosition = new Vector3(0, 0, 1);

                GUIContent iconContent = UnityEditor.EditorGUIUtility.IconContent("sv_label_1");
                UnityEditor.EditorGUIUtility.SetIconForObject(shootingPointObject, (Texture2D)iconContent.image);

                shootPoint = shootingPointObject.transform;
            }

            if(characterShootAnimation == null)
            {
                characterShootAnimation = RuntimeEditorUtils.GetAssetByName<AnimationClip>("Shot");
            }
        }
#endif
    }
}