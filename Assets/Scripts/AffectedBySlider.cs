using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AffectedBySlider : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private string _animatorParameterName = "speedMultiplier";

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat(_animatorParameterName, TempoSlider.Instance.Value);
    }
}
