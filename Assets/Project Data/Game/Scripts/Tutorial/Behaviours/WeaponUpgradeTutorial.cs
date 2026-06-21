using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponUpgradeTutorial : ITutorial
    {
        private const WeaponType FIRST_WEAPON_TYPE = WeaponType.Minigun;

        public TutorialID TutorialID => TutorialID.WeaponUpgrade;

        private const int STEP_TUTORIAL_ACTIVATED = 1;
        private const int STEP_PAGE_OPENED = 2;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private WeaponData weaponData;
        private BaseWeaponUpgrade weaponUpgrade;

        private UIMainMenu mainMenuUI;
        private UIWeaponPage weaponPageUI;

        private WeaponTab weaponTab;
        private CharacterTab characterTab;

        private bool isActive;
        private int stepNumber;

        private UIGamepadButton activatedGamepadButton; 
        private UIGamepadButton noAdsGamepadButton;
        private UIGamepadButton settingsGamepadButton;
        private UIGamepadButton playGamepadButton;

        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        public WeaponUpgradeTutorial()
        {
            TutorialController.RegisterTutorial(this);
        }

        public void Initialise()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            // Load save file
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, TutorialID.ToString()));

            weaponData = WeaponsController.GetWeaponData(FIRST_WEAPON_TYPE);
            weaponUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);

            mainMenuUI = UIController.GetPage<UIMainMenu>();
            weaponPageUI = UIController.GetPage<UIWeaponPage>();

            weaponTab = mainMenuUI.WeaponTab;
            characterTab = mainMenuUI.CharacterTab;

            noAdsGamepadButton = mainMenuUI.NoAdsGamepadButton;
            settingsGamepadButton = mainMenuUI.SettingsGamepadButton;
            playGamepadButton = mainMenuUI.PlayGamepadButton;
        }

        public void StartTutorial()
        {
            if (isActive)
                return;

            isActive = true;

            weaponTab.Disable();
            characterTab.Disable();

            UIController.OnPageOpenedEvent += OnMainMenuPageOpened;
            Control.OnInputChanged += OnInputTypeChanged;
        }

        private void OnInputTypeChanged(InputType input)
        {
            if (activatedGamepadButton != null)
                activatedGamepadButton.StopHighLight();

            TutorialCanvasController.ResetTutorialCanvas();

            if (stepNumber == STEP_TUTORIAL_ACTIVATED)
            {
                TutorialCanvasController.ActivateTutorialCanvas(weaponTab.RectTransform, false, true);

                if (input == InputType.Gamepad)
                {
                    activatedGamepadButton = weaponTab.GamepadButton;
                    if (activatedGamepadButton != null)
                        activatedGamepadButton.StartHighlight();

                    if (characterTab.GamepadButton != null)
                        characterTab.GamepadButton.SetFocus(false);

                    if (noAdsGamepadButton != null)
                        noAdsGamepadButton.SetFocus(false);

                    if (settingsGamepadButton != null)
                        settingsGamepadButton.SetFocus(false);

                    if (playGamepadButton != null)
                        playGamepadButton.SetFocus(false);
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(weaponTab.RectTransform.position + new Vector3(0, 0.1f, 0), TutorialCanvasController.POINTER_TOPDOWN);
                }
            }
            else if (stepNumber == STEP_PAGE_OPENED)
            {
                WeaponPanelUI weaponPanel = weaponPageUI.GetPanel(FIRST_WEAPON_TYPE);
                if (weaponPanel != null)
                {
                    TutorialCanvasController.ActivateTutorialCanvas(weaponPanel.RectTransform, true, true);

                    if (Control.InputType == InputType.Gamepad)
                    {
                        if (weaponPageUI.GamepadCloseButton != null)
                            weaponPageUI.GamepadCloseButton.SetFocus(false);

                        if (characterTab.GamepadButton != null)
                            characterTab.GamepadButton.SetFocus(false);

                        if (noAdsGamepadButton != null)
                            noAdsGamepadButton.SetFocus(false);

                        if (settingsGamepadButton != null)
                            settingsGamepadButton.SetFocus(false);

                        if (playGamepadButton != null)
                            playGamepadButton.SetFocus(false);

                        activatedGamepadButton = weaponTab.GamepadButton;
                        if (activatedGamepadButton != null)
                            activatedGamepadButton.StartHighlight();
                    }
                    else
                    {
                        TutorialCanvasController.ActivatePointer(weaponPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                    }
                }
            }
        }

        private void OnMainMenuPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType == typeof(UIMainMenu))
            {
                if (ActiveRoom.CurrentLevelIndex >= 2)
                {
                    BaseUpgradeStage stage = weaponUpgrade.NextStage;
                    if(stage != null)
                    {
                        // Player has enough money to upgrade first weapon
                        if (CurrenciesController.HasAmount(stage.CurrencyType, stage.Price))
                        {
                            UIController.OnPageOpenedEvent -= OnMainMenuPageOpened;

                            stepNumber = STEP_TUTORIAL_ACTIVATED;

                            weaponTab.Activate();
                            weaponTab.Button.onClick.AddListener(OnWeaponTabOpened);

                            TutorialCanvasController.ActivateTutorialCanvas(weaponTab.RectTransform, false, true);

                            if (Control.InputType == InputType.Gamepad)
                            {
                                activatedGamepadButton = weaponTab.GamepadButton;
                                if (activatedGamepadButton != null)
                                    activatedGamepadButton.StartHighlight();

                                if (characterTab.GamepadButton != null)
                                    characterTab.GamepadButton.SetFocus(false);

                                if (noAdsGamepadButton != null)
                                    noAdsGamepadButton.SetFocus(false);

                                if (settingsGamepadButton != null)
                                    settingsGamepadButton.SetFocus(false);

                                if (playGamepadButton != null)
                                    playGamepadButton.SetFocus(false);
                            }
                            else
                            {
                                TutorialCanvasController.ActivatePointer(weaponTab.RectTransform.position + new Vector3(0, 0.1f, 0), TutorialCanvasController.POINTER_TOPDOWN);
                            }
                        }
                        else
                        {
                            weaponTab.Disable();
                        }
                    }
                }
            }
        }

        private void OnWeaponTabOpened()
        {
            TutorialCanvasController.ResetTutorialCanvas();

            weaponTab.Button.onClick.RemoveListener(OnWeaponTabOpened);

            UIController.OnPageOpenedEvent += OnWeaponPageOpened;

            weaponPageUI.GraphicRaycaster.enabled = false;
        }

        private void OnWeaponPageOpened(UIPage page, System.Type pageType)
        {
            UIController.OnPageOpenedEvent -= OnWeaponPageOpened;

            WeaponPanelUI weaponPanel = weaponPageUI.GetPanel(FIRST_WEAPON_TYPE);
            if (weaponPanel != null)
            {
                stepNumber = STEP_PAGE_OPENED;

                TutorialCanvasController.ActivateTutorialCanvas(weaponPanel.RectTransform, true, true);

                if (Control.InputType == InputType.Gamepad)
                {
                    if (weaponPageUI.GamepadCloseButton != null)
                        weaponPageUI.GamepadCloseButton.SetFocus(false);

                    if (characterTab.GamepadButton != null)
                        characterTab.GamepadButton.SetFocus(false);

                    if (noAdsGamepadButton != null)
                        noAdsGamepadButton.SetFocus(false);

                    if (settingsGamepadButton != null)
                        settingsGamepadButton.SetFocus(false);

                    if (playGamepadButton != null)
                        playGamepadButton.SetFocus(false);

                    activatedGamepadButton = weaponTab.GamepadButton;
                    if (activatedGamepadButton != null)
                        activatedGamepadButton.StartHighlight();
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(weaponPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                }

                WeaponsController.OnWeaponUpgraded += OnWeaponUpgraded;

                if(WeaponsController.IsTutorialWeaponUpgraded())
                {
                    OnWeaponUpgraded();
                }
            }

            weaponPageUI.GraphicRaycaster.enabled = true;
        }

        private void OnWeaponUpgraded()
        {
            WeaponsController.OnWeaponUpgraded -= OnWeaponUpgraded;

            TutorialCanvasController.ResetTutorialCanvas();

            if (Control.InputType == InputType.Gamepad)
            {
                if(weaponPageUI.GamepadCloseButton != null)
                    weaponPageUI.GamepadCloseButton.SetFocus(true);

                if (characterTab.GamepadButton != null)
                    characterTab.GamepadButton.SetFocus(true);
				
                if (weaponTab.GamepadButton != null)
                    weaponTab.GamepadButton.SetFocus(true);

                if (noAdsGamepadButton != null)
                    noAdsGamepadButton.SetFocus(true);

                if (settingsGamepadButton != null)
                    settingsGamepadButton.SetFocus(true);

                if (playGamepadButton != null)
                    playGamepadButton.SetFocus(true);

                if (activatedGamepadButton != null)
                    activatedGamepadButton.StopHighLight();
            }
			
			weaponTab.Activate();
			characterTab.Activate();

            FinishTutorial();
        }

        public void FinishTutorial()
        {
            saveData.isFinished = true;
        }

        public void Unload()
        {

        }
    }
}