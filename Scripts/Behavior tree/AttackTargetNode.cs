using Godot;
using System;

public partial class AttackTargetNode : BehaviorTreeNode
{
  private Enemy AiShip;

  public AttackTargetNode(Enemy enemy)
  {
    AiShip = enemy;
  }

  public override NodeState Evaluate()
  {
    if (AiShip.CurrentTarget == null)
    {
      return NodeState.FAILURE; // No target to attack
    }

    float distance = AiShip.Position.DistanceTo(AiShip.CurrentTarget.Position);
    Vector2 directionToTarget = (AiShip.CurrentTarget.Position - AiShip.Position).Normalized();
    float desiredRotation = directionToTarget.Angle();

    // Turn towards the target
    float angleDifference = Mathf.Wrap(desiredRotation - AiShip.Rotation, -Mathf.Pi, Mathf.Pi);

    if (distance < 540.0f && Mathf.Abs(angleDifference) < Mathf.Pi / 12)
    {
      AiShip.Shoot(); // attack the target
      return NodeState.SUCCESS;
    }

    return NodeState.FAILURE; // Target is not in range or not facing the target
  }
}
