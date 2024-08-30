using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ConeDetection))]
public class OctaviusBehaviour : MonoBehaviour
{
    [SerializeField] private List<Transform> _waypoints = new List<Transform>();
    [SerializeField] private TextMeshProUGUI _timeLeftText;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _timeUntilPlayerHidden = 5f;

    private bool _isChasingPlayer = false;
    private float _timeLeftUntilPlayerHidden;
    private ConeDetection _detection;
    private NavMeshAgent _agent;
    private BehaviourTree _tree;
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _detection = GetComponent<ConeDetection>();

        _tree = new BehaviourTree("Octavius");

        Sequence chaseSequence = new Sequence("Chase", 100);
        chaseSequence.AddChild(new Leaf("CanSeePlayer", new Condition(() => _detection.DetectingPlayer || _isChasingPlayer)));
        chaseSequence.AddChild(new Leaf("ChasePlayer", new ActionStrategy(() => ChasePlayer())));

        Leaf patrol = new Leaf("Patrol", new RandomPatrolStrategy(transform, _agent, _waypoints), 50);

        PrioritySelector chaseOrPatrol = new PrioritySelector("chaseOrPatrol");
        chaseOrPatrol.AddChild(chaseSequence);
        chaseOrPatrol.AddChild(patrol);
        
        // Added chase sequence to behaviour tree
        _tree.AddChild(chaseOrPatrol);  
    }

    void Update()
    {
        _tree.Process();
    }

    void ChasePlayer()
    {
        if (_detection.DetectingPlayer) // if player is within view cone
        {
            _timeLeftUntilPlayerHidden = _timeUntilPlayerHidden;
        }

        _isChasingPlayer = true;

        _timeLeftText.enabled = true; // displays countdown
        TimeSpan time = TimeSpan.FromSeconds(_timeLeftUntilPlayerHidden);
        _timeLeftText.text = string.Format("{0:D2}:{1:D3}", time.Seconds, time.Milliseconds); // turns it into ss:mmm format

        _agent.SetDestination(_playerTransform.position);

        _timeLeftUntilPlayerHidden -= Time.deltaTime;

        if (_timeLeftUntilPlayerHidden <= 0f)
        {
            _isChasingPlayer = false;
            _timeLeftText.enabled = false; // hides countdown
            _agent.SetDestination(_agent.transform.position);// set path to itself to stop moving
        }
    }
}
