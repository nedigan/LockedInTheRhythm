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

    // Update is called once per frame
    void Update()
    {
        Value = 0.1f + (_slider.value * (1f - 0.1f)); //maps the value to 0.1 to 1 so 0 doesnt stop time
    }
}
