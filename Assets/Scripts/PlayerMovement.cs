using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IHandleGameState
{
    private CharacterController _controller;
    private Transform _cameraTransform;
    private InputAction _moveAction;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _sprintMultipler = 1.5f;
    [SerializeField] private float _maxStamina = 10f;
    [SerializeField] private Slider _sprintMeter;
    [SerializeField] private GameObject _footprintPrefab;
    [SerializeField] private GameObject _redFootprintPrefab;

    [SerializeField] private List<GameObject> _animations;

    public bool BeingTracked = false;

    private float _stamina;
    public float StaminaPercentage { get { return _stamina / _maxStamina; }}
    private bool _sprinting = false;

    private float _redFootprintTime = 0f;
    [SerializeField] private float _redFootprintTimeMax = 3f;
    private bool _isRedFootprintTime = false;


    private void Awake()
    {
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
        HandleRedFootprintTime();

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
            SetDirection(true);
            // Normalize the direction to avoid scaling the movement
            direction.Normalize();

            // Set the rotation of the character to look in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            if (_stamina < 0.01f)
            {
                Debug.Log("Sprint BUTTON UP");
                SprintButtonUp();
            }

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

            if (BeingTracked && !_sprinting) 
                ManageFootprints();
        }
        else
        {
            SetDirection(false);
        }

        _sprintMeter.value = _stamina / _maxStamina;
    }

    void HandleRedFootprintTime()
    {
        if (CameraDetection.AnyCameraDetecting)
        {
            _redFootprintTime = 0f;
            _isRedFootprintTime = true;
        }

        if (_isRedFootprintTime)
        {
            _redFootprintTime += Time.deltaTime;
        }

        if (_redFootprintTime > _redFootprintTimeMax && _isRedFootprintTime)
        {
            _isRedFootprintTime = false;
        }
    }

    private Transform _lastFootprint;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _footprintStepDistance = 1.5f;
    private void ManageFootprints()
    {
        if (_lastFootprint == null)
        {
            PlaceFootprint();
            return;
        }

        if (Vector3.Distance(transform.position, _lastFootprint.position) > _footprintStepDistance)
        {
            PlaceFootprint();
        }
    }

    private void PlaceFootprint()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, _groundLayerMask))
        {
            if (_isRedFootprintTime)
            {
                _lastFootprint = Instantiate(_redFootprintPrefab, hit.point + Vector3.up * 0.01f, transform.rotation).transform;
            }
            else
                _lastFootprint = Instantiate(_footprintPrefab, hit.point + Vector3.up * 0.01f, transform.rotation).transform;
        }
    }

    void SetAnimation(int index, bool flip)
    {
        for (int i = 0; i < _animations.Count; i++)
        {
            if (i == index)
            {
                _animations[i].SetActive(true);

                if (flip)
                {
                    _animations[i].transform.localScale = new Vector3(_animations[i].transform.localScale.x * -1, _animations[i].transform.localScale.y, _animations[i].transform.localScale.z);
                }
            }
            else
            {
                _animations[i].SetActive(false);
            }
        }
    }
    void SetDirection(bool moving)
    {
        Vector3 forward = transform.forward;

        if (Mathf.Abs(forward.z) > Mathf.Abs(forward.x))
        {
            if (forward.z > 0)
            {
                int activeIndex = 0;
                // forward sprite
                if (moving)
                {
                    activeIndex = 1;
                    
                }
                else
                {
                    activeIndex = 0;
                }

                bool flip = false;
                if (Mathf.Sign(_animations[activeIndex].transform.localScale.x) == -1)
                    flip = true;
                SetAnimation(activeIndex, flip);
            }
            else
            {
                // backward sprite
                int activeIndex = 0;

                if (moving)
                {
                    activeIndex = 3;
                }
                else
                {
                    activeIndex = 2;
                }

                bool flip = false;
                if (Mathf.Sign(_animations[activeIndex].transform.localScale.x) == 1)
                    flip = true;
                SetAnimation(activeIndex, flip);
            }
        }
        else
        {
            if (forward.x > 0)
            {
                // right sprite
                int activeIndex = 0;

                if (moving)
                {
                    activeIndex = 3;

                }
                else
                {
                    activeIndex = 2;
                }

                bool flip = false;
                if (Mathf.Sign(_animations[activeIndex].transform.localScale.x) == -1)
                    flip = true;
                SetAnimation(activeIndex, flip);
            }
            else
            {
                // left sprite
                int activeIndex = 0;

                if (moving)
                {
                    activeIndex = 1;

                }
                else
                {
                    activeIndex = 0;
                }

                bool flip = false;
                if (Mathf.Sign(_animations[activeIndex].transform.localScale.x) == 1)
                    flip = true;
                SetAnimation(activeIndex, flip);
            }
        }
    }

    public void SetStamina(float staminaPercentage)
    {
        _stamina = staminaPercentage * _maxStamina;
    }

    public void SprintButtonDown()
    {
        _sprinting = true;
    }

    public void SprintButtonUp()
    {
        _sprinting = false;
    }

    public void ChangeState(GameState state)
    {
        //if (state == GameState.EndState)
        //{
        //    BeingTracked = true;
        //}
        //else if (state == GameState.MainState)
        //{
        //    BeingTracked = false;
        //}
    }
}
