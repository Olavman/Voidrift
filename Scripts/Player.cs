using Godot;
using System;

public partial class Player : Ship
{

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    // Handling rotation
    if (Input.IsActionPressed("move_right"))
    {
      RotateShip(true, (float)delta);
    }
    if (Input.IsActionPressed("move_left"))
    {
      RotateShip(false, (float)delta);
    }

    // Handle shooting
    if (Input.IsActionJustPressed("shoot"))
    {
      Shoot();
    }
  }

  protected override void AddToVelocity(double delta)
  {
    base.AddToVelocity(delta);

    bool isThrusting = false;

    // Thrust forward or backward
    if (Input.IsActionPressed("move_up"))
    {
      isThrusting = Thrusting(true);
    }
    if (Input.IsActionPressed("move_down"))
    {
      isThrusting = Thrusting(false);
    }

    // Control particle emission
    _thrustParticles.Emitting = isThrusting;
  }
}
