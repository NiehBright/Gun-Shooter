using UnityEngine;

namespace Watermelon
{
    public abstract class SettingsButtonBase : MonoBehaviour
    {
        [SerializeField] UIGamepadButton gamepadButton;

        private int index;
        protected SettingsPanel settingsPanel;

        private RectTransform rectTransform;
        public RectTransform RectTransform { get { return rectTransform; } }

        public void Init(int index, SettingsPanel settingsPanel)
        {
            this.index = index;
            this.settingsPanel = settingsPanel;

            this.rectTransform = GetComponent<RectTransform>();
        }

        public void SetGamepadButtonFocus(bool isInFocus)
        {
            if (gamepadButton != null) gamepadButton.SetFocus(isInFocus);
        }

        public abstract bool IsActive();
        public abstract void OnClick();
    }
}

// -----------------
// Settings Panel v 0.3
// -----------------