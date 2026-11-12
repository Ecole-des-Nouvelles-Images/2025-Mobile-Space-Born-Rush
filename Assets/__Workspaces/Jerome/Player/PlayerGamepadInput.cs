using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;

public class PlayerGamepadInput : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;

    private bool _wasLeft, _wasRight, _wasUp, _wasDown;

    private void Awake()
    {
        if (_playerController == null) _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_playerController == null || _playerController.IsPaused) return;

        // 1. Try to find a standard Gamepad first
        Gamepad gamepad = Gamepad.current ?? Gamepad.all.FirstOrDefault();

        // 2. If no Gamepad, look for a generic Joystick
        Joystick joystick = null;
        if (gamepad == null)
        {
            joystick = Joystick.current ?? Joystick.all.FirstOrDefault();
        }

        if (gamepad == null && joystick == null)
        {
            // No controller detected at all
            return;
        }

        // --- Read Inputs ---
        Vector2 stick = Vector2.zero;
        bool dpadLeft = false, dpadRight = false, dpadUp = false, dpadDown = false;

        if (gamepad != null)
        {
            // Read from standard Gamepad
            stick = gamepad.leftStick.ReadValue();
            dpadLeft = gamepad.dpad.left.isPressed || gamepad.dpad.left.ReadValue() > 0.5f;
            dpadRight = gamepad.dpad.right.isPressed || gamepad.dpad.right.ReadValue() > 0.5f;
            dpadUp = gamepad.dpad.up.isPressed || gamepad.dpad.up.ReadValue() > 0.5f;
            dpadDown = gamepad.dpad.down.isPressed || gamepad.dpad.down.ReadValue() > 0.5f;
        }
        else if (joystick != null)
        {
            // Read from generic Joystick (Best effort mapping)
            // The Joystick class usually exposes the first two axes as "stick"
            stick = joystick.stick.ReadValue();
            
            // Generic joysticks often map their D-pad or Hat switch to buttons
            // We will check the first few buttons as a fallback.
            // This is not perfect, but it works for many generic controllers.
            if (joystick.allControls.Count > 0)
            {
                // Note: You may need to adjust these indices (1-8) based on your specific controller's generic mapping.
                // You can use the Input Debugger to see which generic button corresponds to your D-pad.
                dpadLeft = IsButtonPressed(joystick, 5); // Example: Button 5 = Left
                dpadRight = IsButtonPressed(joystick, 6); // Example: Button 6 = Right
                dpadUp = IsButtonPressed(joystick, 7);    // Example: Button 7 = Up
                dpadDown = IsButtonPressed(joystick, 8);  // Example: Button 8 = Down
            }
        }

        // --- Process Inputs (Discrete edge detection) ---
        bool inputLeft = (stick.x < -0.5f && Mathf.Abs(stick.x) > Mathf.Abs(stick.y)) || dpadLeft;
        bool inputRight = (stick.x > 0.5f && Mathf.Abs(stick.x) > Mathf.Abs(stick.y)) || dpadRight;
        bool inputUp = (stick.y > 0.5f && Mathf.Abs(stick.y) > Mathf.Abs(stick.x)) || dpadUp;
        bool inputDown = (stick.y < -0.5f && Mathf.Abs(stick.y) > Mathf.Abs(stick.x)) || dpadDown;

        if (inputLeft && !_wasLeft) _playerController.TryMoveLane(-1);
        _wasLeft = inputLeft;

        if (inputRight && !_wasRight) _playerController.TryMoveLane(1);
        _wasRight = inputRight;

        if (inputUp && !_wasUp) _playerController.TryJump();
        _wasUp = inputUp;

        if (inputDown && !_wasDown) _playerController.TrySlide();
        _wasDown = inputDown;
    }

    // Helper to safely check generic joystick buttons
    private bool IsButtonPressed(Joystick stick, int buttonIndex)
    {
        if (stick.allControls.Count > buttonIndex)
        {
            var control = stick.allControls[buttonIndex] as ButtonControl;
            if (control != null) return control.isPressed;
        }
        return false;
    }
}