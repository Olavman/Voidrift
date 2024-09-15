using Godot;
using System;
using System.Collections.Generic;

public partial class SequenceNode : BehaviorTreeNode
{
  private readonly List<BehaviorTreeNode> _nodes = new List<BehaviorTreeNode>();

  public void AddNode (BehaviorTreeNode node)
  {
    _nodes.Add(node);
  }

  bool isRunning = false;

  public override NodeState Evaluate()
  {
    foreach (var node in _nodes)
    {
      NodeState state = node.Evaluate();
      if (state == NodeState.FAILURE)
      {
        return NodeState.FAILURE;
      }
      if (state == NodeState.RUNNING)
      {
        isRunning = true; // If any node is running, we return RUNNING
      }
    }
    return isRunning ? NodeState.RUNNING : NodeState.SUCCESS; // All nodes must succeed
  }
}
