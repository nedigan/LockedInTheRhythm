using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AI;

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
    private bool reachedDestination = false;
    public RandomPatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, Animator animator, float patrolSpeed = 2.0f) : base(entity, agent, patrolPoints, patrolSpeed)
    {
        this.animator = animator;
    }

    public override Node.Status Process()
    {
        if (currentIndex == patrolPoints.Count) return Node.Status.Success;
        

        var target = patrolPoints[currentIndex];
        agent.SetDestination(target.position);

        if (reachedDestination || (isPathCalculated && agent.remainingDistance < agent.stoppingDistance))
        {
            if (!reachedDestination)
            {
                animator.SetTrigger("Investigate");
                reachedDestination = true;
            }
            else if (!animator.GetBool("Investigating"))
                // If it has started investigating and is no longer investigating
            {
                SetRandomPatrolPoint();
                isPathCalculated = false;
                reachedDestination = false;

                return Node.Status.Running;
            }
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

