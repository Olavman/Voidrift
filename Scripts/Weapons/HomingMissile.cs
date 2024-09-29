using Godot;
using System;

public partial class HomingMissile : WeaponBase
{
  [Export] public float TurnRate = 1.0f;
  [Export] public float Acceleration = 20.0f;

  public override void Init(Vector2 direction, Ship weaponOwner)
  {
    base.Init(direction, weaponOwner);
    _velocity = new Vector2(0, 0);
  }
  public override Vector2 Movement(double delta)
  {
    Vector2 newPosition = Position;

    if (LifeTime > 0) // Only change direction and velocity if Lifetime > 0
    {
      Ship target = FindBestTarget();
      Vector2 angleVelocity = _velocity.Normalized();

      if (target != null)
      {
        // Calculate the direction to the target
        Vector2 directionToTarget = (Position - target.Position).Normalized();
        float desiredRotation = directionToTarget.Angle();

        // Turn towards the target
        float angleDifference = Mathf.Wrap(Rotation - desiredRotation, -Mathf.Pi, Mathf.Pi);
        RotateAngle(Mathf.Sign(angleDifference) > 0, (float)delta);
      }

      // Calculate angular velocity
      angleVelocity = new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
      _velocity = _velocity.Length() * angleVelocity + angleVelocity * Acceleration;

      // Cap velocity to Speed
      if (_velocity.Length() > Speed)
      {
        float adjustedVelocity = (_velocity.Length() - Acceleration);
        _velocity = _velocity.Normalized() * adjustedVelocity;
      }
    }

    // Calculate new position
    newPosition = Position +  _velocity * (float)delta;

    return newPosition;
  }
  protected virtual void RotateAngle(bool isRotatingRight, float delta)
  {
    if (isRotatingRight)
    {
      Rotation += TurnRate * delta;
    }
    else
    {
      Rotation -= TurnRate * delta;
    }
  }
  public override void DecreaseLifetime(double delta)
  {
    // Decrease lifetime to stop thrusters
    LifeTime -= delta;

    if (LifeTime <= 0)
    {
      var engine = GetNode<GpuParticles2D>("GPUParticles2D");
      engine.Emitting = false;
    }
  }
  private Ship FindBestTarget()
  {
    Ship bestTarget = null;
    float bestScore = float.MaxValue;

    // Get all potential targets (including the player and other enemies)
    foreach (var target in GetTree().GetNodesInGroup("ships"))
    {
      if (target is Ship ship && ship != WeaponOwner && ship.Health > 0)
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
}
