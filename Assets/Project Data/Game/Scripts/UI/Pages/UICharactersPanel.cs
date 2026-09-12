using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

using UnityEngine.EventSystems;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterType>, IDragHandler, IEndDragHandler
    {
        [Space]
        [SerializeField] GameObject stageStarPrefab;

        [Header("Custom Layout")]
        [SerializeField] UICharacterDetailsPanel detailsPanel;
        public UICharacterDetailsPanel DetailsPanel => detailsPanel;

        [SerializeField] RectTransform leftPanelRectTransform;
        [SerializeField] RectTransform rightPanelRectTransform;

        public RectTransform BackgroundPanelRectTransform => backgroundPanelRectTransform;
        public ScrollRect ScrollView => scrollView;
        public Transform PanelsContainer => panelsContainer;
        public Button BackButtonObject => backButton;
        public RectTransform CloseButtonRectTransform => closeButtonRectTransform;

        private CharactersDatabase charactersDatabase;

        private Pool stageStarPool;
        private Quaternion originalPlayerRotation;
        private Quaternion showcasePlayerRotation;
        private TweenCase resetPlayerRotationTweenCase;

        protected override int SelectedIndex => Mathf.Clamp(CharactersController.GetCharacterIndex(CharactersController.SelectedCharacter.Type), 0, int.MaxValue);

        public GameObject GetStageStarObject()
        {
            return stageStarPool.GetPooledObject();
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNewCharacterOpened())
                    return true;

                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        protected override void EnableGamepadButtonTag()
        {
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Characters);
        }

        #region Animation

        private bool isAnimationPlaying;
        private Coroutine animationCoroutine;

        private static bool isControlBlocked = false;
        public static bool IsControlBlocked => isControlBlocked;

        private static List<CharacterDynamicAnimation> characterDynamicAnimations = new List<CharacterDynamicAnimation>();

        private void ResetAnimations()
        {
            if (isAnimationPlaying)
            {
                StopCoroutine(animationCoroutine);

                isAnimationPlaying = false;
                animationCoroutine = null;
            }

            characterDynamicAnimations = new List<CharacterDynamicAnimation>();
        }

        private void StartAnimations()
        {
            if (isAnimationPlaying)
                return;

            if (!characterDynamicAnimations.IsNullOrEmpty())
            {
                isControlBlocked = true;
                scrollView.enabled = false;

                isAnimationPlaying = true;

                animationCoroutine = StartCoroutine(DynamicAnimationCoroutine());
            }
        }

        private IEnumerator ScrollCoroutine(CharacterPanelUI characterPanelUI)
        {
            float scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);

            float positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);

            if (positionDiff > 80)
            {
                Ease.IEasingFunction easeFunctionCubicIn = Ease.GetFunction(Ease.Type.CubicOut);

                Vector2 currentPosition = scrollView.content.anchoredPosition;
                Vector2 targetPosition = new Vector2(scrollOffsetX, 0);

                float speed = positionDiff / 2500;

                for (float s = 0; s < 1.0f; s += Time.deltaTime / speed)
                {
                    scrollView.content.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, easeFunctionCubicIn.Interpolate(s));

                    yield return null;
                }
            }
        }

        private IEnumerator DynamicAnimationCoroutine()
        {
            int currentAnimationIndex = 0;
            CharacterDynamicAnimation tempAnimation;
            WaitForSeconds delayWait = new WaitForSeconds(0.4f);

            yield return delayWait;

            while (currentAnimationIndex < characterDynamicAnimations.Count)
            {
                tempAnimation = characterDynamicAnimations[currentAnimationIndex];

                delayWait = new WaitForSeconds(tempAnimation.Delay);

                yield return StartCoroutine(ScrollCoroutine(tempAnimation.CharacterPanel));

                tempAnimation.OnAnimationStarted?.Invoke();

                yield return delayWait;

                currentAnimationIndex++;
            }

            yield return null;

            isAnimationPlaying = false;
            isControlBlocked = false;
            scrollView.enabled = true;
        }

        public void AddAnimations(List<CharacterDynamicAnimation> characterDynamicAnimation, bool isPrioritize = false)
        {
            if (!isPrioritize)
            {
                characterDynamicAnimations.AddRange(characterDynamicAnimation);
            }
            else
            {
                characterDynamicAnimations.InsertRange(0, characterDynamicAnimation);
            }
        }

        #endregion

        #region UI Page

        public override void Initialise()
        {
            base.Initialise();

            charactersDatabase = CharactersController.GetDatabase();

            stageStarPool = new Pool(new PoolSettings(stageStarPrefab.name, stageStarPrefab, 1, true));
            stageStarPool.Initialize();

            // Build or refresh iPhone 12 layout
            UICharactersPanelBuilder.BuildLayout(this);

            if (detailsPanel != null)
            {
                detailsPanel.Initialise(this);
            }

            for (int i = 0; i < charactersDatabase.Characters.Length; i++)
            {
                var newPanel = AddNewPanel();
                newPanel.Initialise(charactersDatabase.Characters[i], this);
            }
        }

        public override void PlayShowAnimation()
        {
            ResetAnimations();

            // Subscribe events
            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged += OnCurrencyAmountChanged;
                CurrenciesController.Currencies[i].OnCurrencyChanged += OnCurrencyChangedCallback;
            }

            // Left Details Panel slide in from left (-800 -> 90)
            if (leftPanelRectTransform != null)
            {
                leftPanelRectTransform.anchoredPosition = new Vector2(-800f, 0f);
                leftPanelRectTransform.DOAnchoredPosition(new Vector2(90f, 0f), 0.35f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }

            // Right Selection Panel (backgroundPanelRectTransform) slide in from right (800 -> -70)
            if (backgroundPanelRectTransform != null)
            {
                backgroundPanelRectTransform.anchoredPosition = new Vector2(800f, 0f);
                backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(-70f, 0f), 0.35f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }

            // Reset scroll position
            if (scrollView != null && scrollView.content != null)
            {
                scrollView.content.anchoredPosition = Vector2.zero;
                scrollView.StopMovement();
            }

            // Animate cards appearance (Enlarged scales)
            for (int i = 0; i < itemPanels.Count; i++)
            {
                RectTransform panelTransform = itemPanels[i].RectTransform;
                panelTransform.localScale = Vector2.zero;

                float targetScale = (i == SelectedIndex) ? 0.78f : 0.70f;
                panelTransform.DOScale(Vector3.one * targetScale, 0.3f, 0.15f + i * 0.05f).SetCurveEasing(panelScaleAnimationCurve);

                itemPanels[i].OnPanelOpened();
            }

            // Update details panel with currently selected character
            if (detailsPanel != null && CharactersController.SelectedCharacter != null)
            {
                detailsPanel.DisplayCharacter(CharactersController.SelectedCharacter);
            }

            UIGeneralPowerIndicator.Show();
            UIMainMenu.DotsBackground.gameObject.SetActive(false); // An background de thay ro 3D character

            Tween.DelayedCall(0.9f, () => {
                EnableGamepadButtonTag();
                UIController.OnPageOpened(this);
            });

            StartAnimations();

            // Xoay nhan vat doi dien camera va kích hoat camera bay cận canh chinh giua
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                originalPlayerRotation = characterBehaviour.transform.rotation;
                showcasePlayerRotation = originalPlayerRotation;

                Vector3 playerPos = characterBehaviour.transform.position;
                Vector3 defaultCamPos = CameraController.MainCamera.transform.position;
                Vector3 dirToCam = defaultCamPos - playerPos;
                dirToCam.y = 0;
                if (dirToCam.sqrMagnitude > 0.01f)
                {
                    Vector3 lookDir = dirToCam.normalized;
                    characterBehaviour.transform.rotation = Quaternion.LookRotation(lookDir);
                    showcasePlayerRotation = characterBehaviour.transform.rotation;
                    
                    // Kich hoat camera bay den vi tri phia truoc (horizontalOffset = 0f giup nhan vat dung chinh giua man hinh)
                    Vector3 right = Vector3.Cross(Vector3.up, lookDir).normalized;
                    CameraController.EnterCharacterSelection(playerPos, lookDir, right, Vector3.up, 0f);
                }

                // Tat di chuyen va agent de tranh nguoi choi dieu khien nhan vat trong khi mo UI
                Control.DisableMovementControl();
                characterBehaviour.DisableAgent();

                // An drone
                if (characterBehaviour.CurrentDrone != null)
                {
                    characterBehaviour.CurrentDrone.gameObject.SetActive(false);
                }

                // An UI mau tren dau nhan vat
                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.ForceDisable();
                }
            }
        }

        protected override void Update()
        {
            if (!Canvas.enabled) return;
            // Bo qua logic update ngang tu gamepad cua base class
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();

            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged -= OnCurrencyChangedCallback;
            }

            if (leftPanelRectTransform != null)
            {
                leftPanelRectTransform.DOAnchoredPosition(new Vector2(-800f, 0f), 0.25f).SetEasing(Ease.Type.CubicIn);
            }

            // Khoi phuc huong xoay nhan vat va tra quyen kiem soat cho Cinemachine
            resetPlayerRotationTweenCase.KillActive();
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                characterBehaviour.transform.rotation = originalPlayerRotation;
                Control.EnableMovementControl();
                characterBehaviour.ActivateAgent();

                if (characterBehaviour.CurrentDrone != null)
                {
                    characterBehaviour.CurrentDrone.gameObject.SetActive(true);
                }

                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.EnableBar(true);
                }
            }
            CameraController.ExitCharacterSelection();

            if (backgroundPanelRectTransform != null)
            {
                backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(800f, 0f), 0.25f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
                {
                    UIController.OnPageClosed(this);
                });
            }
            else
            {
                UIController.OnPageClosed(this);
            }
        }

        public void OnCharacterSelected(Character character)
        {
            if (detailsPanel != null)
            {
                detailsPanel.DisplayCharacter(character);
            }
        }

        public void OnCharacterUpgradedInternal(Character character)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                itemPanels[i].OnPanelOpened();
            }
        }

        private void OnCurrencyChangedCallback(Currency currency, int difference)
        {
            if (detailsPanel != null)
            {
                detailsPanel.UpdateUpgradeButtonsState();
            }
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UICharactersPanel>(onFinish);
        }

        public override CharacterPanelUI GetPanel(CharacterType characterType)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Character.Type == characterType)
                    return itemPanels[i];
            }

            return null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                resetPlayerRotationTweenCase.KillActive();
                // Xoay nhan vat theo truc Y
                float rotationSpeed = -0.5f;
                characterBehaviour.transform.Rotate(Vector3.up, eventData.delta.x * rotationSpeed, Space.World);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                Quaternion startRot = characterBehaviour.transform.rotation;
                resetPlayerRotationTweenCase.KillActive();
                resetPlayerRotationTweenCase = Tween.DoFloat(0f, 1f, 0.4f, (float t) =>
                {
                    if (characterBehaviour != null)
                    {
                        characterBehaviour.transform.rotation = Quaternion.Slerp(startRot, showcasePlayerRotation, t);
                    }
                }).SetEasing(Ease.Type.QuadOut);
            }
        }

        #endregion
    }
}

