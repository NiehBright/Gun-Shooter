using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    [DefaultExecutionOrder(100)]
    public class CameraController : MonoBehaviour
    {
        private const int ACTIVE_CAMERA_PRIORITY = 100;
        private const int UNACTIVE_CAMERA_PRIORITY = 0;

        private static CameraController cameraController;

        [SerializeField] CinemachineBrain cameraBrain;
        [SerializeField] CameraType firstCamera;

        [Space]
        [SerializeField] VirtualCameraCase[] virtualCameras;

        [Header("Forward Shift")]
        [SerializeField] float forwardX = 4f;
        [SerializeField] float forwardZ = 1f;
        [SerializeField] float forwardLerpMultiplier = 4f;

        [Header("Enemy Target Shift")]
        [SerializeField] float enemyShiftX = 4f;
        [SerializeField] float enemyShiftZ = 1f;
        [SerializeField] float enemyShiftLerpMultiplier = 4f;

        [Header("Character Selection Camera")]
        [SerializeField] float selectionDistance = 2.2f;
        [SerializeField] float selectionHorizontalOffset = 0.75f;
        [SerializeField] float selectionHeight = 1.35f;
        [SerializeField] float selectionLookAtHeight = 1.0f;

        private static Dictionary<CameraType, int> virtualCamerasLink;

        private static Camera mainCamera;
        public static Camera MainCamera => mainCamera;

        private static Transform mainTarget;
        public static Transform MainTarget => mainTarget;

        private static VirtualCameraCase activeVirtualCamera;
        public static VirtualCameraCase ActiveVirtualCamera => activeVirtualCamera;

        private static Transform InternalTarget { get; set; }

        private static bool cameraShiftEnabled = true;

        private Vector3 forward = Vector3.zero;
        private static Vector3 enemyDirection = Vector3.zero;
        private static BaseEnemyBehavior targetEnemy;
        private TweenCase selectionTweenCase;

        private void Awake()
        {
            cameraController = this;

            // Get camera component
            mainCamera = GetComponent<Camera>();

            // Initialise cameras link
            virtualCamerasLink = new Dictionary<CameraType, int>();
            for(int i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Initialise();
                virtualCamerasLink.Add(virtualCameras[i].CameraType, i);
            }

            // Disable camera brain
            cameraController.cameraBrain.enabled = false;

            EnableCamera(firstCamera);

            InternalTarget = new GameObject("[Internal Camera Target]").transform;

            mainTarget = InternalTarget;
        }

        public static void SetMainTarget(Transform target)
        {
            // Link target
            mainTarget = target;
            InternalTarget.position = mainTarget.position;

            cameraController.cameraBrain.enabled = false;

            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Follow = InternalTarget;
                cameraController.virtualCameras[i].VirtualCamera.LookAt = InternalTarget;
            }

            cameraController.cameraBrain.transform.position = target.position;
            cameraController.cameraBrain.enabled = true;
        }

        public static void SetEnemyTarget(BaseEnemyBehavior enemy)
        {
            targetEnemy = enemy;
        }

        public static void SetCameraShiftState(bool state)
        {
            cameraShiftEnabled = state;
        }

        private void LateUpdate()
        {
            if (cameraShiftEnabled)
            {
                var z = mainTarget.forward.z * forwardZ;
                var x = mainTarget.forward.x * forwardX;

                forward = Vector3.Lerp(forward, new Vector3(x, 0, z), Time.deltaTime * forwardLerpMultiplier);

                var currentEnemyDirection = targetEnemy ? (targetEnemy.transform.position - mainTarget.position).normalized : Vector3.zero;

                currentEnemyDirection.x *= enemyShiftX;
                currentEnemyDirection.z *= enemyShiftZ;

                enemyDirection = Vector3.Lerp(enemyDirection, currentEnemyDirection, Time.deltaTime * enemyShiftLerpMultiplier);

                InternalTarget.position = mainTarget.position + forward + enemyDirection;
            }
            else
            {
                InternalTarget.position = mainTarget.position;
            }
        }

        public static VirtualCameraCase GetCamera(CameraType cameraType)
        {
            return cameraController.virtualCameras[virtualCamerasLink[cameraType]];
        }

        public static void EnableCamera(CameraType cameraType)
        {
            if (activeVirtualCamera != null && activeVirtualCamera.CameraType == cameraType)
                return;

            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                cameraController.virtualCameras[i].VirtualCamera.Priority = UNACTIVE_CAMERA_PRIORITY;
            }

            activeVirtualCamera = cameraController.virtualCameras[virtualCamerasLink[cameraType]];
            activeVirtualCamera.VirtualCamera.Priority = ACTIVE_CAMERA_PRIORITY;
        }

        public static void EnterCharacterSelection(Vector3 playerPos, Vector3 playerForward, Vector3 playerRight, Vector3 playerUp)
        {
            if (cameraController == null) return;

            // Tam thoi tat Cinemachine de di chuyen camera tu do
            cameraController.cameraBrain.enabled = false;

            // Tinh toan vi tri camera (dung truoc mat va lech phai nhan vat de nhan vat lech trai khung hinh)
            Vector3 targetPos = playerPos + playerForward * cameraController.selectionDistance 
                                          + playerRight * cameraController.selectionHorizontalOffset 
                                          + playerUp * cameraController.selectionHeight;
            Vector3 lookAtTarget = playerPos + playerUp * cameraController.selectionLookAtHeight;
            Quaternion targetRot = Quaternion.LookRotation((lookAtTarget - targetPos).normalized);

            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            cameraController.selectionTweenCase.KillActive();
            cameraController.selectionTweenCase = Tween.DoFloat(0f, 1f, 0.5f, (float t) =>
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            });
        }

        public static void ExitCharacterSelection()
        {
            if (cameraController == null) return;

            cameraController.selectionTweenCase.KillActive();

            // Bat lai Cinemachine de no tu dong blend muot ma ve camera sanh cho
            cameraController.cameraBrain.enabled = true;
        }
    }
}