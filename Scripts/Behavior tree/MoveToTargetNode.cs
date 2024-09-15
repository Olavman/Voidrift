using Godot;
using System;

public partial class MoveToTargetNode : BehaviorTreeNode
{
  private Enemy AiShip;

  public MoveToTargetNode(Enemy enemy)
  {
    AiShip = enemy;
  }

  public override NodeState Evaluate()
  {
    if (AiShip.CurrentTarget == null) 
    {
      return NodeState.FAILURE; // No target to move to
    }

    // Use the delta from the game, passed as a parameter
    double delta = AiShip.GetPhysicsProcessDeltaTime(); // Get delta from the engine's timing

    // Move towards the target
    Vector2 target = AiShip.CurrentTarget.Position;
    AiShip.MoveTowardTarget(target, delta);

    // Continue moving until within a certain distance
    float distance = AiShip.Position.DistanceTo(target);
    if (distance < 400.0f)
    {
      return NodeState.SUCCESS; // Reached target
    }

    return NodeState.RUNNING; // Still moving
  }
}
