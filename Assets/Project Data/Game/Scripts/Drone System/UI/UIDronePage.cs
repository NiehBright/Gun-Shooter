using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIDronePage : UIUpgradesAbstractPage<DronePanelUI, DroneType>
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
        private Quaternion originalPlayerRotation;
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

                // Kich hoat camera bay vao Drone giong y het character
                if (characterBehaviour.CurrentDrone != null)
                {
                    isDroneCameraActive = true;
                    originalDroneRotation = characterBehaviour.CurrentDrone.transform.rotation;
                    originalPlayerRotation = characterBehaviour.transform.rotation;

                    Vector3 dronePos = characterBehaviour.CurrentDrone.transform.position;
                    Vector3 defaultCamPos = CameraController.MainCamera.transform.position;
                    Vector3 dirToCam = defaultCamPos - dronePos;
                    dirToCam.y = 0;
                    if (dirToCam.sqrMagnitude > 0.01f)
                    {
                        Vector3 lookDir = dirToCam.normalized;
                        characterBehaviour.CurrentDrone.transform.rotation = Quaternion.LookRotation(lookDir);
                        
                        Vector3 right = Vector3.Cross(Vector3.up, lookDir).normalized;
                        CameraController.EnterCharacterSelection(dronePos, lookDir, right, Vector3.up);
                    }
                }
            }
        }

        public override void PlayHideAnimation()
        {
            // Disable ScrollRect when page hides to prevent "Invalid AABB inAABB"
            if (scrollView != null) scrollView.enabled = false;

            base.PlayHideAnimation();

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                // Hien lai nhan vat
                if (characterBehaviour.Graphics != null)
                {
                    characterBehaviour.Graphics.gameObject.SetActive(true);
                }

                // Khoi phuc xoay va tra quyen kiem soat cho Cinemachine
                if (isDroneCameraActive)
                {
                    if (characterBehaviour.CurrentDrone != null)
                    {
                        characterBehaviour.CurrentDrone.transform.rotation = originalDroneRotation;
                    }
                    characterBehaviour.transform.rotation = originalPlayerRotation;
                    isDroneCameraActive = false;
                }

                // Bat lai di chuyen
                Control.EnableMovementControl();
                characterBehaviour.ActivateAgent();
            }
            
            CameraController.ExitCharacterSelection();

            backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });
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
