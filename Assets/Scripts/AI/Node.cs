using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BehaviourTree : Node
{
    public BehaviourTree(string name) : base(name) { }

    public override Status Process()
    {
        while (_currentChild < Children.Count)
        {
            Status status = Children[_currentChild].Process();
            if (status != Status.Success)
                return status;
            _currentChild++;
        }
        Reset();
        return Status.Success;
    }
}
public class Leaf : Node
{
    readonly IStrategy strategy;

    public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
    {
        this.strategy = strategy;
    }

    public override Status Process() => strategy.Process();
    public override void Reset() => strategy.Reset();
}
public class Node
{
    public enum Status { Success, Failure, Running }

    public readonly string Name;
    public readonly int Priority;

    public readonly List<Node> Children = new();
    protected int _currentChild;

    public Node(string name = "Node", int priority = 0)
    {
        Name = name;
        Priority = priority;
    }

    public void AddChild(Node child) => Children.Add(child);

    public virtual Status Process() => Children[_currentChild].Process();

    public virtual void Reset()
    {
        _currentChild = 0;
        foreach (Node child in Children)
        {
            child.Reset();
        }
    }
}

public class Sequence : Node
{
    public Sequence(string name, int priority = 0) : base(name, priority) { }

    public override Status Process()
    {
        if (_currentChild < Children.Count)
        {
            switch (Children[_currentChild].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    Reset();
                    return Status.Failure;
                default:
                    _currentChild++;
                    return _currentChild == Children.Count ? Status.Success : Status.Running;

            }
        }

        Reset();
        return Status.Success;
    }
}

public class PrioritySelector : Node
{
    List<Node> sortedChildren;
    List<Node> SortedChildren => sortedChildren ??= SortChildren();

    protected virtual List<Node> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList(); 
    
    public PrioritySelector(string name) : base(name) { }

    public override void Reset()
    {
        base.Reset();
        sortedChildren = null;
    }

    public override Status Process()
    {
        foreach (Node child in SortedChildren)
        {
            switch (child.Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    return Status.Success;
                default:
                    continue;
            }
        }

        return Status.Failure;
    }
}