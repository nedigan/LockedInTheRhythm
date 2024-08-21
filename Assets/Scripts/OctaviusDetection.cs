using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OctaviusDetection : ConeDetection
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _timeUntilPlayerHidden = 5f;

    private float _timeLeftUntilPlayerHidden;
    private bool _isChasingPlayer = false;
    private Transform _playerTransform;

    protected override void PlayerDetected(Transform playerTransform)
    {
        //Debug.Log("PLayer detected");
        _isChasingPlayer = true;
        _playerTransform = playerTransform;
        _timeLeftUntilPlayerHidden = _timeUntilPlayerHidden;
    }

    public override void Update()
    {
        base.Update();
        if (_isChasingPlayer)
        {
            _agent.SetDestination(_playerTransform.position);
            _timeLeftUntilPlayerHidden -= Time.deltaTime;

            if (_timeLeftUntilPlayerHidden <= 0f)
            {
                _isChasingPlayer=false;
                _agent.SetDestination(_agent.transform.position);// set path to itself to stop moving
            }
        }
    }
}
