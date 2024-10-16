using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempoSlider : MonoBehaviour
{
    public static TempoSlider Instance {  get; private set; }

    public float Value { get; private set; }
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private float _highLowDiff = 0.40f;

    // Update is called once per frame
    void Update()
    {
        Value = _highLowDiff + (_slider.value * (1f - _highLowDiff)); //maps the value to 0.4 to 1 so 0 doesnt stop time
    }
}
