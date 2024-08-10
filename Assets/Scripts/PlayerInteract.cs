using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerInteract : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Interactable _currentInteractable = null;

    private InputAction _touchPressAction;
    private InputAction _touchPosAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _touchPressAction = _playerInput.actions["TouchPress"];
        _touchPosAction = _playerInput.actions["TouchPosition"];
    }
    private void OnEnable()
    {
        _touchPressAction.started += TestInteract;
    }

    private void TestInteract(InputAction.CallbackContext context)
    {
        Debug.Log(_touchPosAction.ReadValue<Vector2>());
    }
    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        // If the player has entered the trigger of an interactable object
        {
            _currentInteractable = interactable;

            Debug.Log($"Player can interact with: {_currentInteractable.name}");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentInteractable != null && _currentInteractable == other.GetComponent<Interactable>())
        {
            Debug.Log($"Player can no longer interact with: {_currentInteractable.name}");
            _currentInteractable = null;
        }
    }
}
