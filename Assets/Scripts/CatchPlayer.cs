using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CatchPlayer : ConeDetection
{
    [SerializeField] private UnityEvent _onPlayerCaught;
    private bool _caught = false;
    public override void Update()
    {
        base.Update();

        if (DetectingPlayer && !_caught)
        {
            _onPlayerCaught.Invoke();
            _caught = true;
        }
    }
}
