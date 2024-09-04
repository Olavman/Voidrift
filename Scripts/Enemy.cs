using Godot;
using System;

public partial class Enemy : Ship
{
  enum ACTION
  {
    MOVE_FORWARD,
    MOVE_BACKWARD,
    DRIFT
  }

  enum BEHAVIOUR
  {
    EVASIVE,
    AGGRESSIVE,
    ROAMING
  }

  private Ship _currentTarget;
  private ACTION _action;

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    // Find the best target each frame
    _currentTarget = FindBestTarget();

    if ( _currentTarget != null )
    {
      // Calculate the direction to the target
      Vector2 directionToTarget = (Position - _currentTarget.Position).Normalized();
      float desiredRotation = directionToTarget.Angle();

      // Turn towards the target
      float angleDifference = Mathf.Wrap(desiredRotation - Rotation, -Mathf.Pi, Mathf.Pi);
      RotateShip(Mathf.Sign(angleDifference) < 0, (float)delta);

      // Move action
      float distanceToTarget = Position.DistanceTo(_currentTarget.Position);
      if (Mathf.Abs(angleDifference) <= 30)
      {
        if (distanceToTarget > 300)
        {
          _action = ACTION.MOVE_FORWARD;
        }
        else
        {
          _action = ACTION.MOVE_BACKWARD;
        }
      }
      else
      {
        _action = ACTION.MOVE_BACKWARD;
      }
    }

    // Handle shooting
    if (Input.IsActionJustPressed("shoot"))
    {
      Shoot();
    }
  }

  private Ship FindBestTarget()
  {
    Ship bestTarget = null;
    float bestScore = float.MaxValue;

    // Get all potential targets (including the player and other enemies)
    foreach (var target in GetTree().GetNodesInGroup("ships"))
    {
      if (target is Ship ship && ship != this && ship.Health > 0)
      {
        float distance = Position.DistanceTo(ship.Position);
        float angleToTarget = (ship.Position - Position).Angle();
        float angleDifference = Mathf.Abs(Mathf.Wrap(angleToTarget - Rotation, -Mathf.Pi, Mathf.Pi));

        // Score based on distance and angle; lower score is better
        float score = distance + angleDifference * 100; // Adjust weight of angle vs distance as needed

        //GD.Print(bestScore);
        if (score < bestScore)
        {
          bestScore = score;
          bestTarget = ship;
        }
      }
    }
    return bestTarget;
  }

  protected void MoveTowardTarget(Vector2 target, double delta)
  {
    // Calculate the direction to the target
    Vector2 directionToTarget = (Position - _currentTarget.Position).Normalized();
    float desiredRotation = directionToTarget.Angle();

    // Turn towards the target
    float angleDifference = Mathf.Wrap(desiredRotation - Rotation, -Mathf.Pi, Mathf.Pi);
    RotateShip(Mathf.Sign(angleDifference) < 0, (float)delta);

    // Move action
    float distanceToTarget = Position.DistanceTo(target);
    // If the velocity moves the ship further from the target, hit the brakes
  }

  protected override void AddToVelocity(double delta)
  {
    base.AddToVelocity(delta);

    bool isThrusting = false;
    if (_currentTarget != null)
    {
      // Thrust forward or backward
      if (_action == ACTION.MOVE_FORWARD)
      {
        isThrusting = Thrusting(true);
      }
      else if (_action == ACTION.MOVE_BACKWARD)
      {
        isThrusting = Thrusting(false);
      }
      
    }

    // Control particle emission
    _thrustParticles.Emitting = isThrusting;
  }

  protected override void DestroyShip()
  {
    base.DestroyShip();
    QueueFree(); // Remove the enemy from the scene
  }
}
