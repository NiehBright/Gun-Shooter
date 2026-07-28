using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class EquipmentPanelUI : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] EquipmentDatabase database;

        [Header("Equipment Slots")]
        [SerializeField] EquipmentSlotUI hatSlot;
        [SerializeField] EquipmentSlotUI armorSlot;
        [SerializeField] EquipmentSlotUI glovesSlot;
        [SerializeField] EquipmentSlotUI shoesSlot;

        [Header("Inventory")]
        [SerializeField] Transform inventoryContainer;
        [SerializeField] GameObject inventoryItemPrefab;

        [Header("Character Stats Area")]
        [SerializeField] Image charPreviewImage;
        [SerializeField] Text charNameText;
        [SerializeField] Text charStarsText;
        [SerializeField] Text charHpValueText;
        [SerializeField] Text charDmgValueText;
        [SerializeField] Text coinsText; // Ô hiển thị vàng ở góc phải

        [Header("Category Filter Buttons")]
        [SerializeField] Button filterAllBtn;
        [SerializeField] Button filterHatBtn;
        [SerializeField] Button filterArmorBtn;
        [SerializeField] Button filterGlovesBtn;
        [SerializeField] Button filterShoesBtn;

        [Header("Popup")]
        [SerializeField] EquipmentActionPopup actionPopup;

        [Header("Close Panel")]
        [SerializeField] Button closeButton;

        private List<EquipmentItemUI> inventoryItems = new List<EquipmentItemUI>();
        private static EquipmentPanelUI instance;
        public static EquipmentPanelUI Instance => instance;

        private Canvas panelCanvas;
        private GraphicRaycaster panelRaycaster;
        private CanvasGroup panelCanvasGroup;

        private EquipmentType? currentFilter = null;

        private void Awake()
        {
            Debug.Log("[EquipmentPanelUI] Awake triggered.");
            instance = this;

            panelCanvas = GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = gameObject.AddComponent<Canvas>();

            panelRaycaster = GetComponent<GraphicRaycaster>();
            if (panelRaycaster == null) panelRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (closeButton == null)
            {
                // Tìm tự động đối tượng CloseButton trong các con để đề phòng mất liên kết Reference trong Inspector
                Transform closeBtnTrans = null;
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "CloseButton")
                    {
                        closeBtnTrans = child;
                        break;
                    }
                }

                if (closeBtnTrans != null)
                {
                    closeButton = closeBtnTrans.GetComponent<Button>();
                    if (closeButton == null)
                    {
                        closeButton = closeBtnTrans.gameObject.AddComponent<Button>();
                    }
                }
            }

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true; // Đảm bảo nút CloseButton luôn nhận được sự kiện click chuột
                }
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
                Debug.Log("[EquipmentPanelUI] Close button bound successfully!");
            }
            else
            {
                Debug.LogError("[EquipmentPanelUI] Close button not found in hierarchy!");
            }

            // Tự động tắt Raycast Target trên các thành phần Text tĩnh để tránh chặn click chuột của các nút (như nút Close ở góc)
            foreach (var txt in GetComponentsInChildren<Text>(true))
            {
                if (txt.GetComponentInParent<Button>() == null)
                {
                    txt.raycastTarget = false;
                }
            }

#if UNITY_EDITOR
            if (database == null)
            {
                database = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentDatabase>(
                    "Assets/Project Data/Content/Data/Equipment/Equipment Database.asset");
            }
