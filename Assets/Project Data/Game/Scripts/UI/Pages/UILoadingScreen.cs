using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public class UILoadingScreen : UIPage
    {
        [SerializeField] Slider loadingSlider;
        [SerializeField] Image progressFillImage;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] TextMeshProUGUI hintText;
        [SerializeField] CanvasGroup canvasGroup;

        private float targetProgress;
        private float currentProgress;
        private bool triggeredHalfWay;
        private float hintTimer;

        private readonly string[] hints = new string[]
        {
            "Đang nạp đạn...",
            "Đang bảo dưỡng Drone...",
            "Đang sơn lại súng...",
            "Đang đánh bóng áo giáp...",
            "Đang sạc năng lượng...",
            "Đang quét mục tiêu..."
        };

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public override void Initialise()
        {
            canvasGroup.alpha = 0f;
            SetProgress(0f);
        }

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            currentProgress = progress;
            if (loadingSlider != null) loadingSlider.value = progress;
            if (progressFillImage != null) progressFillImage.fillAmount = progress;
            if (progressText != null) progressText.text = string.Format("{0}%", Mathf.RoundToInt(progress * 100f));
        }

        public void ShowLoading(float duration, System.Action onHalfWay, System.Action onComplete)
        {
            triggeredHalfWay = false;
            SetProgress(0f);
            
            if (hintText != null)
            {
                hintText.text = hints[Random.Range(0, hints.Length)];
                hintTimer = 0f;
            }

            // Kích hoạt canvas hiển thị trước khi fade
            EnableCanvas();
            GraphicRaycaster.enabled = true;
            canvasGroup.alpha = 0f;
            
            // Fade-in màn hình đen loading
            canvasGroup.DOFade(1f, 0.25f, unscaledTime: true).OnComplete(() =>
            {
                float timer = 0f;
                // Chạy cập nhật mượt bằng NextFrame để tránh đứng hình
                Tween.NextFrame(() =>
                {
                    UpdateLoading(timer, duration, onHalfWay, onComplete);
                });
            });
        }

        private void UpdateLoading(float timer, float duration, System.Action onHalfWay, System.Action onComplete)
        {
            float dt = Time.unscaledDeltaTime;
            timer += dt;
            
            if (hintText != null)
            {
                hintTimer += dt;
                if (hintTimer >= 1.5f)
                {
                    hintTimer = 0f;
                    hintText.text = hints[Random.Range(0, hints.Length)];
                }
            }

            float progress = Mathf.Clamp01(timer / duration);
            SetProgress(progress);

            // Nạp dữ liệu màn chơi thực tế ở mốc 50%
            if (progress >= 0.5f && !triggeredHalfWay)
            {
                triggeredHalfWay = true;
                onHalfWay?.Invoke();
            }

            if (progress < 1f)
            {
                Tween.NextFrame(() => UpdateLoading(timer, duration, onHalfWay, onComplete));
            }
            else
            {
                SetProgress(1f);
                // Báo hoàn thành nạp màn chơi
                onComplete?.Invoke();
                
                // Trì hoãn nhẹ 0.1s ở mốc 100% cho người chơi kịp nhìn thấy rồi Fade Out
                Tween.DelayedCall(0.15f, () =>
                {
                    canvasGroup.DOFade(0f, 0.25f, unscaledTime: true).OnComplete(() =>
                    {
                        GraphicRaycaster.enabled = false;
                        DisableCanvas();
                    });
                }, unscaledTime: true);
            }
        }
    }
}
