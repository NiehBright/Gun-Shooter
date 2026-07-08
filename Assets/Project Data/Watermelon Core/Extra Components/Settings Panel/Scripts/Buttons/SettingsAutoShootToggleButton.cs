#pragma warning disable 649

using UnityEngine;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class SettingsAutoShootToggleButton : SettingsButtonBase
    {
        [SerializeField] Image imageRef;

        [Space]
        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite disableSprite;

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (CharacterBehaviour.IsAutoShootActive)
                imageRef.sprite = activeSprite;
            else
                imageRef.sprite = disableSprite;
        }

        public override bool IsActive()
        {
            return true;
        }

        public override void OnClick()
        {
            CharacterBehaviour.IsAutoShootActive = !CharacterBehaviour.IsAutoShootActive;

            UpdateUI();

            // Phát âm thanh nút bấm của hệ thống
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}
