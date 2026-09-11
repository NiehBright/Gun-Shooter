using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UIBossHealthBar : MonoBehaviour
    {
        private static UIBossHealthBar instance;
        public static UIBossHealthBar Instance => instance;

        [SerializeField] Slider bossSlider;
        [SerializeField] Image bossFillImage;
        [SerializeField] Image bossMaskFillImage;
        [SerializeField] TextMeshProUGUI bossNameText;
        [SerializeField] TextMeshProUGUI bossHealthText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image bossIcon;

        private TweenCase maskTweenCase;
        private TweenCase fadeTweenCase;

        private void Awake()
        {
            instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            maskTweenCase.KillActive();
            fadeTweenCase.KillActive();
        }

        public static void ShowBoss(string bossName, float currentHealth, float maxHealth)
        {
            if (instance == null) return;

            instance.gameObject.SetActive(true);

            if (instance.bossNameText != null)
            {
                instance.bossNameText.text = bossName;
            }

            instance.SetHealth(currentHealth, maxHealth, true);

            instance.fadeTweenCase.KillActive();
            if (instance.canvasGroup != null)
            {
                instance.fadeTweenCase = instance.canvasGroup.DOFade(1f, 0.4f);
            }
        }

        public static void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (instance == null) return;
            instance.SetHealth(currentHealth, maxHealth, false);
        }

        public static void HideBoss(bool instant = false)
        {
            if (instance == null) return;

            instance.fadeTweenCase.KillActive();
            if (instance.canvasGroup != null)
            {
                if (instant)
                {
                    instance.canvasGroup.alpha = 0f;
                }
                else
                {
                    instance.fadeTweenCase = instance.canvasGroup.DOFade(0f, 0.5f);
                }
            }
            else
            {
                instance.gameObject.SetActive(false);
            }
        }

        public void SetHealth(float currentHealth, float maxHealth, bool instant = false)
        {
            float fill = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

            if (bossSlider != null)
            {
                bossSlider.value = fill;
            }

            if (bossFillImage != null)
            {
                bossFillImage.fillAmount = fill;
            }

            maskTweenCase.KillActive();
            if (bossMaskFillImage != null)
            {
                if (instant)
                {
                    bossMaskFillImage.fillAmount = fill;
                }
                else
                {
                    maskTweenCase = bossMaskFillImage.DOFillAmount(fill, 0.4f).SetEasing(Ease.Type.QuintIn);
                }
            }

            if (bossHealthText != null)
            {
                bossHealthText.text = string.Format("{0} / {1}", Mathf.Max(0, Mathf.RoundToInt(currentHealth)), Mathf.RoundToInt(maxHealth));
            }
        }
    }
}
