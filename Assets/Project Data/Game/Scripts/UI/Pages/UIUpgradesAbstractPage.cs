using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.SquadShooter;
using static Watermelon.Ease;

namespace Watermelon
{
    public abstract class UIUpgradesAbstractPage<T, K> : UIPage
        where T : UIUpgradeAbstractPanel
        where K : System.Enum
    { 
        protected const float SCROLL_SIDE_OFFSET = 50;
        protected const float SCROLL_ELEMENT_WIDTH = 415f;

        [SerializeField] protected Button backButton;

        [Space]
        [SerializeField] protected GameObject panelUIPrefab;
        [SerializeField] protected Transform panelsContainer;

        [Space]
        [SerializeField] protected RectTransform backgroundPanelRectTransform;
        [SerializeField] protected RectTransform closeButtonRectTransform;
        [SerializeField] protected ScrollRect scrollView;

        [Space]
        [SerializeField] protected AnimationCurve panelScaleAnimationCurve;
        [SerializeField] protected AnimationCurve selectedPanelScaleAnimationCurve;

        protected UIGamepadButton gamepadCloseButton;
        public UIGamepadButton GamepadCloseButton => gamepadCloseButton;

        protected List<T> itemPanels = new List<T>();

        protected abstract void EnableGamepadButtonTag();

        public override void Initialise()
        {
            gamepadCloseButton = backButton.GetComponent<UIGamepadButton>();

            backButton.onClick.AddListener(BackButton);

            itemPanels = new List<T>();

            if (UIController.IsTablet)
            {
                var scrollSize = backgroundPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                backgroundPanelRectTransform.sizeDelta = scrollSize;
            }
        }

        protected T AddNewPanel()
        {
            var newPanelObject = Instantiate(panelUIPrefab);
            newPanelObject.transform.SetParent(panelsContainer);
            newPanelObject.transform.ResetLocal();

            var newPanel = newPanelObject.GetComponent<T>();

            itemPanels.Add(newPanel);

            return newPanel;
        }

        protected virtual void Update()
        {
            if (!Canvas.enabled) return;

            T newSelectedPanel = null;

            if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DLeft))
            {
                for (int i = 0; i < itemPanels.Count; i++)
                {
                    if (SelectedIndex == i && i > 0 && itemPanels[i - 1].IsUnlocked)
                    {
                        itemPanels[i - 1].Select();
                        newSelectedPanel = itemPanels[i - 1];
                        break;
                    }
                }
            }
            else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DRight))
            {
                for (int i = 0; i < itemPanels.Count; i++)
                {
                    if (SelectedIndex == i && i < itemPanels.Count - 1 && itemPanels[i + 1].IsUnlocked)
                    {
                        itemPanels[i + 1].Select();
                        newSelectedPanel = itemPanels[i + 1];
                        break;
                    }
                }
            }

            if (newSelectedPanel != null)
            {
                float scrollOffsetX = Mathf.Clamp(-(newSelectedPanel.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET), -scrollView.content.sizeDelta.x, 0);
                scrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0);
                scrollView.StopMovement();
            }
        }

        public override void PlayShowAnimation()
        {
            // Subscribe events
            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged += OnCurrencyAmountChanged;
            }

            backgroundPanelRectTransform.anchoredPosition = new Vector2(0, -1500);
            backgroundPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetCustomEasing(GetCustomEasingFunction("BackOutLight"));

            float scrollOffsetX = -(itemPanels[SelectedIndex].RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);
            scrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0);
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

            UIMainMenu.DotsBackground.gameObject.SetActive(true);

            Tween.DelayedCall(0.9f, () => {
                EnableGamepadButtonTag();
                UIController.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged -= OnCurrencyAmountChanged;
            }

            UIMainMenu.DontFadeRevealNextTime = true;

            UIGeneralPowerIndicator.Hide();

        }

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                itemPanels[i].OnMoneyAmountChanged();
            }
        }

        protected abstract void HidePage(SimpleCallback onFinish);
        public abstract T GetPanel(K type);
        protected abstract int SelectedIndex { get; }

        #region Buttons

        public void BackButton()
        {
            HidePage(UIController.ShowPage<UIMainMenu>);

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        #endregion
    }
}