using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public interface IStrategy
{
    Node.Status Process();
    void Reset()
    {
        // Noop
    }
}

public class PatrolStrategy : IStrategy
{
    protected readonly Transform entity;
    protected readonly NavMeshAgent agent;
    protected readonly List<Transform> patrolPoints;
    protected readonly float patrolSpeed;
    protected readonly OctaviusBehaviour behaviour;
    protected int currentIndex;
    protected bool isPathCalculated;

    public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f)
    {
        this.entity = entity;
        this.agent = agent;
        this.patrolPoints = patrolPoints;
        this.patrolSpeed = patrolSpeed;
    }

    public virtual Node.Status Process()
    {
        if (currentIndex == patrolPoints.Count) return Node.Status.Success;

        var target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);
        
        if (isPathCalculated && agent.remainingDistance < agent.stoppingDistance)
        {
            currentIndex++;
            isPathCalculated = false;
        }

        if (agent.pathPending)
        {
            isPathCalculated = true;
        }

        return Node.Status.Running;
    }

    public virtual void Reset()
    {
        currentIndex = 0;
    }
}

public class RandomPatrolStrategy : PatrolStrategy
{
    readonly Animator animator;
    private bool _newDestinationSet = false;
    public RandomPatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, Animator animator, float patrolSpeed = 2.0f) : base(entity, agent, patrolPoints, patrolSpeed)
    {
        this.animator = animator;
    }

    public override Node.Status Process()
    {
        if (currentIndex == patrolPoints.Count) return Node.Status.Success;
        
        var target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);


        if (!animator.GetBool("Investigating") && isPathCalculated && agent.remainingDistance < agent.stoppingDistance)
        {
            animator.SetTrigger("Investigate");

            SetRandomPatrolPoint();
            isPathCalculated = false;

            return Node.Status.Running;
        }

        if (agent.pathPending)
        {
            isPathCalculated = true;
        }

        return Node.Status.Running;
    }

    private void SetRandomPatrolPoint()
    {
        int newIndex = currentIndex;
        do
        {
            newIndex = UnityEngine.Random.Range(0, patrolPoints.Count);
        } while (newIndex == currentIndex);

        currentIndex = newIndex;
    }
}

public class TrackStrategy : MonoBehaviour, IStrategy
{
    readonly Transform octaviusTransform;
    readonly float searchRange;
    readonly NavMeshAgent agent;
    public TrackStrategy(Transform octaviusTransform, float searchRange, NavMeshAgent agent)
    {
        this.octaviusTransform = octaviusTransform;
        this.searchRange = searchRange;
        this.agent = agent; 
    }

    public Node.Status Process()
    {
        // This be slow : TODO
        if (FootprintsInRange())
        {
            agent.SetDestination(_cachedFootprints.Min().transform.position);
        }
        else // TODO: null reference when there is no footprints at all
        {
            agent.SetDestination(GetOldestFootprint().transform.position);
        }

        return Node.Status.Running;
    }

    private List<FootprintFade> _cachedFootprints = new List<FootprintFade>();
    public bool FootprintsInRange()
    {
        bool foundAtLeastOne = false;
        _cachedFootprints.Clear();

        // Get all colliders within the radius
        Collider[] collidersInRange = Physics.OverlapSphere(octaviusTransform.position, searchRange, LayerMask.NameToLayer("Footprint"));

        // Loop through colliders and check for the specific component
        foreach (Collider collider in collidersInRange)
        {
            FootprintFade component = collider.GetComponent<FootprintFade>();
            if (component != null)
            {
                foundAtLeastOne = true;
                _cachedFootprints.Add(component);
            }
        }

        return foundAtLeastOne;
    }

    private FootprintFade GetOldestFootprint()
    {
        FootprintFade[] footprints = FindObjectsOfType<FootprintFade>();
        return footprints.Max();
    }
}

public class Condition : IStrategy
{
    readonly Func<bool> predicate;

    public Condition(Func<bool> predicate)
    {
        this.predicate = predicate;
    }

    public Node.Status Process() => predicate()? Node.Status.Success : Node.Status.Failure;
}

public class ActionStrategy : IStrategy
{
    readonly Action doSomething;

    public ActionStrategy(Action doSomething)
    {
        this.doSomething = doSomething;
    }

    public Node.Status Process()
    {
        doSomething();
        return Node.Status.Success;
    }
}

