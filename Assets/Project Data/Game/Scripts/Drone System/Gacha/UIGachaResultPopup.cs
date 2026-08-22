using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UIGachaResultPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] Image droneIconImage;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] Button continueButton;

        [Header("Animation")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform contentRect;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(Hide);
        }

        public void Show(GachaResult result)
        {
            gameObject.SetActive(true);

            if (droneIconImage != null && result.Drone.Icon != null)
                droneIconImage.sprite = result.Drone.Icon;

            if (result.IsNewDrone)
            {
                if (titleText != null)
                    titleText.text = "MỞ KHOÁ DRONE MỚI!";
                if (descriptionText != null)
                    descriptionText.text = result.Drone.Name;
            }
            else
            {
                if (titleText != null)
                    titleText.text = "DRONE TRÙNG";
                if (descriptionText != null)
                    descriptionText.text = $"+{result.CardsReceived} Cards\n(Tổng: {result.Drone.CardsAmount} Cards)";
            }

            // Animation
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, 0.3f);
            }

            if (contentRect != null)
            {
                contentRect.localScale = Vector3.one * 0.5f;
                contentRect.DOScale(1, 0.4f).SetEasing(Ease.Type.BackOut);
            }
        }

        public void Hide()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0, 0.2f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
