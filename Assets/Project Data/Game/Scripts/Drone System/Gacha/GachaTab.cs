using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class GachaTab : MonoBehaviour
    {
        [SerializeField] Button gachaButton;
        [SerializeField] TextMeshProUGUI gemsText;

        private void Awake()
        {
            if (gachaButton != null)
            {
                gachaButton.onClick.AddListener(OnGachaButtonClicked);
            }
        }

        private void OnEnable()
        {
            UpdateGemsDisplay();
        }

        public void UpdateGemsDisplay()
        {
            if (gemsText != null && CurrenciesController.Currencies != null)
            {
                gemsText.text = $"{CurrenciesController.Get(CurrencyType.Gems)}";
            }
        }

        private void OnGachaButtonClicked()
        {
            UIController.HidePage<UIMainMenu>(() =>
            {
                UIController.ShowPage<UIGachaPage>();
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}
