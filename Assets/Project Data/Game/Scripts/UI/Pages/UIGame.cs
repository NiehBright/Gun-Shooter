using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] Joystick joystick;
        [SerializeField] AttackButtonBehavior attackButton;
        [SerializeField] RectTransform floatingTextHolder;

        [Header("Controls")]
        [SerializeField] Button dashButton;
        [SerializeField] Image dashCooldownOverlay;

        [Header("Skill Control")]
        [SerializeField] Button skillButton;
        [SerializeField] Image skillCooldownOverlay;
        [SerializeField] Image skillIconImage;

        [Header("Auto Shoot")]
        [SerializeField] Sprite autoShootActiveSprite;
        [SerializeField] Sprite autoShootDisableSprite;
        private Button autoShootButton;
        private Image autoShootButtonImage;

        [Space]
        [SerializeField] TextMeshProUGUI areaText;

        [Space]
        [SerializeField] Transform roomsHolder;
        [SerializeField] GameObject roomIndicatorUIPrefab;

        [Space]
        [SerializeField] Image fadeImage;
        [SerializeField] TextMeshProUGUI coinsText;

        [Header("Pause Panel")]
        [SerializeField] Button pauseButton;
        public Button PauseButton => pauseButton;

        [Space]
        [SerializeField] GameObject pausePanelObject;
        [SerializeField] CanvasGroup pausePanelCanvasGroup;
        [SerializeField] Button pauseResumeButton;
        [SerializeField] Button pauseExitButton;

        public Joystick Joystick => joystick;
        public RectTransform FloatingTextHolder => floatingTextHolder;

        private List<UIRoomIndicator> roomIndicators = new List<UIRoomIndicator>();
        private PoolGeneric<UIRoomIndicator> roomIndicatorsPool;

        private void Awake()
        {
            roomIndicatorsPool = new PoolGeneric<UIRoomIndicator>(new PoolSettings(roomIndicatorUIPrefab.name, roomIndicatorUIPrefab, 3, true, roomsHolder));

            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            pauseExitButton.onClick.AddListener(OnPauseExitButtonClicked);
            pauseResumeButton.onClick.AddListener(OnPauseResumeButtonClicked);


            if (dashButton == null)
            {
                Transform trans = FindChildRecursive(transform, "Dash Button");
                if (trans != null) dashButton = trans.GetComponent<Button>();
            }
            if (dashCooldownOverlay == null)
            {
                Transform trans = FindChildRecursive(transform, "Cooldown Overlay");
                if (trans != null) dashCooldownOverlay = trans.GetComponent<Image>();
            }

            if (dashButton != null)
            {
                dashButton.onClick.AddListener(OnDashButtonClicked);
                Debug.Log("[UIGame] Found and registered click listener for Dash Button.");
            }
            else
            {
                Debug.LogWarning("[UIGame] Dash Button could not be found recursively under UI Game canvas!");
            }

            if (autoShootButton == null)
            {
                Transform trans = FindChildRecursive(transform, "Auto Shoot Button");
                if (trans != null)
                {
                    autoShootButton = trans.GetComponent<Button>();
                    autoShootButtonImage = trans.GetComponent<Image>();
                }
            }

            if (autoShootButton != null)
            {
                autoShootButton.onClick.AddListener(OnAutoShootButtonClicked);
                Debug.Log("[UIGame] Found and registered click listener for Auto Shoot Button.");
            }

            if (skillButton == null)
            {
                Transform trans = FindChildRecursive(transform, "Skill Button");
                if (trans != null)
                {
                    skillButton = trans.GetComponent<Button>();
                    Transform iconTrans = FindChildRecursive(trans, "Icon");
                    if (iconTrans != null) skillIconImage = iconTrans.GetComponent<Image>();
                    else skillIconImage = trans.GetComponent<Image>();
                    Transform overlayTrans = FindChildRecursive(trans, "Cooldown Overlay");
                    if (overlayTrans != null) skillCooldownOverlay = overlayTrans.GetComponent<Image>();
                }
            }

            if (skillButton != null)
            {
                skillButton.onClick.AddListener(OnSkillButtonClicked);
                Debug.Log("[UIGame] Found and registered click listener for Skill Button.");
            }
        }

        private void Start()
        {
            UpdateAttackButtonVisibility();
            UpdateAutoShootButtonUI();
        }

        public void FadeAnimation(float time, float startAlpha, float targetAlpha, Ease.Type easing, SimpleCallback callback, bool disableOnComplete = false)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = fadeImage.color.SetAlpha(startAlpha);
            fadeImage.DOFade(targetAlpha, time).SetEasing(easing).OnComplete(delegate
            {
                callback?.Invoke();

                if (disableOnComplete)
                    fadeImage.gameObject.SetActive(false);
            });
        }

        public override void Initialise()
        {
            joystick.Initialise(UIController.MainCanvas);
        }

        public override void PlayHideAnimation()
        {
            OverlayUI.HideOverlay();

            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            OverlayUI.HideOverlay();

            pauseButton.gameObject.SetActive(true);

            UIController.OnPageOpened(this);

            UIMainMenu.DotsBackground.gameObject.SetActive(false);

            Tween.DelayedCall(0.3f, () =>
            {
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                UIGamepadButton.DisableTag(UIGamepadButtonTag.MainMenu);
            });

            // Initialize/Show Skill Button depending on if the selected character has a skill
            var character = CharactersController.SelectedCharacter;
            if (character != null && character.SkillData != null && character.SkillData.VFXPrefab != null)
            {
                if (skillButton != null) skillButton.gameObject.SetActive(true);
                if (skillIconImage != null) skillIconImage.sprite = character.SkillData.ButtonIcon;
            }
            else
            {
                if (skillButton != null) skillButton.gameObject.SetActive(false);
            }
        }

        public void InitRoomsUI(RoomData[] rooms)
        {
            roomIndicatorsPool.ReturnToPoolEverything();
            roomIndicators.Clear();

            for (int i = 0; i < rooms.Length; i++)
            {
                roomIndicators.Add(roomIndicatorsPool.GetPooledComponent());
                roomIndicators[i].Init();

                if (i == 0)
                    roomIndicators[i].SetAsReached();
            }

            areaText.text = LevelController.GetCurrentAreaText();
        }

        public void UpdateReachedRoomUI(int roomReachedIndex)
        {
            roomIndicators[roomReachedIndex % roomIndicators.Count].SetAsReached();
        }

        public void UpdateCoinsText(int newAmount)
        {
            coinsText.text = CurrenciesHelper.Format(newAmount);
        }

        #region Pause
        private void OnPauseResumeButtonClicked()
        {
            if (!GameController.IsGameActive)
                return;

            Time.timeScale = 1.0f;

            pausePanelCanvasGroup.alpha = 0.0f;
            pausePanelCanvasGroup.DOFade(0.0f, 0.3f, unscaledTime: true).OnComplete(() =>
            {
                pausePanelObject.SetActive(false);
            });
        }

        private void OnPauseExitButtonClicked()
        {
            GameController.OnLevelExit();

            UIController.HidePage<UIGame>();

            ItemDropBehaviour[] dropItems = FindObjectsByType<ItemDropBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            for (int i = 0; i < dropItems.Length; i++)
            {
                dropItems[i].ItemDisable();
            }

            Overlay.Show(0.3f, () =>
            {
                LevelController.UnloadLevel();

                Time.timeScale = 1.0f;

                pausePanelObject.SetActive(false);

                CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);

                CameraController.SetCameraShiftState(false);
                CameraController.EnableCamera(CameraType.Main); // Bật camera follow cho Lobby

                UIController.ShowPage<UIMainMenu>();

                LevelController.LoadLobby(); // Nạp sảnh chờ Lobby

                Overlay.Hide(0.3f, null);
            });
        }

        private void OnPauseButtonClicked()
        {
            Time.timeScale = 0.0f;

            pausePanelObject.SetActive(true);
            pausePanelCanvasGroup.alpha = 0.0f;
            pausePanelCanvasGroup.DOFade(1.0f, 0.3f, unscaledTime: true);

        }
        private void Update()
        {
            if (dashCooldownOverlay != null)
            {
                var behavior = CharacterBehaviour.GetBehaviour();
                if (behavior != null && behavior.DashCooldownTimeLeft > 0)
                {
                    dashCooldownOverlay.gameObject.SetActive(true);
                    dashCooldownOverlay.fillAmount = behavior.DashCooldownTimeLeft / behavior.DashCooldown;
                }
                else
                {
                    dashCooldownOverlay.gameObject.SetActive(false);
                }
            }

            if (skillCooldownOverlay != null)
            {
                var behavior = CharacterBehaviour.GetBehaviour();
                if (behavior != null && behavior.SkillCooldownTimeLeft > 0)
                {
                    skillCooldownOverlay.gameObject.SetActive(true);
                    skillCooldownOverlay.fillAmount = behavior.SkillCooldownTimeLeft / behavior.SkillCooldown;
                }
                else
                {
                    skillCooldownOverlay.gameObject.SetActive(false);
                }
            }
        }



        private void OnDashButtonClicked()
        {
            Debug.Log("[UIGame] OnDashButtonClicked was triggered!");
            var behavior = CharacterBehaviour.GetBehaviour();
            if (behavior == null)
            {
                Debug.LogWarning("[UIGame] Player CharacterBehaviour is null! Cannot perform dash.");
                return;
            }

            Debug.Log($"[UIGame] Player found. IsDashing: {behavior.IsDashing}, CooldownTimeLeft: {behavior.DashCooldownTimeLeft}");
            if (!behavior.IsDashing && behavior.DashCooldownTimeLeft <= 0)
            {
                behavior.PerformDash();
            }
        }

        private void OnSkillButtonClicked()
        {
            var behavior = CharacterBehaviour.GetBehaviour();
            if (behavior == null) return;

            if (behavior.IsSkillReady)
            {
                behavior.ActivateSkill();
            }
        }

        public void SetLobbyMode(bool active)
        {
            if (coinsText != null && coinsText.transform.parent != null)
                coinsText.transform.parent.gameObject.SetActive(!active);

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(!active);

            if (roomsHolder != null && roomsHolder.parent != null)
                roomsHolder.parent.gameObject.SetActive(!active);

            if (areaText != null)
                areaText.gameObject.SetActive(!active);

            UpdateAttackButtonVisibility();
        }

        public void UpdateAttackButtonVisibility()
        {
            if (attackButton != null)
            {
                if (LevelController.IsLobbyMode)
                {
                    attackButton.gameObject.SetActive(false);
                }
                else
                {
                    attackButton.gameObject.SetActive(!CharacterBehaviour.IsAutoShootActive);
                }
            }
        }

        private void OnAutoShootButtonClicked()
        {
            CharacterBehaviour.IsAutoShootActive = !CharacterBehaviour.IsAutoShootActive;
            UpdateAutoShootButtonUI();

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        public void UpdateAutoShootButtonUI()
        {
            if (autoShootButtonImage != null)
            {
                if (CharacterBehaviour.IsAutoShootActive)
                {
                    if (autoShootActiveSprite != null)
                        autoShootButtonImage.sprite = autoShootActiveSprite;
                    else
                        autoShootButtonImage.color = Color.white;
                }
                else
                {
                    if (autoShootDisableSprite != null)
                        autoShootButtonImage.sprite = autoShootDisableSprite;
                    else
                        autoShootButtonImage.color = new Color(1f, 1f, 1f, 0.4f); // Dim button when Auto Shoot is disabled
                }
            }
        }

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent.name == childName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                    return found;
            }

            return null;
        }
        #endregion
    }
}