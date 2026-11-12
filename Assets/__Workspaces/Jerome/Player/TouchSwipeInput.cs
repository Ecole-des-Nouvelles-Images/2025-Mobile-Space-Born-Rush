using UnityEngine;
using UnityEngine.InputSystem;

public class TouchSwipeInput : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InputActionAsset _inputActions; // DRAG TouchControls.inputactions HERE

    private InputAction _touchContact;
    private InputAction _touchPosition;
    
    private Vector2 _touchStartPosition;
    private bool _fingerDown;
    private int _pixelDistToDetect = 20;

    private void Awake()
    {
        if (_playerController == null) _playerController = GetComponent<PlayerController>();

        if (_inputActions != null)
        {
            var map = _inputActions.FindActionMap("Player", true);
            _touchContact = map.FindAction("TouchContact", true);
            _touchPosition = map.FindAction("TouchPosition", true);
        }
    }

    private void OnEnable()
    {
        if (_inputActions == null) return;

        _touchContact.started += OnTouchStarted;
        _touchContact.canceled += OnTouchEnded;
        _touchPosition.performed += OnTouchPositionPerformed;

        _inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        if (_inputActions == null) return;

        _touchContact.started -= OnTouchStarted;
        _touchContact.canceled -= OnTouchEnded;
        _touchPosition.performed -= OnTouchPositionPerformed;

        _inputActions.FindActionMap("Player").Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (_playerController == null || _playerController.IsPaused) return;
        _touchStartPosition = _touchPosition.ReadValue<Vector2>();
        _fingerDown = true;
    }

    private void OnTouchPositionPerformed(InputAction.CallbackContext context)
    {
        if (_playerController == null || _playerController.IsPaused || !_fingerDown) return;

        Vector2 currentTouchPosition = context.ReadValue<Vector2>();
        Vector2 delta = currentTouchPosition - _touchStartPosition;

        if (delta.magnitude >= _pixelDistToDetect)
        {
            ProcessSwipe(delta);
            _fingerDown = false;
        }
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (_playerController == null || _playerController.IsPaused || !_fingerDown) return;

        Vector2 touchEndPosition = _touchPosition.ReadValue<Vector2>();
        Vector2 delta = touchEndPosition - _touchStartPosition;

        if (delta.magnitude >= _pixelDistToDetect)
        {
            ProcessSwipe(delta);
        }
        _fingerDown = false;
    }

    private void ProcessSwipe(Vector2 delta)
    {
        bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

        if (isHorizontal)
        {
            _playerController.TryMoveLane(delta.x > 0 ? 1 : -1);
        }
        else
        {
            if (delta.y > 0) _playerController.TryJump();
            else _playerController.TrySlide();
        }
    }
}