using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UICharacterSkillTooltip : MonoBehaviour
    {
        private static UICharacterSkillTooltip instance;
        public static UICharacterSkillTooltip Instance => instance;

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform tooltipRect;

        [Header("Header")]
        [SerializeField] Image skillIcon;
        [SerializeField] TextMeshProUGUI skillNameText;
        [SerializeField] TextMeshProUGUI skillTagText;

        [Header("Description")]
        [SerializeField] TextMeshProUGUI descriptionText;

        [Header("Stats")]
        [SerializeField] TextMeshProUGUI cooldownText;
        [SerializeField] TextMeshProUGUI durationText;
        [SerializeField] TextMeshProUGUI radiusText;
        [SerializeField] TextMeshProUGUI damageText;

        private TweenCase fadeTween;
        private TweenCase scaleTween;

        private void Awake()
        {
            instance = this;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (tooltipRect != null)
            {
                tooltipRect.localScale = Vector3.one * 0.85f;
            }
            gameObject.SetActive(false);
        }

        public void BindReferences(
            CanvasGroup group,
            RectTransform rect,
            Image icon,
            TextMeshProUGUI skillName,
            TextMeshProUGUI skillTag,
            TextMeshProUGUI desc,
            TextMeshProUGUI cd,
            TextMeshProUGUI dur,
            TextMeshProUGUI rad,
            TextMeshProUGUI dmg)
        {
            canvasGroup = group;
            tooltipRect = rect;
            skillIcon = icon;
            skillNameText = skillName;
            skillTagText = skillTag;
            descriptionText = desc;
            cooldownText = cd;
            durationText = dur;
            radiusText = rad;
            damageText = dmg;
            instance = this;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        public void Show(CharacterSkillData skillData, Vector2? targetPosition = null)
        {
            if (skillData == null)
            {
                skillData = CharactersController.SelectedCharacter?.SkillData;
            }
            if (skillData == null) return;

            gameObject.SetActive(true);

            if (skillIcon != null && skillData.ButtonIcon != null)
            {
                skillIcon.sprite = skillData.ButtonIcon;
                skillIcon.gameObject.SetActive(true);
            }

            if (skillNameText != null)
                skillNameText.text = skillData.SkillName;

            if (skillTagText != null)
                skillTagText.text = skillData.SkillTag;

            if (descriptionText != null)
                descriptionText.text = skillData.GetDescription();

            if (cooldownText != null)
                cooldownText.text = $"{skillData.Cooldown:0.#}s";

            if (durationText != null)
                durationText.text = skillData.Duration > 0 ? $"{skillData.Duration:0.#}s" : "-";

            if (radiusText != null)
                radiusText.text = skillData.AoeRadius > 0 ? $"{skillData.AoeRadius:0.#}m" : "-";

            if (damageText != null)
                damageText.text = skillData.DamageMultiplier > 0 ? $"x{skillData.DamageMultiplier:0.#}" : "-";

            if (tooltipRect != null)
            {
                tooltipRect.anchorMin = new Vector2(0f, 0.5f);
                tooltipRect.anchorMax = new Vector2(0f, 0.5f);
                tooltipRect.pivot = new Vector2(0f, 0.5f);
                tooltipRect.anchoredPosition = new Vector2(610f, 0f);
            }

            fadeTween.KillActive();
            scaleTween.KillActive();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                fadeTween = Tween.DoFloat(0.3f, 1f, 0.15f, (float a) =>
                {
                    if (canvasGroup != null) canvasGroup.alpha = a;
                });
            }

            if (tooltipRect != null)
            {
                tooltipRect.localScale = Vector3.one * 0.88f;
                scaleTween = Tween.DoFloat(0.88f, 1f, 0.2f, (float s) =>
                {
                    if (tooltipRect != null) tooltipRect.localScale = Vector3.one * s;
                }).SetEasing(Ease.Type.BackOut);
            }
        }

        public void Hide()
        {
            fadeTween.KillActive();
            scaleTween.KillActive();

            if (canvasGroup != null)
            {
                fadeTween = Tween.DoFloat(canvasGroup.alpha, 0f, 0.15f, (float a) =>
                {
                    if (canvasGroup != null) canvasGroup.alpha = a;
                }).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (tooltipRect != null)
            {
                scaleTween = Tween.DoFloat(tooltipRect.localScale.x, 0.85f, 0.15f, (float s) =>
                {
                    if (tooltipRect != null) tooltipRect.localScale = Vector3.one * s;
                });
            }
        }
    }

    public class SkillTooltipTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] UICharacterSkillTooltip tooltip;
        [SerializeField] Transform tooltipAnchor;

        private CharacterSkillData currentSkillData;
        private bool isPressed = false;
        private TweenCase pressScaleTween;

        public void SetSkillData(CharacterSkillData skillData, UICharacterSkillTooltip targetTooltip = null)
        {
            currentSkillData = skillData;
            if (targetTooltip != null)
                tooltip = targetTooltip;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;

            pressScaleTween.KillActive();
            pressScaleTween = Tween.DoFloat(transform.localScale.x, 0.96f, 0.1f, (float s) =>
            {
                transform.localScale = Vector3.one * s;
            });

            UICharacterSkillTooltip activeTooltip = tooltip != null ? tooltip : UICharacterSkillTooltip.Instance;
            if (activeTooltip != null)
            {
                activeTooltip.Show(currentSkillData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            pressScaleTween.KillActive();
            pressScaleTween = Tween.DoFloat(transform.localScale.x, 1.0f, 0.15f, (float s) =>
            {
                transform.localScale = Vector3.one * s;
            });

            UICharacterSkillTooltip activeTooltip = tooltip != null ? tooltip : UICharacterSkillTooltip.Instance;
            if (activeTooltip != null)
            {
                activeTooltip.Hide();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isPressed)
            {
                isPressed = false;

                pressScaleTween.KillActive();
                pressScaleTween = Tween.DoFloat(transform.localScale.x, 1.0f, 0.15f, (float s) =>
                {
                    transform.localScale = Vector3.one * s;
                });

                UICharacterSkillTooltip activeTooltip = tooltip != null ? tooltip : UICharacterSkillTooltip.Instance;
                if (activeTooltip != null)
                {
                    activeTooltip.Hide();
                }
            }
        }
    }
}
