using UnityEngine;
using UnityEngine.EventSystems;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIDronePage : UIUpgradesAbstractPage<DronePanelUI, DroneType>, IDragHandler, IEndDragHandler
    {
        protected override int SelectedIndex => Mathf.Clamp(DronesController.SelectedDroneIndex, 0, int.MaxValue);

        public void UpdateUI() => itemPanels.ForEach(panel => panel.UpdateUI());

        public override DronePanelUI GetPanel(DroneType type)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Data.Type == type)
                    return itemPanels[i];
            }

            return null;
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        protected override void EnableGamepadButtonTag()
        {
            // Optional, could use UIGamepadButtonTag.Weapons if we don't add Drones to enum
        }

        public override void Initialise()
        {
            base.Initialise();

            // Disable ScrollRect immediately - it will be re-enabled in PlayShowAnimation
            // This prevents "Invalid AABB inAABB" errors when the page is not visible
            if (scrollView != null)
            {
                scrollView.enabled = false;
            }

            for (int i = 0; i < DronesController.Database.Drones.Length; i++)
            {
                var drone = DronesController.Database.Drones[i];
                var upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(drone.UpgradeType);

                var newPanel = AddNewPanel();
                if (newPanel == null)
                {
                    Debug.LogError("[UIDronePage] newPanel is NULL! This means the Prefab you assigned to 'Panel UI Prefab' does NOT have the 'DronePanelUI' script on it!");
                    continue;
                }
                if (drone == null) Debug.LogError("[UIDronePage] drone is NULL at index " + i);
                if (upgrade == null) Debug.LogError("[UIDronePage] upgrade is NULL for drone " + drone.Type);

                newPanel.Init(upgrade as BaseDroneUpgrade, drone, i);
            }
        }

        private Quaternion originalDroneRotation;
        private Quaternion showcaseDroneRotation;
        private TweenCase resetDroneRotationTweenCase;
        private Quaternion originalPlayerRotation;
        private Vector3 originalDronePosition;
        private bool isDroneCameraActive = false;

        public override void PlayShowAnimation()
        {
            // Enable ScrollRect when page is shown
            if (scrollView != null) scrollView.enabled = true;
            UpdateUI();
            base.PlayShowAnimation();

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                // Tat di chuyen va agent de tranh nguoi choi dieu khien nhan vat trong khi mo UI
                Control.DisableMovementControl();
                characterBehaviour.DisableAgent();

                // An nhan vat de chi hien thi moi drone
                if (characterBehaviour.Graphics != null)
                {
                    characterBehaviour.Graphics.gameObject.SetActive(false);
                }

                // Kich hoat camera bay vao Drone, va di chuyen Drone den vi tri trung tam
                if (characterBehaviour.CurrentDrone != null)
                {
                    isDroneCameraActive = true;
                    characterBehaviour.CurrentDrone.IsUIMode = true;
                    originalDroneRotation = characterBehaviour.CurrentDrone.transform.rotation;
                    showcaseDroneRotation = originalDroneRotation;
                    originalDronePosition = characterBehaviour.CurrentDrone.transform.position;
                    originalPlayerRotation = characterBehaviour.transform.rotation;

                    // Nếu Drone có CameraTarget → Camera sẽ đặt chính xác tại CameraTarget và nhìn thẳng vào Drone
                    if (characterBehaviour.CurrentDrone.CameraTarget != null)
                    {
                        Transform camTarget = characterBehaviour.CurrentDrone.CameraTarget;
                        Vector3 dronePos = characterBehaviour.CurrentDrone.transform.position;
                        Vector3 camPos = camTarget.position;

                        // Huong nhin tu CameraTarget den Drone
                        Vector3 dirToDrone = dronePos - camPos;
                        float dist = dirToDrone.magnitude;
                        Vector3 forwardDir = dist > 0.001f ? dirToDrone.normalized : Vector3.forward;

                        // Vector ben phai vuong goc voi huong nhin
                        Vector3 camRight = Vector3.Cross(Vector3.up, forwardDir).normalized;

                        // Do lech ngang de Drone hien thi o ben trai man hinh (tranh che khuat boi UI ben phai)
                        float horizontalOffset = Mathf.Max(0.6f, dist * 0.35f);

                        // Dich vi tri camera va tam nhin sang phai -> Drone se nam o nua ben trai man hinh
                        Vector3 adjustedCamPos = camPos + camRight * (horizontalOffset * 0.4f);
                        Vector3 lookAtPoint = dronePos + camRight * horizontalOffset;

                        Quaternion targetRot = Quaternion.LookRotation((lookAtPoint - adjustedCamPos).normalized);
                        CameraController.MoveCameraTo(adjustedCamPos, targetRot);
                    }
                    else
                    {
                        // Fallback: Dùng vị trí mặc định (vị trí nhân vật + offset)
                        Vector3 showcasePos = characterBehaviour.transform.position + Vector3.up * 1.0f;
                        characterBehaviour.CurrentDrone.transform.position = showcasePos;

                        Vector3 defaultCamPos = CameraController.MainCamera.transform.position;
                        Vector3 dirToCam = defaultCamPos - showcasePos;
                        dirToCam.y = 0;
                        if (dirToCam.sqrMagnitude > 0.01f)
                        {
                            Vector3 lookDir = dirToCam.normalized;
                            characterBehaviour.CurrentDrone.transform.rotation = Quaternion.LookRotation(lookDir);
                            showcaseDroneRotation = characterBehaviour.CurrentDrone.transform.rotation;
                            
                            Vector3 right = Vector3.Cross(Vector3.up, lookDir).normalized;
                            CameraController.EnterCharacterSelection(showcasePos, lookDir, right, Vector3.up);
                        }
                    }
                }

                // An UI mau tren dau nhan vat
                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.ForceDisable();
                }
            }
        }

        public override void PlayHideAnimation()
        {
            // Disable ScrollRect when page hides to prevent "Invalid AABB inAABB"
            if (scrollView != null) scrollView.enabled = false;

            resetDroneRotationTweenCase.KillActive();
            base.PlayHideAnimation();

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                // Hien lai nhan vat
                if (characterBehaviour.Graphics != null)
                {
                    characterBehaviour.Graphics.gameObject.SetActive(true);
                }

                // Khoi phuc vi tri, xoay va tra quyen kiem soat cho Cinemachine
                if (isDroneCameraActive)
                {
                    if (characterBehaviour.CurrentDrone != null)
                    {
                        characterBehaviour.CurrentDrone.transform.position = originalDronePosition;
                        characterBehaviour.CurrentDrone.transform.rotation = originalDroneRotation;
                        characterBehaviour.CurrentDrone.IsUIMode = false;
                    }
                    characterBehaviour.transform.rotation = originalPlayerRotation;
                    isDroneCameraActive = false;
                }

                // Bat lai di chuyen
                Control.EnableMovementControl();
                characterBehaviour.ActivateAgent();

                // Hien lai UI mau
                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.EnableBar(true);
                }
            }
            
            CameraController.ExitCharacterSelection();

            backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });
        }

        public void OnDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null && characterBehaviour.CurrentDrone != null && isDroneCameraActive)
            {
                resetDroneRotationTweenCase.KillActive();
                // Xoay drone theo truc Y
                float rotationSpeed = -0.5f;
                characterBehaviour.CurrentDrone.transform.Rotate(Vector3.up, eventData.delta.x * rotationSpeed, Space.World);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null && characterBehaviour.CurrentDrone != null && isDroneCameraActive)
            {
                Quaternion startRot = characterBehaviour.CurrentDrone.transform.rotation;
                resetDroneRotationTweenCase.KillActive();
                resetDroneRotationTweenCase = Tween.DoFloat(0f, 1f, 0.4f, (float t) =>
                {
                    if (characterBehaviour != null && characterBehaviour.CurrentDrone != null)
                    {
                        characterBehaviour.CurrentDrone.transform.rotation = Quaternion.Slerp(startRot, showcaseDroneRotation, t);
                    }
                }).SetEasing(Ease.Type.QuadOut);
            }
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UIDronePage>(onFinish);
        }

        private void OnEnable()
        {
            // Disable ScrollRect by default - only enable when page is actively shown
            if (scrollView != null) scrollView.enabled = false;
        }
    }
}
