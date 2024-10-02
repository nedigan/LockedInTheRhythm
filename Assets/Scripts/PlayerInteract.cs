using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerInteract : MonoBehaviour
{
    private Interactable _currentInteractable = null;

    public void Interact()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        // If the player has entered the trigger of an interactable object
        {
            _currentInteractable = interactable;
            _currentInteractable.Highlight(true);

            Debug.Log($"Player can interact with: {_currentInteractable.name}");
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentInteractable != null && _currentInteractable == other.GetComponent<Interactable>())
        {
            Debug.Log($"Player can no longer interact with: {_currentInteractable.name}");
            _currentInteractable.Highlight(false);
            _currentInteractable = null;
        }
    }
}
