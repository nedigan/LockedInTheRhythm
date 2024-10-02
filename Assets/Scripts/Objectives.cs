using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objectives : MonoBehaviour, IHandleGameState
{
    [SerializeField] private List<Objective> _objectives;
    private Objective _currentObjective;

    private void Start()
    {
        _currentObjective = _objectives[0];
    }

    public void ChangeState(GameState state)
    {
        _currentObjective.gameObject.SetActive(false);

        if (state == GameState.EndState)
        {
            _currentObjective = _objectives[1];
        }
        else if (state == GameState.MainState) 
        {
            _currentObjective = _objectives[0];
        }
        
        _currentObjective.gameObject.SetActive(true);
    }
}
