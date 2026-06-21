using UnityEngine;
using UnityEngine.Animations.Rigging;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public abstract class BaseCharacterGraphics : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Upgrade");

        private readonly int ANIMATION_SHOT_HASH = Animator.StringToHash("Shot");
        private readonly int ANIMATION_HIT_HASH = Animator.StringToHash("Hit");

        private readonly int JUMP_ANIMATION_HASH = Animator.StringToHash("Jump");
        private readonly int GRUNT_ANIMATION_HASH = Animator.StringToHash("Grunt");

        [SerializeField]
        protected Animator characterAnimator;

        [Space]
        [SerializeField] SkinnedMeshRenderer meshRenderer;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;

        [Header("Movement")]
        [SerializeField] MovementSettings movementSettings;
        public MovementSettings MovementSettings => movementSettings;

        [SerializeField] MovementSettings movementAimingSettings;
        public MovementSettings MovementAimingSettings => movementAimingSettings;

        [Header("Hands Rig")]
        [SerializeField] TwoBoneIKConstraint leftHandRig;
        public TwoBoneIKConstraint LeftHandRig => leftHandRig;

        [SerializeField] Vector3 leftHandExtraRotation;
        public Vector3 LeftHandExtraRotation => leftHandExtraRotation;

        [SerializeField] TwoBoneIKConstraint rightHandRig;
        public TwoBoneIKConstraint RightHandRig => rightHandRig;

        [SerializeField] Vector3 rightHandExtraRotation;
        public Vector3 RightHandExtraRotation => rightHandExtraRotation;

        [Header("Weapon")]
        [SerializeField] Transform weaponsTransform;

        [SerializeField] Transform minigunHolderTransform;
        public Transform MinigunHolderTransform => minigunHolderTransform;

        [SerializeField] Transform shootGunHolderTransform;
        public Transform ShootGunHolderTransform => shootGunHolderTransform;

        [SerializeField] Transform rocketHolderTransform;
        public Transform RocketHolderTransform => rocketHolderTransform;

        [SerializeField] Transform teslaHolderTransform;
        public Transform TeslaHolderTransform => teslaHolderTransform;

        [Space]
        [SerializeField] Rig mainRig;
        [SerializeField] Transform leftHandController;
        [SerializeField] Transform rightHandController;

        protected CharacterBehaviour characterBehaviour;
        protected CharacterAnimationHandler animationHandler;

        protected Material characterMaterial;
        public Material CharacterMaterial => characterMaterial;

        private int animatorShootingLayerIndex;

        private AnimatorOverrideController animatorOverrideController;

        private TweenCase rigWeightCase;

        protected RagdollBehavior ragdoll;

        public virtual void Initialise(CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;

            ragdoll = new RagdollBehavior();
            ragdoll.Initialise(characterAnimator.transform);

            animationHandler = characterAnimator.GetComponent<CharacterAnimationHandler>();
            animationHandler.Inititalise(characterBehaviour);

            animatorOverrideController = new AnimatorOverrideController(characterAnimator.runtimeAnimatorController);
            characterAnimator.runtimeAnimatorController = animatorOverrideController;

            characterMaterial = meshRenderer.sharedMaterial;

            animatorShootingLayerIndex = characterAnimator.GetLayerIndex("Shooting");
        }

        public abstract void OnMovingStarted();
        public abstract void OnMovingStoped();
        public abstract void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound);

        TweenCase weaponsTransformCase;
        TweenCase leftHandCase;
        TweenCase rightHandCase;

        private Vector3 saveWeaponsPosition;
        private Vector3 saveLeftHandPosition;
        private Vector3 saveRightHandPosition;

        public virtual void OnDeath() { }

        public void Jump()
        {
            characterAnimator.SetTrigger(JUMP_ANIMATION_HASH);

            rigWeightCase.KillActive();
            mainRig.weight = 0f;
        }

        public void Grunt()
        {
            characterAnimator.SetTrigger(GRUNT_ANIMATION_HASH);

            var strength = 0.1f;
            var durationIn = 0.1f;
            var durationOut = 0.15f;

            if (weaponsTransformCase.KillActive()) weaponsTransform.localPosition = saveWeaponsPosition;
            if (leftHandCase.KillActive()) leftHandController.localPosition = saveLeftHandPosition;
            if (rightHandCase.KillActive()) rightHandController.localPosition = saveRightHandPosition;

            saveWeaponsPosition = weaponsTransform.localPosition;
            saveLeftHandPosition = leftHandController.localPosition;
            saveRightHandPosition = rightHandController.localPosition;

            weaponsTransformCase = weaponsTransform.DOMoveY(weaponsTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                weaponsTransformCase = weaponsTransform.DOMoveY(weaponsTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            leftHandCase = leftHandController.DOMoveY(leftHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                leftHandCase = leftHandController.DOMoveY(leftHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            rightHandCase = rightHandController.DOMoveY(rightHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                rightHandCase = rightHandController.DOMoveY(rightHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });
        }

        public void EnableRig()
        {
            rigWeightCase = Tween.DoFloat(0, 1, 0.2f, (value) => mainRig.weight = value);
        }

        public abstract void CustomFixedUpdate();

        public void SetShootingAnimation(AnimationClip animationClip)
        {
            animatorOverrideController["Shot"] = animationClip;
        }

        public void OnShoot()
        {
            characterAnimator.Play(ANIMATION_SHOT_HASH, animatorShootingLayerIndex, 0);
        }

        public void PlayHitAnimation()
        {
            characterAnimator.SetTrigger(ANIMATION_HIT_HASH);
        }

        public void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void PlayUpgradeParticle()
        {
            ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + new Vector3(0, 0.5f, 0)).SetScale((5).ToVector3());
            particleCase.ParticleSystem.transform.rotation = CameraController.MainCamera.transform.rotation;
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        public void EnableRagdoll()
        {
            mainRig.weight = 0.0f;

            characterAnimator.enabled = false;

            characterBehaviour.Weapon.gameObject.SetActive(false);

            ragdoll?.ActivateWithForce(transform.position + transform.forward, 700, 100);
        }

        public void DisableRagdoll()
        {
            ragdoll?.Disable();

            mainRig.weight = 1.0f;

            characterBehaviour.Weapon.gameObject.SetActive(true);
            characterAnimator.enabled = true;
        }

        public abstract void Unload();

        public abstract void Reload();

        public abstract void Disable();

        public abstract void Activate();

#if UNITY_EDITOR
        [Button("Prepare Model")]
        public void PrepareModel()
        {
            // Get animator component
            Animator tempAnimator = characterAnimator;

            if (tempAnimator != null)
            {
                if (tempAnimator.avatar != null && tempAnimator.avatar.isHuman)
                {
                    // Initialise rig
                    RigBuilder rigBuilder = tempAnimator.GetComponent<RigBuilder>();
                    if (rigBuilder == null)
                    {
                        rigBuilder = tempAnimator.gameObject.AddComponent<RigBuilder>();

                        GameObject rigObject = new GameObject("Main Rig");
                        rigObject.transform.SetParent(tempAnimator.transform);
                        rigObject.transform.ResetLocal();

                        Rig rig = rigObject.AddComponent<Rig>();

                        mainRig = rig;

                        rigBuilder.layers.Add(new RigLayer(rig, true));

                        // Left hand rig
                        GameObject leftHandRigObject = new GameObject("Left Hand Rig");
                        leftHandRigObject.transform.SetParent(rigObject.transform);
                        leftHandRigObject.transform.ResetLocal();

                        GameObject leftHandControllerObject = new GameObject("Controller");
                        leftHandControllerObject.transform.SetParent(leftHandRigObject.transform);
                        leftHandControllerObject.transform.ResetLocal();

                        leftHandController = leftHandControllerObject.transform;

                        Transform leftHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                        leftHandControllerObject.transform.position = leftHandBone.position;
                        leftHandControllerObject.transform.rotation = leftHandBone.rotation;

                        TwoBoneIKConstraint leftHandRig = leftHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        leftHandRig.data.target = leftHandControllerObject.transform;
                        leftHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                        leftHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        leftHandRig.data.tip = leftHandBone;

                        // Right hand rig
                        GameObject rightHandRigObject = new GameObject("Right Hand Rig");
                        rightHandRigObject.transform.SetParent(rigObject.transform);
                        rightHandRigObject.transform.ResetLocal();

                        GameObject rightHandControllerObject = new GameObject("Controller");
                        rightHandControllerObject.transform.SetParent(rightHandRigObject.transform);
                        rightHandControllerObject.transform.ResetLocal();

                        rightHandController = rightHandControllerObject.transform;

                        Transform rightHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.RightHand);
                        rightHandControllerObject.transform.position = rightHandBone.position;
                        rightHandControllerObject.transform.rotation = rightHandBone.rotation;

                        TwoBoneIKConstraint rightHandRig = rightHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        rightHandRig.data.target = rightHandControllerObject.transform;
                        rightHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                        rightHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        rightHandRig.data.tip = rightHandBone;

                        this.leftHandRig = leftHandRig;
                        this.rightHandRig = rightHandRig;
                    }

                    // Prepare ragdoll
                    RagdollHelper.CreateRagdoll(tempAnimator, 60, 1, LayerMask.NameToLayer("Ragdoll"));

                    movementSettings.RotationSpeed = 8;
                    movementSettings.MoveSpeed = 5;
                    movementSettings.Acceleration = 781.25f;
                    movementSettings.AnimationMultiplier = new DuoFloat(0, 1.4f);

                    movementAimingSettings.RotationSpeed = 8;
                    movementAimingSettings.MoveSpeed = 4.375f;
                    movementAimingSettings.Acceleration = 781.25f;
                    movementAimingSettings.AnimationMultiplier = new DuoFloat(0, 1.2f);

                    CharacterAnimationHandler tempAnimationHandler = tempAnimator.GetComponent<CharacterAnimationHandler>();
                    if (tempAnimationHandler == null)
                        tempAnimator.gameObject.AddComponent<CharacterAnimationHandler>();

                    // Create weapon holders
                    GameObject weaponHolderObject = new GameObject("Weapons");
                    weaponHolderObject.transform.SetParent(tempAnimator.transform);
                    weaponHolderObject.transform.ResetLocal();

                    weaponsTransform = weaponHolderObject.transform;

                    // Minigun
                    GameObject miniGunHolderObject = new GameObject("Minigun Holder");
                    miniGunHolderObject.transform.SetParent(weaponsTransform);
                    miniGunHolderObject.transform.ResetLocal();
                    miniGunHolderObject.transform.localPosition = new Vector3(0.204f, 0.7f, 0.375f);

                    minigunHolderTransform = miniGunHolderObject.transform;

                    // Shotgun
                    GameObject shotgunHolderObject = new GameObject("Shotgun Holder");
                    shotgunHolderObject.transform.SetParent(weaponsTransform);
                    shotgunHolderObject.transform.ResetLocal();
                    shotgunHolderObject.transform.localPosition = new Vector3(0.22f, 0.6735f, 0.23f);

                    shootGunHolderTransform = shotgunHolderObject.transform;

                    // Rocket
                    GameObject rocketHolderObject = new GameObject("Rocket Holder");
                    rocketHolderObject.transform.SetParent(weaponsTransform);
                    rocketHolderObject.transform.ResetLocal();
                    rocketHolderObject.transform.localPosition = new Vector3(0.234f, 0.726f, 0.369f);
                    rocketHolderObject.transform.localRotation = Quaternion.Euler(-23.68f, -4.74f, 7.92f);

                    rocketHolderTransform = rocketHolderObject.transform;

                    // Tesla
                    GameObject teslaHolderObject = new GameObject("Tesla Holder");
                    teslaHolderObject.transform.SetParent(weaponsTransform);
                    teslaHolderObject.transform.ResetLocal();
                    teslaHolderObject.transform.localPosition = new Vector3(0.213f, 0.783f, 0.357f);

                    teslaHolderTransform = teslaHolderObject.transform;

                    // Initialise mesh renderer
                    meshRenderer = tempAnimator.transform.GetComponentInChildren<SkinnedMeshRenderer>();

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                else
                {
                    Debug.LogError("Avatar is missing or type isn't humanoid!");
                }
            }
            else
            {
                Debug.LogWarning("Animator component can't be found!");
            }
        }
#endif
    }
}