using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Lives next to the EventSystem. Keeps gamepad / keyboard navigation alive:
/// - re-selects the top <see cref="UINavigationPanel"/>'s default when the selection is lost
///   (background click, selected button hidden, ...) and the player uses the gamepad / keyboard;
/// - routes Cancel (B / Escape) and Start to the top panel;
/// - with no panel open, Start / Escape raises <see cref="_onPauseRequested"/> (in-game pause).
/// </summary>
[RequireComponent(typeof(EventSystem))]
[DefaultExecutionOrder(1000)] // after EventSystem: a "waking" press is never also processed as Move / Submit
public class UINavigationManager : MonoBehaviour
{
    /// <summary>True when the last UI input came from a mouse or touch screen.</summary>
    public static bool UsingPointer { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => UsingPointer = false;

    [SerializeField] private InputSystemUIInputModule _inputModule;

    [Tooltip("Invoked by Start / Escape when no navigation panel is open (gameplay). Hook UIHandler.TogglePause here.")]
    [SerializeField] private UnityEvent _onPauseRequested = new();

    private EventSystem _eventSystem;

    private void Awake()
    {
        _eventSystem = GetComponent<EventSystem>();
        if (_inputModule == null) _inputModule = GetComponent<InputSystemUIInputModule>();
    }

    private void Update()
    {
        if (_eventSystem == null) return;
        if (EventSystem.current != _eventSystem) return;

        bool navigationInput = DetectInputDevice();
        var top = UINavigationPanel.Top;

        if (top != null)
        {
            var selected = _eventSystem.currentSelectedGameObject;
            if (UINavigationPanel.IsSelectable(selected))
            {
                top.TrackSelection(selected);
            }
            else if (navigationInput && !top.IsWaitingForSelection)
            {
                // First gamepad input after a mouse click / lost focus only "wakes" the selection.
                top.RestoreSelection();
                _wakeTarget = _eventSystem.currentSelectedGameObject;
                return; // don't treat this press as Cancel / Pause either
            }
        }

        HandleCancelAndPause(top);
    }

    private GameObject _wakeTarget;

    private void LateUpdate()
    {
        // The input module may have processed the same Move / Submit this frame and already left the
        // restored item: put the selection back so the waking press doesn't also move or click.
        if (_wakeTarget == null) return;
        if (_eventSystem.currentSelectedGameObject != _wakeTarget && UINavigationPanel.IsSelectable(_wakeTarget))
            _eventSystem.SetSelectedGameObject(_wakeTarget);
        _wakeTarget = null;
    }

    private void HandleCancelAndPause(UINavigationPanel top)
    {
        bool cancel = WasPressed(_inputModule != null ? _inputModule.cancel : null);
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        if (!cancel && !start) return;

        if (top != null)
        {
            if (top.HasCancel) top.Cancel();
            return;
        }

        // No menu open: Start (or Escape, which is bound to Cancel) toggles the in-game pause.
        _onPauseRequested.Invoke();
    }

    /// <summary>Updates <see cref="UsingPointer"/>. Returns true if a gamepad / keyboard UI input happened this frame.</summary>
    private bool DetectInputDevice()
    {
        bool navigation =
            ReadMove() ||
            WasPressed(_inputModule != null ? _inputModule.submit : null) ||
            WasPressed(_inputModule != null ? _inputModule.cancel : null) ||
            (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame && AnyGamepadButton(Gamepad.current));

        if (navigation)
        {
            UsingPointer = false;
            return true;
        }

        var mouse = Mouse.current;
        if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.delta.ReadValue().sqrMagnitude > 4f))
            UsingPointer = true;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            UsingPointer = true;

        return false;
    }

    private bool ReadMove()
    {
        var move = _inputModule != null ? _inputModule.move : null;
        if (move == null || move.action == null) return false;
        return move.action.ReadValue<Vector2>().sqrMagnitude > 0.25f;
    }

    private static bool AnyGamepadButton(Gamepad pad) =>
        pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
        pad.buttonNorth.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame ||
        pad.startButton.wasPressedThisFrame || pad.dpad.ReadValue().sqrMagnitude > 0.25f ||
        pad.leftStick.ReadValue().sqrMagnitude > 0.25f;

    private static bool WasPressed(InputActionReference reference) =>
        reference != null && reference.action != null && reference.action.WasPressedThisFrame();
}
