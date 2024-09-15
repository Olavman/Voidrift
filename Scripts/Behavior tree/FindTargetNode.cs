using Godot;
using System;

public partial class FindTargetNode : BehaviorTreeNode
{
  private Enemy AiShip;

  public FindTargetNode (Enemy enemy)
  {
    AiShip = enemy;
  }

  public override NodeState Evaluate()
  {
    Ship target = AiShip.FindBestTarget();
    if (target != null)
    {
      AiShip.CurrentTarget = target;
      return NodeState.SUCCESS; // Target found
    }
    return NodeState.FAILURE; // No target found
  }
}
