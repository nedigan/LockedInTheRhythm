using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;
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
        return Status.Success;
    }
}
public class Leaf : Node
{
    readonly IStrategy strategy;

    public Leaf(string name, IStrategy strategy) : base(name)
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

    public readonly List<Node> Children = new();
    protected int _currentChild;

    public Node(string name = "Node")
    {
        Name = name;
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
