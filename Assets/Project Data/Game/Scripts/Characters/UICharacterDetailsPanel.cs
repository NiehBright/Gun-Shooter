using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class UICharacterDetailsPanel : MonoBehaviour
    {
        [Header("Header Info")]
        [SerializeField] TextMeshProUGUI characterNameText;
        [SerializeField] TextMeshProUGUI stageLevelText;
        [SerializeField] GameObject starsContainer;
        [SerializeField] Image[] starImages;
        [SerializeField] Color starActiveColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] Color starInactiveColor = new Color(0.3f, 0.3f, 0.35f, 0.5f);

        [Header("Stats Elements")]
        [SerializeField] TextMeshProUGUI powerText;
        [SerializeField] TextMeshProUGUI powerBonusText;
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] TextMeshProUGUI healthBonusText;
        [SerializeField] TextMeshProUGUI damageMultiplierText;
        [SerializeField] TextMeshProUGUI speedText;

        [Header("Skill Elements")]
        [SerializeField] Image skillIconImage;
        [SerializeField] TextMeshProUGUI skillNameText;
        [SerializeField] TextMeshProUGUI skillTagText;
        [SerializeField] SkillTooltipTrigger skillTooltipTrigger;
        [SerializeField] UICharacterSkillTooltip skillTooltip;

        [Header("Upgrades - Coins")]
        [SerializeField] Button upgradeCoinsButton;
        [SerializeField] Image upgradeCoinsImage;
        [SerializeField] TextMeshProUGUI upgradeCoinsPriceText;
        [SerializeField] Color coinsButtonActiveColor = new Color(0.98f, 0.74f, 0.17f, 1f);
        [SerializeField] Color coinsButtonDisabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);

        [Header("Upgrades - Gems")]
        [SerializeField] Button upgradeGemsButton;
        [SerializeField] Image upgradeGemsImage;
        [SerializeField] TextMeshProUGUI upgradeGemsPriceText;
        [SerializeField] Color gemsButtonActiveColor = new Color(0.3f, 0.85f, 0.95f, 1f);
        [SerializeField] Color gemsButtonDisabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);

        [Header("States")]
        [SerializeField] GameObject upgradesContainer;
        [SerializeField] GameObject maxLevelObject;
        [SerializeField] GameObject lockedObject;
        [SerializeField] TextMeshProUGUI lockedNoticeText;

        private Character currentCharacter;
        private UICharactersPanel parentPanel;

        public Character CurrentCharacter => currentCharacter;
        public Transform UpgradeButtonTransform => upgradeCoinsButton != null ? upgradeCoinsButton.transform : transform;

        private void Awake()
        {
            if (upgradeCoinsButton != null)
                upgradeCoinsButton.onClick.AddListener(OnUpgradeWithCoinsClicked);

            if (upgradeGemsButton != null)
                upgradeGemsButton.onClick.AddListener(OnUpgradeWithGemsClicked);
        }

        public void BindReferences(
            TextMeshProUGUI characterName,
            TextMeshProUGUI stageLevel,
            GameObject starsCont,
            Image[] stars,
            TextMeshProUGUI power,
            TextMeshProUGUI powerBonus,
            TextMeshProUGUI health,
            TextMeshProUGUI healthBonus,
            TextMeshProUGUI damageMultiplier,
            TextMeshProUGUI speed,
            Image skillIcon,
            TextMeshProUGUI skillName,
            TextMeshProUGUI skillTag,
            SkillTooltipTrigger tooltipTrigger,
            UICharacterSkillTooltip tooltip,
            Button coinsBtn,
            Image coinsImg,
            TextMeshProUGUI coinsPrice,
            Button gemsBtn,
            Image gemsImg,
            TextMeshProUGUI gemsPrice,
            GameObject upgradesCont,
            GameObject maxLvl,
            GameObject lockedObj,
            TextMeshProUGUI lockedNotice)
        {
            characterNameText = characterName;
            stageLevelText = stageLevel;
            starsContainer = starsCont;
            starImages = stars;
            powerText = power;
            powerBonusText = powerBonus;
            healthText = health;
            healthBonusText = healthBonus;
            damageMultiplierText = damageMultiplier;
            speedText = speed;
            skillIconImage = skillIcon;
            skillNameText = skillName;
            skillTagText = skillTag;
            skillTooltipTrigger = tooltipTrigger;
            skillTooltip = tooltip;
            upgradeCoinsButton = coinsBtn;
            upgradeCoinsImage = coinsImg;
            upgradeCoinsPriceText = coinsPrice;
            upgradeGemsButton = gemsBtn;
            upgradeGemsImage = gemsImg;
            upgradeGemsPriceText = gemsPrice;
            upgradesContainer = upgradesCont;
            maxLevelObject = maxLvl;
            lockedObject = lockedObj;
            lockedNoticeText = lockedNotice;

            if (upgradeCoinsButton != null)
            {
                upgradeCoinsButton.onClick.RemoveListener(OnUpgradeWithCoinsClicked);
                upgradeCoinsButton.onClick.AddListener(OnUpgradeWithCoinsClicked);
            }

            if (upgradeGemsButton != null)
            {
                upgradeGemsButton.onClick.RemoveListener(OnUpgradeWithGemsClicked);
                upgradeGemsButton.onClick.AddListener(OnUpgradeWithGemsClicked);
            }
        }

        public void Initialise(UICharactersPanel charactersPanel)
        {
            parentPanel = charactersPanel;
        }

        public void DisplayCharacter(Character character)
        {
            currentCharacter = character;
            if (character == null) return;

            // Character Name & Stage
            if (characterNameText != null)
                characterNameText.text = character.Name.ToUpper();

            int stageIndex = character.GetCurrentStageIndex();
            int upgradeLevel = character.GetCurrentUpgradeIndex();

            if (stageLevelText != null)
                stageLevelText.text = $"CẤP {upgradeLevel + 1}  •  GIAI ĐOẠN {stageIndex + 1}";

            // Stars
            if (starImages != null && starImages.Length > 0)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        bool isActive = i <= upgradeLevel;
                        starImages[i].color = isActive ? starActiveColor : starInactiveColor;
                    }
                }
            }

            // Stats
            CharacterUpgrade currentUpgrade = character.GetCurrentUpgrade();
            CharacterUpgrade nextUpgrade = character.GetNextUpgrade();

            if (currentUpgrade != null && currentUpgrade.Stats != null)
            {
                if (powerText != null)
                    powerText.text = currentUpgrade.Stats.Power.ToString();

                if (healthText != null)
                    healthText.text = currentUpgrade.Stats.Health.ToString();

                if (damageMultiplierText != null)
                    damageMultiplierText.text = $"x{currentUpgrade.Stats.BulletDamageMultiplier:0.#}";

                // Speed
                float moveSpeed = 5f;
                CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
                if (characterBehaviour != null && characterBehaviour.ActualMoveSpeed > 0)
                {
                    moveSpeed = characterBehaviour.ActualMoveSpeed;
                }
                if (speedText != null)
                    speedText.text = $"{moveSpeed:0.#}";

                // Bonus previews
                if (nextUpgrade != null && nextUpgrade.Stats != null)
                {
                    int powerDiff = nextUpgrade.Stats.Power - currentUpgrade.Stats.Power;
                    int hpDiff = nextUpgrade.Stats.Health - currentUpgrade.Stats.Health;

                    if (powerBonusText != null)
                    {
                        powerBonusText.text = powerDiff > 0 ? $"+{powerDiff}" : "";
                        powerBonusText.gameObject.SetActive(powerDiff > 0);
                    }

                    if (healthBonusText != null)
                    {
                        healthBonusText.text = hpDiff > 0 ? $"+{hpDiff}" : "";
                        healthBonusText.gameObject.SetActive(hpDiff > 0);
                    }
                }
                else
                {
                    if (powerBonusText != null) powerBonusText.gameObject.SetActive(false);
                    if (healthBonusText != null) healthBonusText.gameObject.SetActive(false);
                }
            }

            // Skill
            CharacterSkillData skillData = character.SkillData;
            if (skillData != null)
            {
                if (skillIconImage != null && skillData.ButtonIcon != null)
                {
                    skillIconImage.sprite = skillData.ButtonIcon;
                    skillIconImage.gameObject.SetActive(true);
                }

                if (skillNameText != null)
                    skillNameText.text = skillData.SkillName;

                if (skillTagText != null)
                    skillTagText.text = skillData.SkillTag;

                if (skillTooltipTrigger != null)
                    skillTooltipTrigger.SetSkillData(skillData, skillTooltip);
            }

            // Check locked state
            bool isUnlocked = character.IsUnlocked();
            if (!isUnlocked)
            {
                if (lockedObject != null) lockedObject.SetActive(true);
                if (upgradesContainer != null) upgradesContainer.SetActive(false);
                if (maxLevelObject != null) maxLevelObject.SetActive(false);
                if (lockedNoticeText != null) lockedNoticeText.text = $"MỞ KHÓA Ở MÀN {character.RequiredLevel}";
                return;
            }

            if (lockedObject != null) lockedObject.SetActive(false);

            // Upgrades & Prices
            UpdateUpgradeButtonsState();
        }

        public void UpdateUpgradeButtonsState()
        {
            if (currentCharacter == null || !currentCharacter.IsUnlocked())
                return;

            if (currentCharacter.IsMaxUpgrade())
            {
                if (upgradesContainer != null) upgradesContainer.SetActive(false);
                if (maxLevelObject != null) maxLevelObject.SetActive(true);
                return;
            }

            if (maxLevelObject != null) maxLevelObject.SetActive(false);
            if (upgradesContainer != null) upgradesContainer.SetActive(true);

            CharacterUpgrade nextUpgrade = currentCharacter.GetNextUpgrade();
            if (nextUpgrade == null) return;

            int coinPrice = nextUpgrade.Price;
            int gemPrice = CalculateGemPrice(coinPrice);

            // Coin Upgrade Button
            bool canAffordCoins = CurrenciesController.HasAmount(CurrencyType.Coins, coinPrice);
            if (upgradeCoinsButton != null)
            {
                upgradeCoinsButton.interactable = canAffordCoins;
                if (upgradeCoinsPriceText != null)
                    upgradeCoinsPriceText.text = CurrenciesHelper.Format(coinPrice);

                if (upgradeCoinsImage != null)
                    upgradeCoinsImage.color = canAffordCoins ? coinsButtonActiveColor : coinsButtonDisabledColor;
            }

            // Gem Upgrade Button
            bool canAffordGems = CurrenciesController.HasAmount(CurrencyType.Gems, gemPrice);
            if (upgradeGemsButton != null)
            {
                upgradeGemsButton.interactable = canAffordGems;
                if (upgradeGemsPriceText != null)
                    upgradeGemsPriceText.text = gemPrice.ToString();

                if (upgradeGemsImage != null)
                    upgradeGemsImage.color = canAffordGems ? gemsButtonActiveColor : gemsButtonDisabledColor;
            }
        }

        public static int CalculateGemPrice(int coinPrice)
        {
            // Tỷ lệ: 40 Coins = 1 Gem, tối thiểu 1 Gem
            return Mathf.Max(1, Mathf.RoundToInt(coinPrice / 40f));
        }

        public void OnUpgradeWithCoinsClicked()
        {
            if (UICharactersPanel.IsControlBlocked || currentCharacter == null || currentCharacter.IsMaxUpgrade())
                return;

            CharacterUpgrade nextUpgrade = currentCharacter.GetNextUpgrade();
            if (nextUpgrade == null) return;

            int coinPrice = nextUpgrade.Price;
            if (!CurrenciesController.HasAmount(CurrencyType.Coins, coinPrice))
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
                return;
            }

            CurrenciesController.Substract(CurrencyType.Coins, coinPrice);
            ExecuteUpgrade();
        }

        public void OnUpgradeWithGemsClicked()
        {
            if (UICharactersPanel.IsControlBlocked || currentCharacter == null || currentCharacter.IsMaxUpgrade())
                return;

            CharacterUpgrade nextUpgrade = currentCharacter.GetNextUpgrade();
            if (nextUpgrade == null) return;

            int gemPrice = CalculateGemPrice(nextUpgrade.Price);
            if (!CurrenciesController.HasAmount(CurrencyType.Gems, gemPrice))
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
                return;
            }

            CurrenciesController.Substract(CurrencyType.Gems, gemPrice);
            ExecuteUpgrade();
        }

        private void ExecuteUpgrade()
        {
            currentCharacter.UpgradeCharacter();

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            CharacterUpgrade currentUpgrade = currentCharacter.GetCurrentUpgrade();

            if (characterBehaviour != null && currentUpgrade != null)
            {
                if (currentUpgrade.ChangeStage)
                {
                    characterBehaviour.SetGraphics(currentCharacter.Stages[currentUpgrade.StageIndex].Prefab, true, true);
                }
                else
                {
                    BaseCharacterGraphics characterGraphics = characterBehaviour.Graphics;
                    if (characterGraphics != null)
                    {
                        characterGraphics.PlayUpgradeParticle();
                        characterGraphics.PlayBounceAnimation();
                    }
                }

                characterBehaviour.SetStats(currentUpgrade.Stats);
            }

            // Refresh view
            DisplayCharacter(currentCharacter);

            UIGeneralPowerIndicator.UpdateText(true);

            if (parentPanel != null)
            {
                parentPanel.OnCharacterUpgradedInternal(currentCharacter);
            }
        }
    }
}
