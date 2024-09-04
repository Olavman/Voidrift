using Godot;
using System;

public abstract partial class BehaviorTreeNode : Node
{
  public enum NodeState
  {
    RUNNING,
    SUCCESS,
    FAILURE
  }

  public abstract NodeState Evaluate();
}
