using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _maxStaminaRefillSpeed = 5.0f;
    [SerializeField] private PlayerMovement _playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _playerMovement.SetStamina(Mathf.MoveTowards(_playerMovement.StaminaPercentage, 1f, (_curve.Evaluate(_slider.value) * _maxStaminaRefillSpeed) * 0.1f * Time.deltaTime));
    }
}
