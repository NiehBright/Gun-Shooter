using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIWeaponPage : UIUpgradesAbstractPage<WeaponPanelUI, WeaponType>
    {
        private WeaponsController weaponController;

        protected override int SelectedIndex => Mathf.Clamp(WeaponsController.SelectedWeaponIndex, 0, int.MaxValue);

        public void SetWeaponsController(WeaponsController weaponController)
        {
            this.weaponController = weaponController;
        }

        public void UpdateUI() => itemPanels.ForEach(panel => panel.UpdateUI());

        public override WeaponPanelUI GetPanel(WeaponType weaponType)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Data.Type == weaponType)
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
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Weapons);
        }

        #region UI Page

        public override void Initialise()
        {
            base.Initialise();

            for (int i = 0; i < WeaponsController.Database.Weapons.Length; i++)
            {
                var weapon = WeaponsController.Database.Weapons[i];
                var upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(weapon.UpgradeType);

                var newPanel = AddNewPanel();
                newPanel.Init(weaponController, upgrade as BaseWeaponUpgrade, weapon, i);
            }

            WeaponsController.OnWeaponUnlocked += (weapon) => UpdateUI();
            WeaponsController.OnWeaponUpgraded += UpdateUI;
        }

        public override void PlayShowAnimation()
        {
            base.PlayShowAnimation();

            UpdateUI();
            OverlayUI.ShowOverlay();
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();

            UIController.OnPageClosed(this);
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UIWeaponPage>(onFinish);
        }

        #endregion
    }
}