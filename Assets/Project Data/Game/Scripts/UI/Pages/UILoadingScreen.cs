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
        [SerializeField] CanvasGroup canvasGroup;

        private float targetProgress;
        private float currentProgress;
        private bool triggeredHalfWay;

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
            timer += Time.unscaledDeltaTime;
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
