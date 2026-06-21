#pragma warning disable 0067

using UnityEngine;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Watermelon
{
    public class GamepadControl : MonoBehaviour, IControlBehavior
    {
        //Left Stick x and y axes
        public Vector3 MovementInput { get; private set; }
        public bool IsMovementInputNonZero { get; private set; }

        public bool IsMovementControlActive { get; private set; }

        public event SimpleCallback OnMovementInputActivated;

        public void Initialise()
        {
            if (Control.InputType == InputType.Gamepad)
            {
                Control.SetControl(this);

                // As Behavior.enabled, inherited variable
                enabled = true;
                IsMovementControlActive = true;
            }
            else
            {
                enabled = false;
            }
        }

        private void Update()
        {
#if MODULE_INPUT_SYSTEM
            // Dev: not 100% sure this 'if' statement works in every scenario, but so far so good
            if (Gamepad.current == null || Keyboard.current.wasUpdatedThisFrame)
            {
                Control.ChangeInputType(InputType.Keyboard);

                return;
            }

            if (!IsMovementControlActive) return;

            float horizontalInput = Gamepad.current.leftStick.x.value;
            float verticalInput = Gamepad.current.leftStick.y.value;

            MovementInput = Vector3.ClampMagnitude(new Vector3(horizontalInput, 0, verticalInput), 1);

            if (!IsMovementInputNonZero && MovementInput.magnitude > 0.1f)
            {
                IsMovementInputNonZero = true;

                OnMovementInputActivated?.Invoke();
            }

            IsMovementInputNonZero = MovementInput.magnitude > 0.1f;
#endif
        }

        // Dev: reasons for adding this method: it helps to abstract ui gamepad buttons from the actual gamepad buttons, and keeps the code cleaner
        public static bool WasButtonPressedThisFrame(GamepadButtonType button)
        {
#if MODULE_INPUT_SYSTEM
            if(Gamepad.current == null) return false;

            switch (button)
            {
                case GamepadButtonType.A: return Gamepad.current.aButton.wasPressedThisFrame;
                case GamepadButtonType.B: return Gamepad.current.bButton.wasPressedThisFrame;
                case GamepadButtonType.X: return Gamepad.current.xButton.wasPressedThisFrame;
                case GamepadButtonType.Y: return Gamepad.current.yButton.wasPressedThisFrame;

                case GamepadButtonType.Start: return Gamepad.current.startButton.wasPressedThisFrame;

                case GamepadButtonType.DDown: return Gamepad.current.dpad.down.wasPressedThisFrame;
                case GamepadButtonType.DUp: return Gamepad.current.dpad.up.wasPressedThisFrame;
                case GamepadButtonType.DLeft: return Gamepad.current.dpad.left.wasPressedThisFrame;
                case GamepadButtonType.DRight: return Gamepad.current.dpad.right.wasPressedThisFrame;

                case GamepadButtonType.LB: return Gamepad.current.leftShoulder.wasPressedThisFrame;
                case GamepadButtonType.RB: return Gamepad.current.rightShoulder.wasPressedThisFrame;

                case GamepadButtonType.L3: return Gamepad.current.leftStickButton.wasPressedThisFrame;
                case GamepadButtonType.R3: return Gamepad.current.rightStickButton.wasPressedThisFrame;

                default: return false;
            }
#else
            return false;
#endif
        }

        public static bool WasButtonReleasedThisFrame(GamepadButtonType button)
        {
#if MODULE_INPUT_SYSTEM
            if (Gamepad.current == null) return false;

            switch (button)
            {
                case GamepadButtonType.A: return Gamepad.current.aButton.wasReleasedThisFrame;
                case GamepadButtonType.B: return Gamepad.current.bButton.wasReleasedThisFrame;
                case GamepadButtonType.X: return Gamepad.current.xButton.wasReleasedThisFrame;
                case GamepadButtonType.Y: return Gamepad.current.yButton.wasReleasedThisFrame;

                case GamepadButtonType.Start: return Gamepad.current.startButton.wasReleasedThisFrame;

                case GamepadButtonType.DDown: return Gamepad.current.dpad.down.wasReleasedThisFrame;
                case GamepadButtonType.DUp: return Gamepad.current.dpad.up.wasReleasedThisFrame;
                case GamepadButtonType.DLeft: return Gamepad.current.dpad.left.wasReleasedThisFrame;
                case GamepadButtonType.DRight: return Gamepad.current.dpad.right.wasReleasedThisFrame;

                case GamepadButtonType.LB: return Gamepad.current.leftShoulder.wasReleasedThisFrame;
                case GamepadButtonType.RB: return Gamepad.current.rightShoulder.wasReleasedThisFrame;

                case GamepadButtonType.L3: return Gamepad.current.leftStickButton.wasReleasedThisFrame;
                case GamepadButtonType.R3: return Gamepad.current.rightStickButton.wasReleasedThisFrame;

                default: return false;
            }
#else
            return false;
#endif
        }

        #region Control management

        public void DisableMovementControl()
        {
            IsMovementControlActive = false;
        }

        public void EnableMovementControl()
        {
            IsMovementControlActive = true;
        }

        public void ResetControl()
        {
            IsMovementInputNonZero = false;
            MovementInput = Vector3.zero;
        }

#endregion
    }
}