using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

using UnityEngine.EventSystems;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterType>, IDragHandler
    {
        [Space]
        [SerializeField] GameObject stageStarPrefab;

        private CharactersDatabase charactersDatabase;

        private Pool stageStarPool;
        private Quaternion originalPlayerRotation;

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
            }

            backgroundPanelRectTransform.anchoredPosition = new Vector2(0, -1500);
            backgroundPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));

            // Dat vi tri Scroll View ve (0, 0) va dung cuon
            scrollView.content.anchoredPosition = Vector2.zero;
            scrollView.StopMovement();

            for (int i = 0; i < itemPanels.Count; i++)
            {
                RectTransform panelTransform = itemPanels[i].RectTransform;

                panelTransform.localScale = Vector2.zero;

                if (i == SelectedIndex)
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.2f).SetCurveEasing(selectedPanelScaleAnimationCurve);
                }
                else
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.3f).SetCurveEasing(panelScaleAnimationCurve);
                }

                itemPanels[i].OnPanelOpened();
            }

            UIGeneralPowerIndicator.Show();

            UIMainMenu.DotsBackground.gameObject.SetActive(false); // An background de thay ro 3D character

            Tween.DelayedCall(0.9f, () => {
                EnableGamepadButtonTag();
                UIController.OnPageOpened(this);
            });

            StartAnimations();

            // Xoay nhan vat doi dien camera va kích hoat camera bay cận canh
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                originalPlayerRotation = characterBehaviour.transform.rotation;

                Vector3 playerPos = characterBehaviour.transform.position;
                Vector3 defaultCamPos = CameraController.MainCamera.transform.position;
                Vector3 dirToCam = defaultCamPos - playerPos;
                dirToCam.y = 0;
                if (dirToCam.sqrMagnitude > 0.01f)
                {
                    Vector3 lookDir = dirToCam.normalized;
                    characterBehaviour.transform.rotation = Quaternion.LookRotation(lookDir);
                    
                    // Kich hoat camera bay den vi tri phia truoc và lech phai (gip nhan vat dung ben trai man hinh)
                    Vector3 right = Vector3.Cross(Vector3.up, lookDir).normalized; // Right vector local
                    CameraController.EnterCharacterSelection(playerPos, lookDir, right, Vector3.up);
                }

                // Tat di chuyen va agent de tranh nguoi choi dieu khien nhan vat trong khi mo UI
                Control.DisableMovementControl();
                characterBehaviour.DisableAgent();
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

            // Khoi phuc huong xoay nhan vat va tra quyen kiem soat cho Cinemachine
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                characterBehaviour.transform.rotation = originalPlayerRotation;
                // Bat lai di chuyen va agent cua nhan vat
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
                // Xoay nhan vat theo truc Y
                float rotationSpeed = -0.5f;
                characterBehaviour.transform.Rotate(Vector3.up, eventData.delta.x * rotationSpeed, Space.World);
            }
        }

        #endregion
    }
}