using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum SwipeDirection
{
    Up,
    Right,
    Down,
    Left
}

[RequireComponent(typeof(SwipeInput))]
public class SwipeGameTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _swipeDirectionText;
    private SwipeInput _swipeInput;
    private SwipeDirection _swipeDirection;

    private void Awake()
    {
        _swipeInput = GetComponent<SwipeInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _swipeInput.Swiped.AddListener(CheckSwipe);

        _swipeDirection = GetRandomSwipeDirection(SwipeDirection.Up);
        SetSwipeDirectionText(_swipeDirection);
    }

    private void CheckSwipe(SwipeDirection direction)
    {
        if (direction == _swipeDirection)
        {
            _swipeDirection = GetRandomSwipeDirection(_swipeDirection);
            SetSwipeDirectionText(_swipeDirection);
        }
    }


    SwipeDirection GetRandomSwipeDirection(SwipeDirection prevDirection) // prev direction to make sure it doesnt do the same direciton
    {
        int dir;

        do
        {
            dir = Random.Range(0, 4);
        } while (dir == (int)prevDirection);

        return (SwipeDirection) dir;
    }

    void SetSwipeDirectionText(SwipeDirection swipeDirection)
    {
        switch (swipeDirection)
        {
            case (SwipeDirection.Up):
                _swipeDirectionText.text = "UP";
                break;
            case (SwipeDirection.Right):
                _swipeDirectionText.text = "RIGHT";
                break;
            case (SwipeDirection.Down):
                _swipeDirectionText.text = "DOWN";
                break;
            case (SwipeDirection.Left):
                _swipeDirectionText.text = "LEFT";
                break;
        }
    }
}
