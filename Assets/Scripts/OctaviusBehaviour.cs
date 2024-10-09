using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

//[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(OctaviusDetection))]
public class OctaviusBehaviour : MonoBehaviour, IHandleGameState
{
    [SerializeField] private List<Transform> _waypoints = new List<Transform>();
    [SerializeField] private TextMeshProUGUI _timeLeftText;
    [SerializeField] private Transform _playerTransform;

    [SerializeField] private Transform _feetTransform;

    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private float _alertSpeedLevel1 = 2f;
    [SerializeField] private float _alertSpeedLevel2 = 3f;
    [SerializeField] private float _alertSpeedLevel3 = 5f; 

    [SerializeField] private float _timeUntilPlayerHidden = 5f;

    private bool _isChasingPlayer = false;

    private bool _newAlert = false;
    private bool _investigatingAlert = false;
    public bool _isTrackingPlayer = false; // TODO : change to private

    private float _timeLeftUntilPlayerHidden;
    private OctaviusDetection _detection;
    private BehaviourTree _tree;
    void Awake()
    {
        _detection = GetComponent<OctaviusDetection>();

        EnemyAlert.NewAlert.AddListener(NewAlertOccurred);

        _tree = new BehaviourTree("Octavius");

        Leaf stopInvestigation = new Leaf("StopInvestigation", new ActionStrategy(() =>
        {
            if (_animator.GetBool("Investigating"))
                _animator.SetTrigger("StopInvestigation");
        }));

        Sequence chaseSequence = new Sequence("Chase", 100);
        chaseSequence.AddChild(new Leaf("CanSeePlayer", new Condition(() => _detection.DetectingPlayer || _isChasingPlayer)));
        chaseSequence.AddChild(stopInvestigation);
        chaseSequence.AddChild(new Leaf("ChasePlayer", new ActionStrategy(() => ChasePlayer())));

        Sequence alertSequence = new Sequence("Alert", 75);
        alertSequence.AddChild(new Leaf("IsAlertPresent", new Condition(() => _newAlert || _investigatingAlert)));
        alertSequence.AddChild(stopInvestigation);
        alertSequence.AddChild(new Leaf("InvestigateAlert", new ActionStrategy(() => GoToAlert())));

        Sequence patrolSequence = new Sequence("PatrolSequence", 50);
        Leaf patrol = new Leaf("Patrol", new RandomPatrolStrategy(transform, _agent, _waypoints, _animator));
        patrolSequence.AddChild(new Leaf("SetAlertLevelLow", new ActionStrategy(() => _agent.speed = _alertSpeedLevel1))); // TODO
        patrolSequence.AddChild(patrol);

        Sequence trackSequence = new Sequence("TrackSequence", 90);
        trackSequence.AddChild(new Leaf("IsTracking", new Condition(() => _isTrackingPlayer || _detection.DetectingFootprint)));
        trackSequence.AddChild(new Leaf("SetAlertLevelModerate", new ActionStrategy(() => _agent.speed = _alertSpeedLevel2)));// TODO
        trackSequence.AddChild(new Leaf("TrackPlayer", new TrackStrategy(_feetTransform,_agent, _detection, _playerTransform, _isTrackingPlayer)));

        PrioritySelector decision = new PrioritySelector("decision");
        // Added chase sequence to behaviour tree
        decision.AddChild(chaseSequence);
        decision.AddChild(patrolSequence);
        decision.AddChild(alertSequence);
        decision.AddChild(trackSequence);
        
        _tree.AddChild(decision);  
    }

    void Update()
    {
        _tree.Process();
        _agent.isStopped = _animator.GetBool("Investigating");
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

    void GoToAlert()
    {
        if (!_investigatingAlert || _newAlert)
        {
            _investigatingAlert = true;
            _agent.SetDestination(_playerTransform.position);
        }

        if (_agent.remainingDistance < _agent.stoppingDistance)
        {
            _investigatingAlert = false;
        }

        _newAlert = false;
    }

    void NewAlertOccurred()
    {
        _newAlert = true;
    }

    public void SetAlertLevel(AlertLevel level)
    {
        switch (level)
        {
            case AlertLevel.Level1:
                _agent.speed = _alertSpeedLevel1;
                break;
            case AlertLevel.Level2:
                _agent.speed = _alertSpeedLevel2;
                break;
            case AlertLevel.Level3:
                _agent.speed = _alertSpeedLevel3;
                break;
        }
    }

    public void ChangeState(GameState state)
    {
        if (state == GameState.EndState)
        {
            _isTrackingPlayer = true;
        }
        else if (state == GameState.MainState)
        {
            _isTrackingPlayer = false;
        }
    }
}

public enum AlertLevel
{
    Level1,
    Level2,
    Level3
}
