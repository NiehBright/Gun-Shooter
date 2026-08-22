using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UIGachaPage : UIPage
    {
        [Header("Buttons")]
        [SerializeField] Button singlePullButton;
        [SerializeField] Button multiPullButton;
        [SerializeField] Button backButton;

        [Header("Price Text")]
        [SerializeField] TextMeshProUGUI singlePriceText;
        [SerializeField] TextMeshProUGUI multiPriceText;
        [SerializeField] TextMeshProUGUI gemsAmountText;

        [Header("Result Panel")]
        [SerializeField] GameObject resultPanel;
        [SerializeField] Image resultDroneImage;
        [SerializeField] TextMeshProUGUI resultTitleText;
        [SerializeField] TextMeshProUGUI resultDescText;
        [SerializeField] Button resultCloseButton;

        [Header("Animation")]
        [SerializeField] RectTransform mainPanelRect;

        public override void Initialise()
        {
            if (singlePullButton != null)
                singlePullButton.onClick.AddListener(OnSinglePullClicked);
            if (multiPullButton != null)
                multiPullButton.onClick.AddListener(OnMultiPullClicked);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            if (resultCloseButton != null)
                resultCloseButton.onClick.AddListener(OnResultCloseClicked);

            if (resultPanel != null)
                resultPanel.SetActive(false);
        }

        public override void PlayShowAnimation()
        {
            UpdateUI();

            if (mainPanelRect != null)
            {
                mainPanelRect.anchoredPosition = new Vector2(0, -1500);
                mainPanelRect.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.BackOut);
            }

            Tween.DelayedCall(0.5f, () => {
                UIController.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            if (mainPanelRect != null)
            {
                mainPanelRect.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
                {
                    UIController.OnPageClosed(this);
                });
            }
            else
            {
                UIController.OnPageClosed(this);
            }
        }

        private void UpdateUI()
        {
            if (singlePriceText != null)
                singlePriceText.text = $"{GachaController.Database.SinglePullPrice}";
            if (multiPriceText != null)
                multiPriceText.text = $"{GachaController.Database.MultiPullPrice}";
            if (gemsAmountText != null)
                gemsAmountText.text = $"{CurrenciesController.Get(CurrencyType.Gems)}";

            // Update button states
            if (singlePullButton != null)
                singlePullButton.interactable = GachaController.CanPullSingle();
            if (multiPullButton != null)
                multiPullButton.interactable = GachaController.CanPullMulti();
        }

        private void OnSinglePullClicked()
        {
            if (!GachaController.CanPullSingle())
            {
                Debug.Log("[Gacha UI] Không đủ Gems!");
                return;
            }

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GachaResult result = GachaController.PullSingle();
            if (result != null)
            {
                ShowResult(result);
            }

            UpdateUI();
        }

        private void OnMultiPullClicked()
        {
            if (!GachaController.CanPullMulti())
            {
                Debug.Log("[Gacha UI] Không đủ Gems cho multi pull!");
                return;
            }

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GachaResult[] results = GachaController.PullMulti();
            if (results != null && results.Length > 0)
            {
                // Show first result (later can implement carousel)
                ShowResult(results[0]);
            }

            UpdateUI();
        }

        private void ShowResult(GachaResult result)
        {
            if (resultPanel == null) return;

            resultPanel.SetActive(true);

            if (resultDroneImage != null && result.Drone.Icon != null)
                resultDroneImage.sprite = result.Drone.Icon;

            if (result.IsNewDrone)
            {
                if (resultTitleText != null)
                    resultTitleText.text = "MỞ KHOÁ DRONE MỚI!";
                if (resultDescText != null)
                    resultDescText.text = result.Drone.Name;
            }
            else
            {
                if (resultTitleText != null)
                    resultTitleText.text = "DRONE TRÙNG";
                if (resultDescText != null)
                    resultDescText.text = $"+{result.CardsReceived} Cards\n(Tổng: {result.Drone.CardsAmount} Cards)";
            }
        }

        private void OnResultCloseClicked()
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        private void OnBackClicked()
        {
            UIController.HidePage<UIGachaPage>(UIController.ShowPage<UIMainMenu>);
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}
