using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Broadcaster : Interactable, IHandleGameState
{
    [SerializeField] private UnityEvent _gameWinEvent;
    private bool _locked = true;
    public void ChangeState(GameState state)
    {
        if (state == GameState.EndState)
        {
            _locked = false;
        }
        else if (state == GameState.MainState) 
        {
            _locked = true; 
        }
    }

    public override void Highlight(bool highlight)
    {
        
    }

    public override void Interact()
    {
        if (_locked)
            return;

        _gameWinEvent?.Invoke();
    }

}
