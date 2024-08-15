using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    private float _speed;

    private Vector2 _startPos;
    private Vector2 _targetPos;

    private float _time = 0f;
    private float _maxTime = 0f;

    public void Start()
    {
        _startPos = transform.position;
    }
    public void Setup(float speed, Vector2 targetPos)
    {
        _speed = speed;
        _targetPos = targetPos;

        _maxTime = Vector2.Distance(_targetPos, _startPos) / (_speed * 10f) ;
    }

    // Update is called once per frame
    void Update()
    {
        if (_time < _maxTime)
        {
            _time += Time.deltaTime;
            float percentage = _time / _maxTime;
            transform.position = Vector2.Lerp(_startPos, _targetPos, percentage);
        }
        else
            Destroy(gameObject);
    }
}
