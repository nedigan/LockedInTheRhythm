using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    private Transform _cameraTransform;

    [SerializeField] private float _speed = 10f;
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
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate the camera's forward direction, ignoring the Y component
        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Calculate the camera's right direction, ignoring the Y component
        Vector3 right = _cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        // Calculate the direction based on camera's forward and right vectors
        Vector3 direction = forward * vertical + right * horizontal;
        direction.Normalize(); // Ensure the direction vector has a magnitude of 1


        _controller.Move(direction * _speed * Time.deltaTime);
    }
}
