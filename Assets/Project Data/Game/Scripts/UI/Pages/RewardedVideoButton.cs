using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class RewardedVideoButton : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Sprite activeBackgroundSprite;
        [SerializeField] Sprite blockedBackgroundSprite;

        [Space]
        [SerializeField] GameObject adsContentObject;

        [Space]
        [SerializeField] GameObject currencyContentObject;
        [SerializeField] Image currencyIconImage;
        [SerializeField] TextMeshProUGUI currencyText;

        private Button button;
        public Button Button => button;

        private CurrencyPrice currencyPrice;
        private SimpleBoolCallback completeCallback;

        private bool isInitialised;
        private Currency currency;

        public void Initialise(SimpleBoolCallback completeCallback, CurrencyPrice currencyPrice)
        {
            this.completeCallback = completeCallback;
            this.currencyPrice = currencyPrice;

            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

            currency = currencyPrice.Currency;
            currency.OnCurrencyChanged += OnCurrencyChanged;

            isInitialised = true;

            Redraw();
        }

        private void OnCurrencyChanged(Currency currency, int difference)
        {
            if (!isInitialised) return;
            if (AdsManager.Settings.RewardedVideoType != AdProvider.Disable) return;

            Redraw();
        }

        public void Redraw()
        {
            // Activate currency purchase option if RV is disabled
            if(AdsManager.Settings.RewardedVideoType == AdProvider.Disable)
            {
                adsContentObject.SetActive(false);

                currencyContentObject.SetActive(true);

                Currency currency = currencyPrice.Currency;
                currencyIconImage.sprite = currency.Icon;
                currencyText.text = currencyPrice.FormattedPrice;

                if(currencyPrice.EnoughMoneyOnBalance())
                {
                    backgroundImage.sprite = activeBackgroundSprite;
                }
                else
                {
                    backgroundImage.sprite = blockedBackgroundSprite;
                }
            }
            else
            {
                currencyContentObject.SetActive(false);

                adsContentObject.SetActive(true);

                backgroundImage.sprite = activeBackgroundSprite;
            }
        }

        private void OnButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (AdsManager.Settings.RewardedVideoType == AdProvider.Disable)
            {
                if (currencyPrice.EnoughMoneyOnBalance())
                {
                    currencyPrice.SubstractFromBalance();

                    completeCallback?.Invoke(true);
                }
                else
                {
                    completeCallback?.Invoke(false);
                }
            }
            else
            {
                AdsManager.ShowRewardBasedVideo((success) =>
                {
                    completeCallback?.Invoke(success);
                });
            }
        }

        public void Clear()
        {
            isInitialised = false;

            completeCallback = null;
            currencyPrice = null;

            if (currency != null)
            {
                currency.OnCurrencyChanged -= OnCurrencyChanged;

                currency = null;
            }

            button.onClick.RemoveAllListeners();

            gameObject.SetActive(false);
        }
    }
}
