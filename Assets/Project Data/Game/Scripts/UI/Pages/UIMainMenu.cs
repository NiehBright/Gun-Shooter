using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        [SerializeField] ExperienceUIController experienceUIController;
        public ExperienceUIController ExperienceUIController => experienceUIController;

        [SerializeField] LevelProgressionPanel levelProgressionPanel;
        public LevelProgressionPanel LevelProgressionPanel => levelProgressionPanel;

        [Space]
        [SerializeField] GameObject areaAndPowerPanel;
        [SerializeField] TextMeshProUGUI areaText;
        [SerializeField] TextMeshProUGUI recomendedPowerText;

        [Space]
        [SerializeField] Button settingsButton;

        [Space]
        [SerializeField] GameObject tapToPlayObject;
        [SerializeField] Button tapToPlayButton;
        [SerializeField] RectTransform bottomPanelRectTransform;

        [Space]
        [SerializeField] CharacterTab characterTab;
        [SerializeField] WeaponTab weaponTab;

        [Space]
        [SerializeField] OverlayUI overlayUI;

        [Space]
        [SerializeField] GameObject dotsBackPrefab;
        public static Canvas DotsBackground { get; private set; }

        [Header("No Ads Button")]
        [SerializeField] bool useNoAdsButton = true;
        [SerializeField] Button noAdsButton;

        public CharacterTab CharacterTab => characterTab;
        public WeaponTab WeaponTab => weaponTab;

        private CharacterUpgradeTutorial characterUpgradeTutorial;
        private WeaponUpgradeTutorial weaponUpgradeTutorial;

        private RectTransform noAdsRectTransform;

        private UIGamepadButton noAdsGamepadButton;
        public UIGamepadButton NoAdsGamepadButton => noAdsGamepadButton;

        private UIGamepadButton settingsGamepadButton;
        public UIGamepadButton SettingsGamepadButton => settingsGamepadButton;

        private UIGamepadButton playGamepadButton;
        public UIGamepadButton PlayGamepadButton => playGamepadButton;

        public static bool DontFadeRevealNextTime { get; set; }

        [Header("User Profile Header UI")]
        [SerializeField] Button profileAvatarButton;
        [SerializeField] Image profileAvatarIcon;
        [SerializeField] TextMeshProUGUI profileNameText;

        [Header("User Profile Popup UI")]
        [SerializeField] GameObject profilePopupObject;
        [SerializeField] Button profilePopupCloseButton;
        [SerializeField] Button profilePopupBlockerButton;
        [SerializeField] Image profilePopupAvatarIcon;
        [SerializeField] TextMeshProUGUI profilePopupNameText;
        [SerializeField] TMP_InputField profilePopupNameInputField;
        [SerializeField] Button profilePopupEditNameButton;
        [SerializeField] TextMeshProUGUI profilePopupDmgText;
        [SerializeField] TextMeshProUGUI profilePopupHpText;

        #region UI Page

        public override void Initialise()
        {
            if(useNoAdsButton)
            {
                noAdsRectTransform = (RectTransform)noAdsButton.transform;
                noAdsButton.onClick.AddListener(() => OnNoAdsButtonClicked());
                noAdsButton.gameObject.SetActive(true);

                noAdsGamepadButton = noAdsButton.GetComponent<UIGamepadButton>();
            }
            else
            {
                noAdsButton.gameObject.SetActive(false);
            }

            settingsGamepadButton = settingsButton.GetComponent<UIGamepadButton>();
            playGamepadButton = tapToPlayButton.GetComponent<UIGamepadButton>();

            levelProgressionPanel.Initialise();

            characterTab.Initialise();
            weaponTab.Initialise();

            if (overlayUI == null)
            {
                overlayUI = FindAnyObjectByType<OverlayUI>();
            }

            if (overlayUI != null)
            {
                overlayUI.Initialise();
            }
            else
            {
                Debug.LogWarning("[UIMainMenu] overlayUI is null and could not be found in the scene!");
            }

            // Create tutorial components
            if(TutorialController.ActivateCharacterTutorial)
            {
                characterUpgradeTutorial = new CharacterUpgradeTutorial();
                TutorialController.ActivateTutorial(characterUpgradeTutorial);
            }

            if(TutorialController.ActivateWeaponTutorial)
            {
                weaponUpgradeTutorial = new WeaponUpgradeTutorial();
                TutorialController.ActivateTutorial(weaponUpgradeTutorial);
            }

            DotsBackground = Instantiate(dotsBackPrefab).GetComponent<Canvas>();
            DotsBackground.worldCamera = Camera.main;
            DotsBackground.planeDistance = CameraController.GetCamera(CameraType.Menu).VirtualCamera.m_Lens.FarClipPlane - 0.1f;

            if (UIController.IsTablet)
            {
                var scrollSize = bottomPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                bottomPanelRectTransform.sizeDelta = scrollSize;
            }

            InitUserProfile();
        }

        private void InitUserProfile()
        {
            if (profileAvatarButton != null)
            {
                profileAvatarButton.onClick.AddListener(ShowProfilePopup);
            }

            if (profilePopupCloseButton != null)
            {
                profilePopupCloseButton.onClick.AddListener(HideProfilePopup);
            }

            if (profilePopupBlockerButton != null)
            {
                profilePopupBlockerButton.onClick.AddListener(HideProfilePopup);
            }

            if (profilePopupEditNameButton != null)
            {
                profilePopupEditNameButton.onClick.AddListener(OnEditProfileNameClicked);
            }

            if (profilePopupNameInputField != null)
            {
                profilePopupNameInputField.onEndEdit.AddListener(OnProfileNameInputEndEdit);
            }

            CharactersController.OnCharacterSelectedEvent += (charType, character) => UpdateProfileUI();

            UpdateProfileUI();
        }

        public void UpdateProfileUI()
        {
            string userName = PlayerPrefs.GetString("PlayerProfileName", "User Name");

            if (profileNameText != null)
            {
                profileNameText.text = userName;
            }

            if (profilePopupNameText != null)
            {
                profilePopupNameText.text = userName;
            }

            if (profilePopupNameInputField != null)
            {
                profilePopupNameInputField.text = userName;
            }

            var character = CharactersController.SelectedCharacter;
            if (character != null)
            {
                var sprite = character.GetCurrentStage().PreviewSprite;
                if (profileAvatarIcon != null && sprite != null) profileAvatarIcon.sprite = sprite;
                if (profilePopupAvatarIcon != null && sprite != null) profilePopupAvatarIcon.sprite = sprite;
            }
        }

        private void ShowProfilePopup()
        {
            UpdateProfileUI();

            float totalHP = 100f;
            float totalDmg = 100f;

            if (CharactersController.SelectedCharacter != null)
            {
                var character = CharactersController.SelectedCharacter;
                if (character.Upgrades != null && character.Save != null && character.Save.UpgradeLevel < character.Upgrades.Length)
                {
                    var charStats = character.Upgrades[character.Save.UpgradeLevel].Stats;
                    if (charStats != null)
                    {
                        float charHP = charStats.BaseHealth;
                        var bonusStats = EquipmentController.GetTotalBonusStats();
                        totalHP = charHP + bonusStats.bonusHP;
                    }
                }
            }

            totalDmg = EquipmentController.GetTotalPlayerDamage();

            if (profilePopupHpText != null)
            {
                profilePopupHpText.text = Mathf.RoundToInt(totalHP).ToString();
            }

            if (profilePopupDmgText != null)
            {
                profilePopupDmgText.text = Mathf.RoundToInt(totalDmg).ToString();
            }

            if (profilePopupObject != null)
            {
                profilePopupObject.SetActive(true);
            }

            if (profilePopupNameInputField != null)
            {
                profilePopupNameInputField.gameObject.SetActive(false);
            }

            if (profilePopupNameText != null)
            {
                profilePopupNameText.gameObject.SetActive(true);
            }
        }

        private void HideProfilePopup()
        {
            if (profilePopupObject != null)
            {
                profilePopupObject.SetActive(false);
            }
        }

        private void OnEditProfileNameClicked()
        {
            if (profilePopupNameInputField != null && profilePopupNameText != null)
            {
                profilePopupNameText.gameObject.SetActive(false);
                profilePopupNameInputField.gameObject.SetActive(true);
                profilePopupNameInputField.text = PlayerPrefs.GetString("PlayerProfileName", "User Name");
                profilePopupNameInputField.ActivateInputField();
            }
        }

        private void OnProfileNameInputEndEdit(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                newName = "User Name";
            }

            PlayerPrefs.SetString("PlayerProfileName", newName);
            PlayerPrefs.Save();

            UpdateProfileUI();

            if (profilePopupNameInputField != null)
            {
                profilePopupNameInputField.gameObject.SetActive(false);
            }

            if (profilePopupNameText != null)
            {
                profilePopupNameText.gameObject.SetActive(true);
            }
        }

        public void UpdateLevelText()
        {
            areaText.text = LevelController.GetCurrentAreaText();
            if (recomendedPowerText != null)
            {
                recomendedPowerText.text = Mathf.RoundToInt(EquipmentController.GetTotalPlayerDamage()).ToString();
            }
        }

        private void Update()
        {
            if (recomendedPowerText != null && recomendedPowerText.gameObject.activeInHierarchy)
            {
                recomendedPowerText.text = Mathf.RoundToInt(EquipmentController.GetTotalPlayerDamage()).ToString();
            }
        }

        public override void PlayShowAnimation()
        {
            UpdateProfileUI();
            IAPManager.OnPurchaseComplete += OnPurchaseComplete;

            if (characterUpgradeTutorial != null && !characterUpgradeTutorial.IsFinished)
            {
                characterUpgradeTutorial.StartTutorial();
            }
            else
            {
                if (weaponUpgradeTutorial != null && !weaponUpgradeTutorial.IsFinished)
                {
                    weaponUpgradeTutorial.StartTutorial();
                }
            }

            OverlayUI.ShowOverlay();

            characterTab.OnWindowOpened();
            weaponTab.OnWindowOpened();

            levelProgressionPanel.Show();

            bottomPanelRectTransform.anchoredPosition = new Vector2(0, -500);
            bottomPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(() => { 
                UIController.OnPageOpened(this);

                UIGamepadButton.EnableTag(UIGamepadButtonTag.MainMenu);
            });

            tapToPlayObject.SetActive(true);

            if (!DontFadeRevealNextTime)
            {
                Overlay.Hide(0.3f, null);
            }
            else
            {
                DontFadeRevealNextTime = false;
            }

            DotsBackground.gameObject.SetActive(true);

            if (useNoAdsButton)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true);
                    noAdsRectTransform.anchoredPosition = new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y);
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(-35, noAdsRectTransform.anchoredPosition.y), 0.5f).SetEasing(Ease.Type.CubicOut);
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false);
                }
            }
        }

        public override void PlayHideAnimation()
        {
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;

            UIController.OnPageClosed(this);
            tapToPlayObject.SetActive(false);
            SettingsPanel.HidePanel(true);

            if (useNoAdsButton)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true);
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y), 0.3f).SetEasing(Ease.Type.CubicIn);
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        private void OnPurchaseComplete(ProductKeyType productKeyType)
        {
            if (productKeyType == ProductKeyType.NoAds)
            {
                noAdsRectTransform.gameObject.SetActive(false);
            }
        }

        #region Buttons
        public void OnNoAdsButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            IAPManager.BuyProduct(ProductKeyType.NoAds);
        }

        public void PlayButton()
        {
            levelProgressionPanel.Hide();

            bottomPanelRectTransform.DOAnchoredPosition(new Vector2(0, -500), 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
            {
                characterTab.OnWindowClosed();
                weaponTab.OnWindowClosed();
            });

            Overlay.Show(0.3f, () =>
            {
                LevelController.OnGameStarted();
                AudioController.PlaySound(AudioController.Sounds.buttonSound);

                Overlay.Hide(0.3f, null);
            });

            SettingsPanel.HidePanel(true);
        }
        #endregion
    }
}