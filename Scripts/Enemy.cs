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

  private Ship _currentTarget;
  private ACTION _action;

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    // Find the best target each frame
    _currentTarget = FindBestTarget();

    if ( _currentTarget != null )
    {
      /*
      // Calculate the direction to the target
      Vector2 directionToTarget = (Position - _currentTarget.Position).Normalized();
      float desiredRotation = directionToTarget.Angle();

      // Turn towards the target
      float angleDifference = Mathf.Wrap(desiredRotation - Rotation, -Mathf.Pi, Mathf.Pi);
      RotateShip(Mathf.Sign(angleDifference) < 0, (float)delta);

      // Move action
      float distanceToTarget = Position.DistanceTo(_currentTarget.Position);*/
      MoveTowardTarget(_currentTarget.Position, delta);
      /*if (Mathf.Abs(angleDifference) <= 30)
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
      }*/
    }

    // Handle shooting
    if (Position.DistanceTo(_currentTarget.Position) < 600.0f && Position.DirectionTo(_currentTarget.Position).Angle() > Mathf.Pi/4)
    {
      //Shoot();
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
    Vector2 directionToTarget = (Position - target).Normalized();
    float desiredRotation = directionToTarget.Angle();

    // Turn towards the target
    float angleDifference = Mathf.Wrap(Rotation - desiredRotation, -Mathf.Pi, Mathf.Pi);
    //GD.Print(angleDifference);
    RotateShip(Mathf.Sign(angleDifference) > 0, (float)delta);

    // Determine the distance to the target
    float distanceToTarget = Position.DistanceTo(target);

    // Determine if we are moving closer or further from the target based on the current velocity
    Vector2 futurePosition = Position + Velocity * (float)delta;
    float distanceToTargetAfterMoved = futurePosition.DistanceTo(target);

    // If the velocity moves the ship further from the target, hit the brakes
    if (distanceToTargetAfterMoved > distanceToTarget)
    {
      //GD.Print("Slow down");
      SlowDown();
    }
    else if (Mathf.Abs(angleDifference) > Mathf.Pi/2)
    {
      //GD.Print(angleDifference);
      _action = ACTION.MOVE_FORWARD;
    }
    else
    {
      //GD.Print("Drift");
      _action = ACTION.DRIFT;
    }
  }

  private void SpeedUp()
  {
    throw new NotImplementedException();
  }

  private void SlowDown()
  {
    // Get the direction of movement
    float velocityDirection = Velocity.Angle();

    // Calculate the angle difference between the ship's current velocity and facing direction
    float angleVsDirection = Mathf.Abs( Mathf.Wrap(velocityDirection - Rotation, -Mathf.Pi, Mathf.Pi));

    // If facing the velocity direction, apply backwards thrust to slow down
    if (angleVsDirection < Mathf.Pi/2)
    {
      _action = ACTION.MOVE_BACKWARD;
    }
    else
    {
      _action = ACTION.MOVE_FORWARD;
    }
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
