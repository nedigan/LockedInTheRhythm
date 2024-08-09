using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeTest : MonoBehaviour
{
    private PlayerInput _playerInput;

    private InputAction _touchPosAction;
    private InputAction _touchPressAction;

    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _isSwiping;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _touchPosAction = _playerInput.actions["TouchPosition"];
        _touchPressAction = _playerInput.actions["TouchPress"];
    }

    private void OnEnable()
    {
        _touchPressAction.started += StartTouch;
        _touchPressAction.canceled += EndTouch;
    }

    private void OnDisable()
    {
        _touchPressAction.started -= StartTouch;
        _touchPressAction.canceled -= EndTouch;
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        _isSwiping = true;
        _startPosition = _touchPosAction.ReadValue<Vector2>();
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        if (_isSwiping)
        {
            _isSwiping = false;
            _endPosition = _touchPosAction.ReadValue<Vector2>();
            DetectSwipe();
        }
    }

    private void DetectSwipe()
    {
        Vector2 swipeVector = _endPosition - _startPosition;

        if (swipeVector.magnitude > 50) // Minimum distance for swipe detection
        {
            float angle = Vector2.SignedAngle(Vector2.up, swipeVector);

            if (angle > -45 && angle <= 45)
            {
                Debug.Log("Swipe Up");
            }
            else if (angle > 45 && angle <= 135)
            {
                Debug.Log("Swipe Left");
            }
            else if (angle > -135 && angle <= -45)
            {
                Debug.Log("Swipe Right");
            }
            else
            {
                Debug.Log("Swipe Down");
            }
        }
    }
}
