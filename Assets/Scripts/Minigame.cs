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
public class Minigame : MonoBehaviour
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
        int dir = 1;

        do
        {
            dir = Random.Range(1, 5);
        } while (dir == (int)prevDirection);


        switch (dir)
        {
            case 1:
                return SwipeDirection.Up;
            case 2:
                return SwipeDirection.Right;
            case 3:
                return SwipeDirection.Down;
            case 4:
                return SwipeDirection.Left;
        }

        return SwipeDirection.Up; // Shouldnt ever get here.
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
