using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class AttackButtonBehavior : Button
    {
        private static AttackButtonBehavior instance;

        [SerializeField] Image radialFillImage;
        [SerializeField] UIGamepadButton uiGamepadButton;

        public static bool IsButtonPressed { get; private set;}

        public static event SimpleBoolCallback onStatusChanged;

        protected override void Awake()
        {
            instance = this;
            radialFillImage = transform.GetChild(0).GetComponent<Image>();
            uiGamepadButton = GetComponent<UIGamepadButton>();
        }

        private void Update()
        {
            if(Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(uiGamepadButton.ButtonType))
                {
                    IsButtonPressed = true;
                    onStatusChanged?.Invoke(true);
                } else if(GamepadControl.WasButtonReleasedThisFrame(uiGamepadButton.ButtonType))
                {
                    IsButtonPressed = false;
                    onStatusChanged?.Invoke(false);
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            IsButtonPressed = false;
            onStatusChanged?.Invoke(false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            IsButtonPressed = true;
            onStatusChanged?.Invoke(true);
        }

        public static void SetReloadFill(float t)
        {
            instance.radialFillImage.fillAmount = t;
        }
    }
}