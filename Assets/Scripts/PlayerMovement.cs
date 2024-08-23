using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    private Transform _cameraTransform;
    private PlayerInput _playerInput;
    private InputAction _moveAction;

    [SerializeField] private float _speed = 10f;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
    }

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;   

        Debug.Log(Camera.main.transform.forward.y);
    }

    // Update is called once per frame
    void Update()
    {
        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = _moveAction.ReadValue<Vector2>();

        // Calculate the camera's forward direction, ignoring the Y component
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Calculate the camera's right direction, ignoring the Y component
        Vector3 right = _cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        // Calculate the direction based on camera's forward and right vectors
        Vector3 direction = forward * inputDirection.y + right * inputDirection.x;
        direction.Normalize(); // Ensure the direction vector has a magnitude of 1

        // If the direction is not zero, rotate the character
        if (direction != Vector3.zero)
        {
            // Normalize the direction to avoid scaling the movement
            direction.Normalize();

            // Set the rotation of the character to look in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            // Move the character
            _controller.Move(direction * _speed * Time.deltaTime);
        }
    }
}
