using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    private Transform _cameraTransform;
    private PlayerInput _playerInput;
    private InputAction _moveAction;

    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _sprintMultipler = 1.5f;
    [SerializeField] private float _maxStamina = 10f;
    [SerializeField] private Slider _sprintMeter;

    private float _stamina;
    private bool _sprinting = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _stamina = _maxStamina;
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

        // If the direction is not zero
        if (direction != Vector3.zero)
        {
            // Normalize the direction to avoid scaling the movement
            direction.Normalize();

            // Set the rotation of the character to look in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            // Move the character
            float speedMultipler;
            if (_sprinting && _stamina > 0f)
            {
                speedMultipler = _sprintMultipler;
                _stamina -= _speed * Time.deltaTime; // TODO: Fix this prolly
            }
            else
                speedMultipler = 1f;

            _controller.Move(direction * _speed * speedMultipler * Time.deltaTime);
        }

        _sprintMeter.value = _stamina / _maxStamina;
    }

    public void SprintButtonDown()
    {
        _sprinting = true;
    }

    public void SprintButtonUp()
    {
        _sprinting = false;
    }
}
