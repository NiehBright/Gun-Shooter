using UnityEngine;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponShowcaseBehaviour : MonoBehaviour
    {
        private static WeaponShowcaseBehaviour instance;
        public static WeaponShowcaseBehaviour Instance => instance;

        [Header("Floating Settings")]
        [SerializeField] private float bobbingSpeed = 2.0f;
        [SerializeField] private float bobbingAmount = 0.03f;
        [SerializeField] private float idleRotationSpeed = 0f; // Súng nằm yên ở giữa màn hình theo yêu cầu

        [Header("Showcase Light")]
        [SerializeField] private Light showcaseLight;

        private GameObject currentGunObject;
        private Transform pivotTransform;
        private Vector3 baseLocalPosition;
        private float currentRotationY = 75f; // Góc nghiêng 3/4 nhìn rõ thân súng 3D
        private float currentPitch = 0f;
        private bool isDragging = false;
        private TweenCase scaleTweenCase;
        private TweenCase pitchTweenCase;

        private const float RESTING_YAW = 75f;

        private void Awake()
        {
            instance = this;
            baseLocalPosition = transform.position;

            // Tao Pivot trung gian de xoay quanh tam hinh hoc
            GameObject pivotObj = new GameObject("[Showcase Pivot]");
            pivotObj.transform.SetParent(transform, false);
            pivotTransform = pivotObj.transform;
            pivotTransform.localRotation = Quaternion.Euler(currentPitch, currentRotationY, 0f);

            // Tao anh sang soft point light cho showcase neu chua co
            if (showcaseLight == null)
            {
                GameObject lightObj = new GameObject("[Showcase Light]");
                lightObj.transform.SetParent(transform, false);
                lightObj.transform.localPosition = new Vector3(0f, 1.2f, 1.2f);
                showcaseLight = lightObj.AddComponent<Light>();
                showcaseLight.type = LightType.Point;
                showcaseLight.range = 5f;
                showcaseLight.intensity = 1.4f;
                showcaseLight.color = new Color(0.9f, 0.95f, 1f);
                showcaseLight.shadows = LightShadows.None;
            }
        }

        public void SetBasePosition(Vector3 position)
        {
            baseLocalPosition = position;
            transform.position = position;
        }

        public void SetShowcaseTransform(Vector3 position, Quaternion rotation)
        {
            baseLocalPosition = position;
            transform.position = position;
            transform.rotation = rotation;
        }

        public void DisplayWeapon(WeaponData weaponData, BaseWeaponUpgrade upgrade)
        {
            if (upgrade == null) return;

            BaseWeaponUpgradeStage currentStage = upgrade.GetCurrentStage();
            if (currentStage == null || currentStage.WeaponPrefab == null) return;

            // Xoa súng cu neu co
            if (currentGunObject != null)
            {
                Destroy(currentGunObject);
                currentGunObject = null;
            }

            // Khoi tao sung moi tu Addressables / Prefab
            if (currentStage.WeaponPrefab.RuntimeKeyIsValid())
            {
                currentGunObject = currentStage.WeaponPrefab.InstantiateAsync().WaitForCompletion();
                if (currentGunObject != null)
                {
                    currentGunObject.SetActive(true);
                    currentGunObject.transform.SetParent(pivotTransform, false);

                    // Tat va go cac component logic chien dau tranh anh huong gameplay
                    BaseGunBehavior gunBehavior = currentGunObject.GetComponent<BaseGunBehavior>();
                    if (gunBehavior != null) gunBehavior.enabled = false;

                    Collider[] colliders = currentGunObject.GetComponentsInChildren<Collider>(true);
                    foreach (var col in colliders) col.enabled = false;

                    Rigidbody[] rbs = currentGunObject.GetComponentsInChildren<Rigidbody>(true);
                    foreach (var rb in rbs) rb.isKinematic = true;

                    // Ngung cac particle ban sung (neu co)
                    ParticleSystem[] particles = currentGunObject.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in particles)
                    {
                        if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }

                    // Can tam trong tam hinh hoc (Geometric Center Alignment)
                    CenterGunMesh(currentGunObject);

                    // Dat lai goc nghieng 3D dep mat
                    currentRotationY = RESTING_YAW;
                    currentPitch = 0f;
                    pivotTransform.localRotation = Quaternion.Euler(currentPitch, currentRotationY, 0f);

                    // Hieu ung Pop-in mượt mà
                    scaleTweenCase.KillActive();
                    pivotTransform.localScale = Vector3.zero;
                    scaleTweenCase = pivotTransform.DOScale(Vector3.one * 1.15f, 0.35f)
                        .SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
                }
            }
        }

        private void CenterGunMesh(GameObject gun)
        {
            gun.transform.localPosition = Vector3.zero;
            gun.transform.localRotation = Quaternion.identity;

            Renderer[] renderers = gun.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                // Chuyen doi toa do tam bounds ve local space cua pivot
                Vector3 localCenter = pivotTransform.InverseTransformPoint(bounds.center);
                gun.transform.localPosition = -localCenter;
            }
            else
            {
                gun.transform.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            if (pivotTransform == null) return;

            // Hieu ung dap dềnh nhe nhang (Floating / Bobbing len xuong em ai)
            float bobOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = baseLocalPosition + Vector3.up * bobOffset;

            // Súng nằm yên ở giữa màn hình, chi xoay khi nguoi dung vuot drag
            if (!isDragging)
            {
                if (idleRotationSpeed > 0.01f)
                {
                    currentRotationY += idleRotationSpeed * Time.deltaTime;
                }
                pivotTransform.localRotation = Quaternion.Euler(currentPitch, currentRotationY, 0f);
            }
        }

        public void OnDrag(Vector2 delta)
        {
            isDragging = true;
            pitchTweenCase.KillActive();

            // Drag X xoay truc Y
            currentRotationY -= delta.x * 0.45f;
            // Drag Y nghieng goc pitch nhe (-20 den +20 do)
            currentPitch = Mathf.Clamp(currentPitch + delta.y * 0.2f, -20f, 20f);

            if (pivotTransform != null)
            {
                pivotTransform.localRotation = Quaternion.Euler(currentPitch, currentRotationY, 0f);
            }
        }

        public void OnEndDrag()
        {
            isDragging = false;

            // Luot nhe ve lai goc nghieng dep 75 do
            pitchTweenCase.KillActive();
            float startPitch = currentPitch;
            float startYaw = currentRotationY;
            pitchTweenCase = Tween.DoFloat(0f, 1f, 0.4f, (float t) =>
            {
                currentPitch = Mathf.Lerp(startPitch, 0f, t);
                currentRotationY = Mathf.Lerp(startYaw, RESTING_YAW, t);
                if (pivotTransform != null)
                {
                    pivotTransform.localRotation = Quaternion.Euler(currentPitch, currentRotationY, 0f);
                }
            }).SetEasing(Ease.Type.QuadOut);
        }

        public void Clear()
        {
            scaleTweenCase.KillActive();
            pitchTweenCase.KillActive();

            if (currentGunObject != null)
            {
                Destroy(currentGunObject);
                currentGunObject = null;
            }
        }

        private void OnDestroy()
        {
            Clear();
            if (instance == this) instance = null;
        }
    }
}
