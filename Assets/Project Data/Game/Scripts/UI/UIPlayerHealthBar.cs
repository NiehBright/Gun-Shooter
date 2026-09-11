using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class UIPlayerHealthBar : MonoBehaviour
    {
        [SerializeField] RectTransform healthFillRect;
        [SerializeField] RectTransform delayFillRect;
        [SerializeField] Slider healthSlider;
        [SerializeField] Image healthFillImage;
        [SerializeField] Image maskFillImage;
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image heartIcon;

        private float targetFill = 1f;
        private float currentFill = 1f;
        private float delayFill = 1f;
        private float delayTimer = 0f;
        private TweenCase maskTweenCase;
        private TweenCase fadeTweenCase;
        private bool isVisible = true;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            UpdateInitialVisibility();
        }

        private void OnEnable()
        {
            CharacterBehaviour.OnPlayerHealthChanged += OnHealthChanged;
            CharacterBehaviour.OnPlayerSpawned += OnPlayerSpawned;

            UpdateInitialVisibility();

            // Fetch current player if already spawned
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null)
            {
                SetHealth(player.CurrentHealth, player.MaxHealth, true);
            }
        }

        private void UpdateInitialVisibility()
        {
            bool isLobby = LevelController.IsLobbyMode;
            bool isCombat = LobbyCombatController.IsCombatModeActive;
            SetVisible(!isLobby || isCombat, true);
        }

        private void OnDisable()
        {
            CharacterBehaviour.OnPlayerHealthChanged -= OnHealthChanged;
            CharacterBehaviour.OnPlayerSpawned -= OnPlayerSpawned;
            maskTweenCase.KillActive();
            fadeTweenCase.KillActive();
        }

        private void Update()
        {
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
            }
            else if (delayFillRect != null && delayFill > targetFill)
            {
                delayFill = Mathf.MoveTowards(delayFill, targetFill, Time.deltaTime * 0.8f);
                delayFillRect.anchorMax = new Vector2(delayFill, 1f);
                delayFillRect.offsetMin = Vector2.zero;
                delayFillRect.offsetMax = Vector2.zero;
            }
        }

        private void OnPlayerSpawned(CharacterBehaviour player)
        {
            if (player != null)
            {
                SetHealth(player.CurrentHealth, player.MaxHealth, true);
            }
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            SetHealth(currentHealth, maxHealth, false);
        }

        public void SetHealth(float currentHealth, float maxHealth, bool instant = false)
        {
            float fill = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
            targetFill = fill;

            if (instant)
            {
                currentFill = fill;
                delayFill = fill;
                delayTimer = 0f;
                if (healthFillRect != null)
                {
                    healthFillRect.anchorMax = new Vector2(fill, 1f);
                    healthFillRect.offsetMin = Vector2.zero;
                    healthFillRect.offsetMax = Vector2.zero;
                }
                if (delayFillRect != null)
                {
                    delayFillRect.anchorMax = new Vector2(fill, 1f);
                    delayFillRect.offsetMin = Vector2.zero;
                    delayFillRect.offsetMax = Vector2.zero;
                }
            }
            else
            {
                if (fill < currentFill)
                {
                    // Player took damage: delay starts after a short pause
                    delayTimer = 0.25f;
                }

                currentFill = fill;
                if (healthFillRect != null)
                {
                    healthFillRect.anchorMax = new Vector2(fill, 1f);
                    healthFillRect.offsetMin = Vector2.zero;
                    healthFillRect.offsetMax = Vector2.zero;
                }

                if (fill >= delayFill)
                {
                    delayFill = fill;
                    delayTimer = 0f;
                    if (delayFillRect != null)
                    {
                        delayFillRect.anchorMax = new Vector2(fill, 1f);
                        delayFillRect.offsetMin = Vector2.zero;
                        delayFillRect.offsetMax = Vector2.zero;
                    }
                }
            }

            if (healthSlider != null)
            {
                healthSlider.value = fill;
            }

            if (healthFillImage != null && healthFillImage.type == Image.Type.Filled)
            {
                healthFillImage.fillAmount = fill;
            }

            maskTweenCase.KillActive();
            if (maskFillImage != null && maskFillImage.type == Image.Type.Filled)
            {
                if (instant)
                {
                    maskFillImage.fillAmount = fill;
                }
                else
                {
                    maskTweenCase = maskFillImage.DOFillAmount(fill, 0.35f).SetEasing(Ease.Type.QuintIn);
                }
            }

            if (healthText != null)
            {
                healthText.text = string.Format("{0} / {1}", Mathf.Max(0, Mathf.RoundToInt(currentHealth)), Mathf.RoundToInt(maxHealth));
            }
        }

        public void SetVisible(bool visible, bool instant = false)
        {
            isVisible = visible;
            fadeTweenCase.KillActive();

            if (canvasGroup != null)
            {
                if (instant)
                {
                    canvasGroup.alpha = visible ? 1f : 0f;
                    canvasGroup.interactable = visible;
                    canvasGroup.blocksRaycasts = visible;
                }
                else
                {
                    fadeTweenCase = canvasGroup.DOFade(visible ? 1f : 0f, 0.25f).OnComplete(() =>
                    {
                        canvasGroup.interactable = visible;
                        canvasGroup.blocksRaycasts = visible;
                    });
                }
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
