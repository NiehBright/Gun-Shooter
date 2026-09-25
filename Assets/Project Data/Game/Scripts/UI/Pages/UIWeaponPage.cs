using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIWeaponPage : UIUpgradesAbstractPage<WeaponPanelUI, WeaponType>, IDragHandler, IEndDragHandler
    {
        [Header("Custom Docked Layout")]
        [SerializeField] private UIWeaponDetailsPanel detailsPanel;
        public UIWeaponDetailsPanel DetailsPanel => detailsPanel;

        [SerializeField] private RectTransform leftPanelRectTransform;
        [SerializeField] private RectTransform rightPanelRectTransform;

        public RectTransform BackgroundPanelRectTransform => backgroundPanelRectTransform;
        public ScrollRect ScrollView => scrollView;
        public Transform PanelsContainer => panelsContainer;
        public Button BackButtonObject => backButton;
        public RectTransform CloseButtonRectTransform => closeButtonRectTransform;

        private WeaponsController weaponController;
        private int previewWeaponIndex = 0;
        public int PreviewWeaponIndex => previewWeaponIndex;

        private WeaponShowcaseBehaviour showcaseBehaviour;
        private Vector3 showcaseWorldPosition;
        private Quaternion originalPlayerRotation;

        [Header("3D Weapon Showcase Camera Settings")]
        [SerializeField] private float cameraDistance = 3.0f;
        [SerializeField] private float cameraHeight = 0.85f;
        [SerializeField] private float lookAtHeight = 1.15f;
        [SerializeField] private float cameraAngleOffset = 15f;    // Góc lệch của camera sang góc bên kia (+15°)
        [SerializeField] private float characterYawAngle = -30f;   // Góc xoay của nhân vật sang phía đối diện (-30°)
        [SerializeField] private float weaponFocusForward = 0.15f; // Dời tiêu điểm nhìn hướng về phía thân súng

        private Quaternion showcasePlayerRotation;
        private TweenCase resetPlayerRotationTweenCase;

        protected override int SelectedIndex => Mathf.Clamp(WeaponsController.SelectedWeaponIndex, 0, int.MaxValue);

        public void SetWeaponsController(WeaponsController weaponController)
        {
            this.weaponController = weaponController;
        }

        public void UpdateUI()
        {
            itemPanels.ForEach(panel => panel.UpdateUI());

            if (detailsPanel != null && WeaponsController.Database != null && previewWeaponIndex < WeaponsController.Database.Weapons.Length)
            {
                var weaponData = WeaponsController.Database.Weapons[previewWeaponIndex];
                var upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
                detailsPanel.DisplayWeapon(weaponData, upgrade, previewWeaponIndex);
            }
        }

        public override WeaponPanelUI GetPanel(WeaponType weaponType)
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].Data.Type == weaponType)
                    return itemPanels[i];
            }

            return null;
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        protected override void EnableGamepadButtonTag()
        {
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Weapons);
        }

        #region UI Page

        private float defaultLeftPanelX = 85f;
        private float defaultRightPanelX = -65f;

        public override void Initialise()
        {
            if (canvas == null) CacheComponents();
            base.Initialise();

            // Chỉ xây dựng layout nếu chưa có sẵn trong prefab
            if (detailsPanel == null)
            {
                UIWeaponPageBuilder.BuildLayout(this);
            }

            // Ghi nhớ toạ độ X mà người dùng căn chỉnh trong Editor
            if (leftPanelRectTransform != null)
                defaultLeftPanelX = leftPanelRectTransform.anchoredPosition.x;
            if (backgroundPanelRectTransform != null)
                defaultRightPanelX = backgroundPanelRectTransform.anchoredPosition.x;

            if (detailsPanel != null)
            {
                detailsPanel.Initialise(this);
            }

            // Xóa sạch mọi thẻ cũ hoặc dummy trong panelsContainer để không bao giờ bị nhân đôi
            if (panelsContainer != null)
            {
                for (int i = panelsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(panelsContainer.GetChild(i).gameObject);
                }
            }
            itemPanels.Clear();

            for (int i = 0; i < WeaponsController.Database.Weapons.Length; i++)
            {
                var weapon = WeaponsController.Database.Weapons[i];
                var upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(weapon.UpgradeType);

                var newPanel = AddNewPanel();
                newPanel.Init(weaponController, upgrade as BaseWeaponUpgrade, weapon, i);
            }

            previewWeaponIndex = SelectedIndex;

            WeaponsController.OnWeaponUnlocked += (weapon) => UpdateUI();
            WeaponsController.OnWeaponUpgraded += UpdateUI;
            WeaponsController.OnWeaponCardsAmountChanged += UpdateUI;
        }

        public override void PlayShowAnimation()
        {
            // Subscribe currency changes
            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged += OnCurrencyChangedCallback;
            }

            previewWeaponIndex = SelectedIndex;

            // Slide in Left Panel (-800 -> toạ độ Editor đã chỉnh)
            if (leftPanelRectTransform != null)
            {
                leftPanelRectTransform.anchoredPosition = new Vector2(-800f, 0f);
                leftPanelRectTransform.DOAnchoredPosition(new Vector2(defaultLeftPanelX, 0f), 0.35f)
                    .SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }

            // Slide in Right Panel (800 -> toạ độ Editor đã chỉnh)
            if (backgroundPanelRectTransform != null)
            {
                backgroundPanelRectTransform.anchoredPosition = new Vector2(800f, 0f);
                backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(defaultRightPanelX, 0f), 0.35f)
                    .SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }

            if (scrollView != null)
            {
                scrollView.enabled = true;
                if (scrollView.content != null)
                {
                    scrollView.content.anchoredPosition = Vector2.zero;
                    scrollView.StopMovement();
                }
            }

            // Kich hoat va hien thi toan bo cac the súng trong danh sach cot phai
            for (int i = 0; i < itemPanels.Count; i++)
            {
                if (itemPanels[i] != null)
                {
                    itemPanels[i].transform.localScale = Vector3.one;
                    itemPanels[i].gameObject.SetActive(true);
                    itemPanels[i].OnPanelOpened();
                }
            }

            UIGeneralPowerIndicator.Show();
            if (UIMainMenu.DotsBackground != null)
            {
                UIMainMenu.DotsBackground.gameObject.SetActive(false);
            }

            // Cap nhat thong so goc nhin moi nhat (nhin tu goc ben kia va nang cao cam)
            characterYawAngle = -35f;
            cameraAngleOffset = 20f;
            cameraHeight = 0.85f;
            lookAtHeight = 1.15f;
            cameraDistance = 3.0f;
            weaponFocusForward = 0.15f;

            // Xoay nhan vat va kich hoat camera goc nhin phia doi dien tu duoi len de show vu khi
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                characterBehaviour.gameObject.SetActive(true);
                originalPlayerRotation = characterBehaviour.transform.rotation;

                Vector3 playerPos = characterBehaviour.transform.position;
                Vector3 defaultCamPos = CameraController.OriginalCameraPosition;
                if (defaultCamPos == Vector3.zero)
                {
                    Camera cam = CameraController.MainCamera != null ? CameraController.MainCamera : Camera.main;
                    if (cam != null) defaultCamPos = cam.transform.position;
                }

                Vector3 dirToCam = defaultCamPos - playerPos;
                dirToCam.y = 0;
                if (dirToCam.sqrMagnitude > 0.01f)
                {
                    Vector3 baseCamDir = dirToCam.normalized;

                    // Xoay nhan vat huong phan suon va cay sung ve phia camera (goc ben kia)
                    Vector3 charForward = Quaternion.AngleAxis(characterYawAngle, Vector3.up) * baseCamDir;
                    characterBehaviour.transform.rotation = Quaternion.LookRotation(charForward);
                    showcasePlayerRotation = characterBehaviour.transform.rotation;

                    // Camera dat lech sang goc ben kia (+20°) va o do cao ngang that lung (0.85m) ngung nhe len nguc & vu khi
                    Vector3 camDir = Quaternion.AngleAxis(cameraAngleOffset, Vector3.up) * baseCamDir;
                    Vector3 targetCamPos = playerPos + camDir * cameraDistance + Vector3.up * cameraHeight;
                    Vector3 lookAtTarget = playerPos + Vector3.up * lookAtHeight + charForward * weaponFocusForward;

                    Quaternion targetCamRot = Quaternion.LookRotation((lookAtTarget - targetCamPos).normalized);
                    CameraController.MoveCameraTo(targetCamPos, targetCamRot, 0.5f);

                    Debug.Log($"<color=cyan>[UIWeaponPage] 3D Camera Active: Yaw={characterYawAngle}°, CamOffset={cameraAngleOffset}°, CamHeight={cameraHeight}m, CamPos={targetCamPos}</color>");
                }

                // Tat di chuyen va agent de tranh nguoi choi dieu khien nhan vat trong khi mo UI
                Control.DisableMovementControl();
                characterBehaviour.DisableAgent();

                // An vong tam danh duoi chan nhan vat trong UI chon vu khi
                characterBehaviour.HideAimRing();

                // An drone
                if (characterBehaviour.CurrentDrone != null)
                {
                    characterBehaviour.CurrentDrone.gameObject.SetActive(false);
                }

                // An UI mau tren dau nhan vat
                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.ForceDisable();
                }
            }

            // An Showcase cu neu co
            if (showcaseBehaviour != null)
            {
                showcaseBehaviour.Clear();
                showcaseBehaviour.gameObject.SetActive(false);
            }

            // Preview sung mac dinh dang chon tren tay nhan vat
            SelectWeaponForPreview(previewWeaponIndex);

            Tween.DelayedCall(0.5f, () =>
            {
                EnableGamepadButtonTag();
                UIController.OnPageOpened(this);
            });
        }

        protected override void Update()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                if (canvas == null) return;
            }

            if (!canvas.enabled) return;
        }

        public override void PlayHideAnimation()
        {
            base.PlayHideAnimation();

            for (int i = 0; i < CurrenciesController.Currencies.Length; i++)
            {
                CurrenciesController.Currencies[i].OnCurrencyChanged -= OnCurrencyChangedCallback;
            }

            // Slide out Left Panel
            if (leftPanelRectTransform != null)
            {
                leftPanelRectTransform.DOAnchoredPosition(new Vector2(-800f, 0f), 0.25f)
                    .SetEasing(Ease.Type.CubicIn);
            }

            // An / Clear Showcase neu co
            if (showcaseBehaviour != null)
            {
                showcaseBehaviour.Clear();
                showcaseBehaviour.gameObject.SetActive(false);
            }

            resetPlayerRotationTweenCase.KillActive();

            // Khoi phuc nhan vat sanh voi sung da trang bi va tra camera ve goc nhin sanh
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                characterBehaviour.transform.rotation = originalPlayerRotation;
                characterBehaviour.SetGun(WeaponsController.GetCurrentWeapon(), true);
                Control.EnableMovementControl();
                characterBehaviour.ActivateAgent();

                // Mo lai vong tam danh duoi chan nhan vat khi tat UI chon vu khi
                characterBehaviour.ShowAimRing();

                if (characterBehaviour.CurrentDrone != null)
                {
                    characterBehaviour.CurrentDrone.gameObject.SetActive(true);
                }

                if (characterBehaviour.HealthbarBehaviour != null)
                {
                    characterBehaviour.HealthbarBehaviour.EnableBar(true);
                }
            }

            // Tra camera ve goc nhin sanh
            CameraController.ExitCharacterSelection();

            // Slide out Right Panel
            if (backgroundPanelRectTransform != null)
            {
                backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(800f, 0f), 0.25f)
                    .SetEasing(Ease.Type.CubicIn).OnComplete(delegate
                    {
                        UIController.OnPageClosed(this);
                    });
            }
            else
            {
                UIController.OnPageClosed(this);
            }
        }

        private void OnCurrencyChangedCallback(Currency currency, int difference)
        {
            UpdateUI();
        }

        protected override void HidePage(SimpleCallback onFinish)
        {
            UIController.HidePage<UIWeaponPage>(onFinish);
        }

        #endregion

        #region Preview & Equip Actions

        public void SelectWeaponForPreview(int weaponIndex)
        {
            previewWeaponIndex = weaponIndex;

            if (WeaponsController.Database == null || weaponIndex >= WeaponsController.Database.Weapons.Length) return;

            var weaponData = WeaponsController.Database.Weapons[weaponIndex];
            var upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);

            // Cap nhat cot trai
            if (detailsPanel != null)
            {
                detailsPanel.DisplayWeapon(weaponData, upgrade, weaponIndex);
            }

            // Thay doi sung tren tay nhan vat ngay lap tuc de xem truoc trong khong gian 3D
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                characterBehaviour.SetGun(weaponData, true);
                characterBehaviour.HideAimRing();
                characterBehaviour.Weapon?.ApplyOutline();
            }

            // An showcase bay lo lung cu neu co
            if (showcaseBehaviour != null)
            {
                showcaseBehaviour.Clear();
                showcaseBehaviour.gameObject.SetActive(false);
            }

            // Cap nhat vien highlight tren the cot phai
            for (int i = 0; i < itemPanels.Count; i++)
            {
                itemPanels[i].UpdateSelectionState();
            }
        }

        public void EquipWeapon(int weaponIndex)
        {
            // Thuc su trang bi cho nhan vat ben ngoai!
            weaponController.OnWeaponSelected(weaponIndex);
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            var weaponData = WeaponsController.Database.Weapons[weaponIndex];
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null && weaponData != null)
            {
                characterBehaviour.SetGun(weaponData, true);
            }

            for (int i = 0; i < itemPanels.Count; i++)
            {
                itemPanels[i].UpdateSelectionState();
            }

            if (detailsPanel != null)
            {
                var upgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
                detailsPanel.UpdateEquipButton(upgrade.UpgradeLevel > 0);
            }

            UIGeneralPowerIndicator.UpdateText(true);
        }

        public void UpgradeWeaponWithCoins(WeaponData data, BaseWeaponUpgrade upgrade, int weaponIndex)
        {
            if (upgrade == null || upgrade.NextStage == null) return;

            int price = upgrade.NextStage.Price;
            if (CurrenciesController.HasAmount(CurrencyType.Coins, price))
            {
                CurrenciesController.Add(CurrencyType.Coins, -price);
                upgrade.UpgradeStage();

                weaponController.WeaponUpgraded(data);
                AudioController.PlaySound(AudioController.Sounds.upgrade);
                UIGeneralPowerIndicator.UpdateText(true);

                SelectWeaponForPreview(weaponIndex);
                UpdateUI();
            }
        }

        public void UnlockWeaponWithCards(WeaponData data, BaseWeaponUpgrade upgrade, int weaponIndex)
        {
            if (upgrade == null || upgrade.NextStage == null) return;

            if (upgrade.UpgradeLevel == 0 && data.CardsAmount >= upgrade.NextStage.Price)
            {
                upgrade.UpgradeStage();
                AudioController.PlaySound(AudioController.Sounds.upgrade);
                UIGeneralPowerIndicator.UpdateText(true);

                SelectWeaponForPreview(weaponIndex);
                UpdateUI();
            }
        }

        #endregion

        #region Drag to Rotate 360

        public void OnDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                resetPlayerRotationTweenCase.KillActive();
                float rotationSpeed = -0.5f;
                characterBehaviour.transform.Rotate(Vector3.up, eventData.delta.x * rotationSpeed, Space.World);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            if (characterBehaviour != null)
            {
                Quaternion startRot = characterBehaviour.transform.rotation;
                resetPlayerRotationTweenCase.KillActive();
                resetPlayerRotationTweenCase = Tween.DoFloat(0f, 1f, 0.4f, (float t) =>
                {
                    if (characterBehaviour != null)
                    {
                        characterBehaviour.transform.rotation = Quaternion.Slerp(startRot, showcasePlayerRotation, t);
                    }
                }).SetEasing(Ease.Type.QuadOut);
            }
        }

        #endregion
    }
}
