using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class CharacterUpgradeTutorial : ITutorial
    {
        private const CharacterType FIRST_CHARACTER_TYPE = CharacterType.Character_01;

        public TutorialID TutorialID => TutorialID.CharacterUpgrade;

        private const int STEP_TUTORIAL_ACTIVATED = 1;
        private const int STEP_PAGE_OPENED = 2;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private Character firstCharacter;

        private UIMainMenu mainMenuUI;
        private UICharactersPanel characterPanelUI;
        private CharacterTab characterTab;
        private WeaponTab weaponTab;

        private bool isActive;
        private int stepNumber;

        private UIGamepadButton activatedGamepadButton;
        private UIGamepadButton noAdsGamepadButton;
        private UIGamepadButton settingsGamepadButton;
        private UIGamepadButton playGamepadButton;

        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        public CharacterUpgradeTutorial()
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

            firstCharacter = CharactersController.GetCharacter(FIRST_CHARACTER_TYPE);

            mainMenuUI = UIController.GetPage<UIMainMenu>();
            characterPanelUI = UIController.GetPage<UICharactersPanel>();

            characterTab = mainMenuUI.CharacterTab;
            weaponTab = mainMenuUI.WeaponTab;

            noAdsGamepadButton = mainMenuUI.NoAdsGamepadButton;
            settingsGamepadButton = mainMenuUI.SettingsGamepadButton;
            playGamepadButton = mainMenuUI.PlayGamepadButton;
        }

        public void StartTutorial()
        {
            if (isActive)
                return;

            isActive = true;

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
                TutorialCanvasController.ActivateTutorialCanvas(mainMenuUI.CharacterTab.RectTransform, false, true);

                if (input == InputType.Gamepad)
                {
                    activatedGamepadButton = characterTab.GamepadButton;
                    activatedGamepadButton.StartHighlight();

                    if (weaponTab.GamepadButton != null)
                        weaponTab.GamepadButton.SetFocus(false);

                    if (noAdsGamepadButton != null)
                        noAdsGamepadButton.SetFocus(false);

                    if (settingsGamepadButton != null)
                        settingsGamepadButton.SetFocus(false);

                    if (playGamepadButton != null)
                        playGamepadButton.SetFocus(false);
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(mainMenuUI.CharacterTab.RectTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                }
            }
            else if(stepNumber == STEP_PAGE_OPENED)
            {
                CharacterPanelUI characterPanel = characterPanelUI.GetPanel(FIRST_CHARACTER_TYPE);
                if (characterPanel != null)
                {
                    TutorialCanvasController.ActivateTutorialCanvas(characterPanel.RectTransform, true, true);

                    if (input == InputType.Gamepad)
                    {
                        if (characterPanelUI.GamepadCloseButton != null)
                            characterPanelUI.GamepadCloseButton.SetFocus(false);

                        if (weaponTab.GamepadButton != null)
                            weaponTab.GamepadButton.SetFocus(false);

                        if (noAdsGamepadButton != null)
                            noAdsGamepadButton.SetFocus(false);

                        if (settingsGamepadButton != null)
                            settingsGamepadButton.SetFocus(false);

                        if (playGamepadButton != null)
                            playGamepadButton.SetFocus(false);

                        activatedGamepadButton = characterPanel.GamepadButton;
                        activatedGamepadButton.StartHighlight();
                    }
                    else
                    {
                        TutorialCanvasController.ActivatePointer(characterPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                    }
                }
            }
        }

        private void OnMainMenuPageOpened(UIPage page, System.Type pageType)
        {
            weaponTab.Disable();

            if (pageType == typeof(UIMainMenu))
            {
                if (ActiveRoom.CurrentLevelIndex >= 1)
                {
                    CharacterUpgrade nextStage = firstCharacter.GetNextUpgrade();
                    if(nextStage != null)
                    {
                        // Player has enough money to upgrade first character
                        if (CurrenciesController.HasAmount(nextStage.CurrencyType, nextStage.Price))
                        {
                            stepNumber = STEP_TUTORIAL_ACTIVATED;

                            UIController.OnPageOpenedEvent -= OnMainMenuPageOpened;

                            characterTab.Activate();
                            characterTab.Button.onClick.AddListener(OnCharacterTabOpened);

                            TutorialCanvasController.ActivateTutorialCanvas(mainMenuUI.CharacterTab.RectTransform, false, true);

                            if(Control.InputType == InputType.Gamepad)
                            {
                                activatedGamepadButton = characterTab.GamepadButton;
                                if (activatedGamepadButton != null)
                                    activatedGamepadButton.StartHighlight();

                                if (weaponTab.GamepadButton != null)
                                {
                                    weaponTab.GamepadButton.SetFocus(false);
                                }  

                                if (noAdsGamepadButton != null)
                                    noAdsGamepadButton.SetFocus(false);

                                if (settingsGamepadButton != null)
                                    settingsGamepadButton.SetFocus(false);

                                if (playGamepadButton != null)
                                    playGamepadButton.SetFocus(false);
                            }
                            else
                            {
                                TutorialCanvasController.ActivatePointer(mainMenuUI.CharacterTab.RectTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                            }
                        }
                        else
                        {
                            characterTab.Disable();
                        }
                    }
                }
            }
        }

        private void OnCharacterTabOpened()
        {
            if (activatedGamepadButton != null)
                activatedGamepadButton.StopHighLight();

            TutorialCanvasController.ResetTutorialCanvas();

            characterTab.Button.onClick.RemoveListener(OnCharacterTabOpened);

            characterPanelUI.GraphicRaycaster.enabled = false;

            if (Control.InputType == InputType.Gamepad)
            {
                if (characterPanelUI.GamepadCloseButton != null)
                    characterPanelUI.GamepadCloseButton.SetFocus(false);

                CharacterPanelUI characterPanel = characterPanelUI.GetPanel(FIRST_CHARACTER_TYPE);
                activatedGamepadButton = characterPanel.GamepadButton;
                if (activatedGamepadButton != null)
                    activatedGamepadButton.SetFocus(false);
            } 

            UIController.OnPageOpenedEvent += OnCharacterPageOpened;
        }

        private void OnCharacterPageOpened(UIPage page, System.Type pageType)
        {
            UIController.OnPageOpenedEvent -= OnCharacterPageOpened;

            CharacterPanelUI characterPanel = characterPanelUI.GetPanel(FIRST_CHARACTER_TYPE);
            if (characterPanel != null)
            {
                stepNumber = STEP_PAGE_OPENED;

                TutorialCanvasController.ActivateTutorialCanvas(characterPanel.RectTransform, true, true);

                if (Control.InputType == InputType.Gamepad)
                {
                    if (characterPanelUI.GamepadCloseButton != null)
                        characterPanelUI.GamepadCloseButton.SetFocus(false);

                    activatedGamepadButton = characterPanel.GamepadButton;
                    if(activatedGamepadButton != null) 
                    {
                        activatedGamepadButton.StartHighlight();
                        activatedGamepadButton.SetFocus(true);
                    }
                }
                else
                {
                    TutorialCanvasController.ActivatePointer(characterPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                }

                CharactersController.OnCharacterUpgradedEvent += OnCharacterUpgraded;
            }

            characterPanelUI.GraphicRaycaster.enabled = true;
        }

        private void OnCharacterUpgraded(CharacterType characterType, Character character)
        {
            CharactersController.OnCharacterUpgradedEvent -= OnCharacterUpgraded;

            TutorialCanvasController.ResetTutorialCanvas();

            if (Control.InputType == InputType.Gamepad)
            {
                if (characterPanelUI.GamepadCloseButton != null)
                    characterPanelUI.GamepadCloseButton.SetFocus(true);

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