#endif

            if (EquipmentController.Instance == null)
            {
                Debug.Log("[EquipmentPanelUI] Creating dynamic EquipmentController.");
                GameObject controllerObj = new GameObject("[EQUIPMENT CONTROLLER]");
                controllerObj.AddComponent<EquipmentController>();
                controllerObj.AddComponent<EquipmentStatsApplier>();
            }

            BindFilterButtons();
            HideImmediate();
        }

        private void OnEnable()
        {
            EquipmentController.OnEquipmentChanged += RefreshUI;
            CharactersController.OnCharacterSelectedEvent += OnCharacterSelected;
        }

        private void OnDisable()
        {
            EquipmentController.OnEquipmentChanged -= RefreshUI;
            CharactersController.OnCharacterSelectedEvent -= OnCharacterSelected;
        }

        private void OnCharacterSelected(CharacterType type, Character character)
        {
            Debug.Log("[EquipmentPanelUI] Character selected changed: " + type);
            RefreshUI();
        }

        private void BindFilterButtons()
        {
            if (filterAllBtn != null) filterAllBtn.onClick.AddListener(() => FilterCategory(null));
            if (filterHatBtn != null) filterHatBtn.onClick.AddListener(() => FilterCategory(EquipmentType.Hat));
            if (filterArmorBtn != null) filterArmorBtn.onClick.AddListener(() => FilterCategory(EquipmentType.Armor));
            if (filterGlovesBtn != null) filterGlovesBtn.onClick.AddListener(() => FilterCategory(EquipmentType.Gloves));
            if (filterShoesBtn != null) filterShoesBtn.onClick.AddListener(() => FilterCategory(EquipmentType.Shoes));
        }

        private void FilterCategory(EquipmentType? type)
        {
            currentFilter = type;
            RefreshInventory();
            UpdateFilterBtnColors();
        }

        private void UpdateFilterBtnColors()
        {
            Color selectedCol = new Color(0.9f, 0.7f, 0.2f, 1f);
            Color normalCol = new Color(0.25f, 0.25f, 0.35f, 1f);

            if (filterAllBtn != null) filterAllBtn.GetComponent<Image>().color = currentFilter == null ? selectedCol : normalCol;
            if (filterHatBtn != null) filterHatBtn.GetComponent<Image>().color = currentFilter == EquipmentType.Hat ? selectedCol : normalCol;
            if (filterArmorBtn != null) filterArmorBtn.GetComponent<Image>().color = currentFilter == EquipmentType.Armor ? selectedCol : normalCol;
            if (filterGlovesBtn != null) filterGlovesBtn.GetComponent<Image>().color = currentFilter == EquipmentType.Gloves ? selectedCol : normalCol;
            if (filterShoesBtn != null) filterShoesBtn.GetComponent<Image>().color = currentFilter == EquipmentType.Shoes ? selectedCol : normalCol;
        }

        public static void Show()
        {
            Debug.Log("[EquipmentPanelUI] Show static method called.");
            if (instance == null)
            {
                Debug.LogError("[EquipmentPanelUI] Show failed: instance is null! Is the script active in the scene?");
                return;
            }

            instance.panelCanvas.enabled = true;
            instance.panelRaycaster.enabled = true;
            instance.panelCanvasGroup.alpha = 1f;
            instance.panelCanvasGroup.blocksRaycasts = true;
            instance.panelCanvasGroup.interactable = true;

            instance.currentFilter = null;
            instance.RefreshUI();
            instance.UpdateFilterBtnColors();
        }

        public void Close()
        {
            Debug.Log("[EquipmentPanelUI] Close panel called.");
            if (actionPopup != null)
                actionPopup.Hide();

            HideImmediate();
        }

        private void HideImmediate()
        {
            if (actionPopup != null) actionPopup.Hide();
            if (panelCanvas != null) panelCanvas.enabled = false;
            if (panelRaycaster != null) panelRaycaster.enabled = false;
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }
        }

        public void RefreshUI()
        {
            Debug.Log("[EquipmentPanelUI] RefreshUI called.");
            RefreshSlot(hatSlot, EquipmentType.Hat);
            RefreshSlot(armorSlot, EquipmentType.Armor);
            RefreshSlot(glovesSlot, EquipmentType.Gloves);
            RefreshSlot(shoesSlot, EquipmentType.Shoes);

            RefreshInventory();
            RefreshCharacterStats();
        }

        private void RefreshSlot(EquipmentSlotUI slot, EquipmentType type)
        {
            if (slot == null) return;

            var equippedItem = EquipmentController.GetEquippedItem(type);
            int level = 0;

            if (equippedItem != null && EquipmentController.SaveData != null)
            {
                var saveItem = EquipmentController.SaveData.GetItem(equippedItem.ItemID);
                if (saveItem != null) level = saveItem.level;
            }

            slot.Setup(equippedItem, type, level, OnSlotClicked);
        }

        private void RefreshInventory()
        {
            foreach (var item in inventoryItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            inventoryItems.Clear();

            if (EquipmentController.SaveData == null || inventoryItemPrefab == null || inventoryContainer == null) return;

            var sortedItems = new List<EquipmentSaveItem>(EquipmentController.SaveData.OwnedItems);
            sortedItems.Sort((a, b) =>
            {
                var dataA = EquipmentController.Database?.GetEquipmentByID(a.itemID);
                var dataB = EquipmentController.Database?.GetEquipmentByID(b.itemID);
                if (dataA == null && dataB == null) return 0;
                if (dataA == null) return 1;
                if (dataB == null) return -1;

                int typeCompare = dataA.EquipmentType.CompareTo(dataB.EquipmentType);
                if (typeCompare != 0) return typeCompare;

                int rarityCompare = dataB.Rarity.CompareTo(dataA.Rarity);
                if (rarityCompare != 0) return rarityCompare;

                return b.level.CompareTo(a.level);
            });

            foreach (var saveItem in sortedItems)
            {
                var data = EquipmentController.Database?.GetEquipmentByID(saveItem.itemID);
                if (data == null) continue;

                if (currentFilter.HasValue && data.EquipmentType != currentFilter.Value)
                    continue;

                // Kiểm tra xem trang bị này có đang mặc không
                bool isEquipped = EquipmentController.SaveData.IsEquipped(saveItem.itemID);
                int countInInventory = saveItem.count;

                // Nếu đang mặc 1 chiếc, ta ẩn chiếc đang mặc đi (chỉ hiển thị những chiếc còn dư trong kho đồ)
                if (isEquipped)
                {
                    countInInventory -= 1;
                }

                if (countInInventory <= 0)
                {
                    continue; // Ẩn hoàn toàn khỏi kho đồ nếu không còn dư chiếc nào
                }

                GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryContainer);
                itemObj.SetActive(true);

                var itemUI = itemObj.GetComponent<EquipmentItemUI>();
                if (itemUI != null)
                {
                    // Các vật phẩm hiển thị trong kho đồ lúc này chắc chắn là chưa mặc
                    itemUI.Setup(data, saveItem.level, false, OnInventoryItemClicked);
                    inventoryItems.Add(itemUI);
                }
            }
        }

        private void RefreshCharacterStats()
        {
            var character = CharactersController.SelectedCharacter;
            if (character == null)
            {
                Debug.LogWarning("[EquipmentPanelUI] SelectedCharacter is null!");
                return;
            }

            Debug.Log("[EquipmentPanelUI] Selected character name: " + character.Name);
            if (charNameText != null) charNameText.text = character.Name;
            if (charStarsText != null)
            {
                string stars = "";
                int count = Mathf.Clamp(character.Save != null ? character.Save.UpgradeLevel + 1 : 1, 1, 5);
                for (int i = 0; i < count; i++) stars += "★";
                charStarsText.text = stars;
            }

            if (charPreviewImage != null && character.GetCurrentStage() != null)
            {
                charPreviewImage.sprite = character.GetCurrentStage().PreviewSprite;
                charPreviewImage.enabled = charPreviewImage.sprite != null;
            }

            // Cập nhật số lượng vàng ở góc phải
            if (coinsText != null)
            {
                coinsText.text = CurrenciesController.Get(CurrencyType.Coins).ToString();
            }

            var charStats = character.Upgrades[character.Save.UpgradeLevel].Stats;
            float charHP = charStats.BaseHealth;
            var bonusStats = EquipmentController.GetTotalBonusStats();
            float equipHP = bonusStats.bonusHP;

            float baseDmg = 100f;
            var activeWeapon = WeaponsController.Database.Weapons[WeaponsController.SelectedWeaponIndex];
            if (activeWeapon != null)
            {
                var stage = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(activeWeapon.UpgradeType).GetCurrentStage();
                if (stage != null)
                {
                    baseDmg = (stage.Damage.firstValue + stage.Damage.secondValue) / 2f;
                }
            }

            // Chỉ số cơ bản = sát thương vũ khí * hệ số sát thương nhân vật
            float charDmgMult = charStats.BaseBulletDamageMultiplier;
            float finalBaseDmg = baseDmg * charDmgMult;
            float equipDmg = finalBaseDmg * (bonusStats.bonusDamagePercent / 100f);

            if (charHpValueText != null)
            {
                charHpValueText.text = equipHP > 0 ? $"{charHP} (+{equipHP})" : $"{charHP}";
            }

            if (charDmgValueText != null)
            {
                charDmgValueText.text = equipDmg > 0 ? $"{Mathf.RoundToInt(finalBaseDmg)} (+{Mathf.RoundToInt(equipDmg)})" : $"{Mathf.RoundToInt(finalBaseDmg)}";
            }
        }

        private void OnSlotClicked(EquipmentSlotUI slot, EquipmentData item, EquipmentType type)
        {
            if (item != null)
            {
                if (actionPopup != null)
                    actionPopup.ShowForEquipped(item, type);
            }
            else
            {
                FilterCategory(type);
            }
        }

        private void OnInventoryItemClicked(EquipmentItemUI itemUI, EquipmentData data, bool isEquipped)
        {
            if (actionPopup != null)
                actionPopup.ShowForUnequipped(data);
        }
    }
}
