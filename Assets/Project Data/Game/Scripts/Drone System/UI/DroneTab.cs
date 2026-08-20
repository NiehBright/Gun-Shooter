using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class DroneTab : MonoBehaviour
    {
        [SerializeField] Image tabImage;
        [SerializeField] Color defaultColor;
        [SerializeField] Color notificationColor;
        [SerializeField] Color disabledColor;
        [SerializeField] GameObject notificationObject;

        private UIDronePage dronePage;
        private TweenCase movementTweenCase;
        private Vector2 defaultAnchoredPosition;
        private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        private Button button;
        public Button Button => button;

        private UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        private CanvasGroup canvasGroup;
        private bool isActive;

        public void Initialise()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();
            gamepadButton = GetComponent<UIGamepadButton>();
            rectTransform = (RectTransform)transform;

            dronePage = UIController.GetPage<UIDronePage>();

            defaultAnchoredPosition = rectTransform.anchoredPosition;
            isActive = true;
        }

        public void OnWindowOpened()
        {
            if (!isActive) return;

            movementTweenCase.KillActive();

            rectTransform.anchoredPosition = defaultAnchoredPosition;
            tabImage.color = defaultColor;

            if (dronePage != null && dronePage.IsAnyActionAvailable())
            {
                notificationObject.SetActive(true);
                tabImage.color = notificationColor;
            }
            else
            {
                notificationObject.SetActive(false);
            }

            canvasGroup.alpha = 1.0f;
            button.enabled = true;
        }

        public void OnWindowClosed()
        {
            if (!isActive) return;

            movementTweenCase.KillActive();

            rectTransform.anchoredPosition = defaultAnchoredPosition;
            tabImage.color = defaultColor;

            if (dronePage != null && dronePage.IsAnyActionAvailable())
            {
                notificationObject.SetActive(true);
                tabImage.color = notificationColor;
            }
            else
            {
                notificationObject.SetActive(false);
            }

            canvasGroup.alpha = 1.0f;
            button.enabled = true;
        }

        public void PlayHideAnimation()
        {
            if (!isActive) return;

            movementTweenCase = rectTransform.DOAnchoredPosition(defaultAnchoredPosition + new Vector2(0, -250), 0.3f).SetEasing(Ease.Type.BackIn);
            canvasGroup.alpha = 0.5f;
            button.enabled = false;
        }

        public void OnButtonClicked()
        {
            UIController.HidePage<UIMainMenu>(() =>
            {
                UIController.ShowPage<UIDronePage>();
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}
