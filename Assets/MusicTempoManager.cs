using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTempoManager : MonoBehaviour
{
    [SerializeField] private float _maxTempo = 240f; //1.42857142 pitch
    [SerializeField] private float _minTempo = 96f;
    [SerializeField] private float _trackTempo = 168f;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        _audioSource.pitch = (_minTempo + (TempoSlider.Instance.Value - 0.4f) * (_maxTempo - _minTempo) / (1f - 0.4f)) / _trackTempo;
    }
}
