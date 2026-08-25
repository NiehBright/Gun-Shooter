using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UIGachaResultPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Button continueButton;
        
        [Header("Item Spawning")]
        [SerializeField] Transform itemsContainer;
        [SerializeField] GameObject itemPrefab;

        [Header("Animation")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform contentRect;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(Hide);
        }

        public void Show(GachaResult[] results)
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "KẾT QUẢ QUAY DRONE";
            }

            // Clear old items
            if (itemsContainer != null)
            {
                foreach (Transform child in itemsContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Spawn new items
            if (itemsContainer != null && itemPrefab != null)
            {
                for (int i = 0; i < results.Length; i++)
                {
                    GameObject obj = Instantiate(itemPrefab, itemsContainer);
                    UIGachaResultItem itemScript = obj.GetComponent<UIGachaResultItem>();
                    if (itemScript != null)
                    {
                        itemScript.Init(results[i]);
                    }
                }
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
