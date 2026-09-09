using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using Watermelon.LevelSystem;

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
            "Đang kiểm tra tài nguyên Addressables...",
            "Đang khởi tạo bản đồ sảnh...",
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

        public void ShowInstant(string initialHint = "Đang tải dữ liệu...")
        {
            gameObject.SetActive(true);
            EnableCanvas();
            if (GraphicRaycaster != null) GraphicRaycaster.enabled = true;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            SetProgress(0.05f);
            if (hintText != null) hintText.text = initialHint;
            hintTimer = 0f;
        }

        public void CycleHint(float dt)
        {
            if (hintText == null) return;
            hintTimer += dt;
            if (hintTimer >= 1.5f)
            {
                hintTimer = 0f;
                hintText.text = hints[Random.Range(0, hints.Length)];
            }
        }

        public void StartLobbyLoading(System.Action onFinish = null)
        {
            ShowInstant("Đang kiểm tra tài nguyên Addressables...");
            StartCoroutine(LobbyLoadingCoroutine(onFinish));
        }

        private System.Collections.IEnumerator LobbyLoadingCoroutine(System.Action onFinish)
        {
            // 1. Khởi tạo Addressables nếu chưa khởi tạo
            float targetP = 0.25f;
            var initHandle = Addressables.InitializeAsync();
            while (!initHandle.IsDone)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, targetP, Time.unscaledDeltaTime * 0.8f);
                SetProgress(currentProgress);
                CycleHint(Time.unscaledDeltaTime);
                yield return null;
            }

            // 2. Preload các Addressables (Vũ khí, Đạn, Drone)
            targetP = 0.5f;
            if (hintText != null) hintText.text = "Đang nạp vũ khí & Drone...";

            try
            {
                if (WeaponsController.Database != null && WeaponsController.Database.Weapons != null && WeaponsController.SelectedWeaponIndex < WeaponsController.Database.Weapons.Length)
                {
                    var weaponData = WeaponsController.Database.Weapons[WeaponsController.SelectedWeaponIndex];
                    if (weaponData != null)
                    {
                        var gunUpgrade = UpgradesController.GetUpgrade<Watermelon.Upgrades.BaseWeaponUpgrade>(weaponData.UpgradeType);
                        var gunStage = gunUpgrade?.GetCurrentStage();
                        if (gunStage != null)
                        {
                            var preloadGun = gunStage.WeaponPrefab;
                            var preloadBullet = gunStage.BulletPrefab;
                        }
                    }
                }

                if (DronesController.Database != null && DronesController.Database.Drones != null && DronesController.SelectedDroneIndex != -1 && DronesController.SelectedDroneIndex < DronesController.Database.Drones.Length)
                {
                    var droneData = DronesController.Database.Drones[DronesController.SelectedDroneIndex];
                    if (droneData != null)
                    {
                        var droneUpgrade = UpgradesController.GetUpgrade<Watermelon.Upgrades.BaseDroneUpgrade>(droneData.UpgradeType);
                        var droneStage = droneUpgrade?.GetCurrentStage();
                        if (droneStage != null)
                        {
                            var preloadDrone = droneStage.DronePrefab;
                            var preloadBullet = droneStage.BulletPrefab;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Loading] Preload addressables warning: " + e.Message);
            }

            while (currentProgress < targetP)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, targetP, Time.unscaledDeltaTime * 1.5f);
                SetProgress(currentProgress);
                CycleHint(Time.unscaledDeltaTime);
                yield return null;
            }

            // 3. Tải sảnh chờ (Lobby)
            targetP = 0.9f;
            if (hintText != null) hintText.text = "Đang kiến tạo bản đồ sảnh chờ...";

            bool isLobbyLoaded = false;
            LevelController.LoadLobby(() =>
            {
                isLobbyLoaded = true;
            });

            while (!isLobbyLoaded || currentProgress < 0.95f)
            {
                float ceiling = isLobbyLoaded ? 1.0f : 0.95f;
                currentProgress = Mathf.MoveTowards(currentProgress, ceiling, Time.unscaledDeltaTime * 0.8f);
                SetProgress(currentProgress);
                CycleHint(Time.unscaledDeltaTime);
                yield return null;
            }

            // 4. Hoàn tất
            SetProgress(1f);
            if (hintText != null) hintText.text = "Sẵn sàng!";
            yield return new WaitForSecondsRealtime(0.25f);

            // 5. Fade out biến mất
            onFinish?.Invoke();
            canvasGroup.DOFade(0f, 0.35f, unscaledTime: true).OnComplete(() =>
            {
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;
                DisableCanvas();
            });
        }

        public void FinishLoading(System.Action onFinished = null)
        {
            if (hintText != null) hintText.text = "Sẵn sàng!";
            SetProgress(1f);

            Tween.DelayedCall(0.25f, () =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.35f, unscaledTime: true).OnComplete(() =>
                    {
                        if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;
                        DisableCanvas();
                        onFinished?.Invoke();
                    });
                }
                else
                {
                    if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;
                    DisableCanvas();
                    onFinished?.Invoke();
                }
            }, unscaledTime: true);
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
            
            CycleHint(dt);

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
