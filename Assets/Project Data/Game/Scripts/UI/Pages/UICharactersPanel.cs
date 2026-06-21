using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIUpgradesAbstractPage<CharacterPanelUI, CharacterType>
    {
        [Space]
        [SerializeField] GameObject stageStarPrefab;

        private CharactersDatabase charactersDatabase;

        private Pool stageStarPool;

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

            base.PlayShowAnimation();

            StartAnimations();
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();

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

        #endregion
    }
}