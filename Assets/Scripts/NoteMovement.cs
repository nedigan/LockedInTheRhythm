using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    private float _speed;

    private Minigame _minigame;

    private Vector2 _startPos;
    private Vector2 _targetPos;

    private float _time = 0f;
    public float _maxTime = 0f;

    private int _trackIndex;
    public int TrackIndex => _trackIndex;
    public bool wasHit = false;

    public void Start()
    {
        _startPos = transform.position;
    }
    public void Setup(float speed, Vector2 targetPos, int trackIndex, Minigame minigame)
    {
        _speed = speed;
        _targetPos = targetPos;
        _trackIndex = trackIndex;

       _minigame = minigame;
        minigame.AddNote(this);

        _maxTime = Vector2.Distance(_targetPos, _startPos) / (_speed * 10f) ;
    }

    // Update is called once per frame
    void Update()
    {
        if (_time < _maxTime)
        {
            _time += Time.unscaledDeltaTime;
            float percentage = _time / _maxTime;
            transform.position = Vector2.Lerp(_startPos, _targetPos, percentage);
        }
        else if (!wasHit)
        {
            _minigame.MissedNote(this);
        }
    }
}
