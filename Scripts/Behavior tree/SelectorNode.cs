using Godot;
using System;
using System.Collections.Generic;

public partial class SelectorNode : BehaviorTreeNode
{
  private readonly List<BehaviorTreeNode> _nodes = new List<BehaviorTreeNode>();

  public void AddNode (BehaviorTreeNode node)
  {
    _nodes.Add(node); 
  }

  public override NodeState Evaluate()
  {
    foreach (var node in _nodes)
    {
      NodeState state = node.Evaluate();
      if (state == NodeState.SUCCESS)
      {
        return NodeState.SUCCESS;
      }
      if (state == NodeState.RUNNING)
      {
        return NodeState.RUNNING;
      }
    }
    return NodeState.FAILURE;
  }
}
