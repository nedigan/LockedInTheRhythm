using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AccessibleJoystick : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _touchPressAction;
    private InputAction _touchPosAction;

    private Camera _camera;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _camera = Camera.main;

        _touchPressAction = _playerInput.actions["TouchPress"];
        _touchPosAction = _playerInput.actions["TouchPosition"];
    }

    private void OnEnable()
    {
        _touchPressAction.started += PlaceJoystick;
    }

    public void PlaceJoystick(InputAction.CallbackContext context)
    {
        Vector2 touchViewportPos = _camera.ScreenToViewportPoint(_touchPosAction.ReadValue<Vector2>());

        if (touchViewportPos.x <= 0.5f)
        {
            transform.position = _touchPosAction.ReadValue<Vector2>();
        }
    }
}
