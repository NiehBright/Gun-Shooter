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

        public override void PlayShowAnimation()
        {
            // Enable ScrollRect when page is shown
            if (scrollView != null) scrollView.enabled = true;
            UpdateUI();
            base.PlayShowAnimation();
        }

        public override void PlayHideAnimation()
        {
            // Disable ScrollRect when page hides to prevent "Invalid AABB inAABB"
            if (scrollView != null) scrollView.enabled = false;

            base.PlayHideAnimation();

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
