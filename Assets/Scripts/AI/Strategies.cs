using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    Node.Status Process();
    void Reset();
}

public class PatrolStrategy : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly List<Transform> patrolPoints;
    // TODO
    public Node.Status Process()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}

