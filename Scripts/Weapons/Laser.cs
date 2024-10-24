using Godot;
using System;

public partial class Laser : WeaponBase
{
  [Export] Sprite2D _laserBeam;
  public override void _Ready()
  {
    base._Ready();
    _velocity = WeaponOwner.Velocity;
  }
  public override void CheckCollision(ref Vector2 previousPosition, Vector2 newPosition)
  {
    Vector2 angle = new Vector2(MathF.Cos(WeaponOwner.Rotation), MathF.Sin(WeaponOwner.Rotation));
    previousPosition = Position;
    newPosition = Position + Range * angle;

    // Create the raycast query parameters
    PhysicsRayQueryParameters2D raycastParams = new PhysicsRayQueryParameters2D
    {
      From = previousPosition,
      To = newPosition,
      Exclude = new Godot.Collections.Array<Rid> { _collisionArea.GetRid() },
      CollisionMask = 1 << 0
    };

    // Perform the raycast
    var spaceState = GetWorld2D().DirectSpaceState;
    var result = spaceState.IntersectRay(raycastParams);


    if (result.Count > 0)
    {
      // If we hit something, check if it's a ship
      var hitObject = result["collider"].As<Node2D>();

      if (hitObject != WeaponOwner)
      {
        // Apply damage to the ship
        Collided(hitObject, previousPosition);

        // Set the new position to the intersection point
        var intersectionPoint = (Vector2)result["position"];

        // Set to destroy the weapon after hitting the ship if not allowed to pierce
        if (!AllowedToPierce())
        {
          _gettingDestroyed = true;
          newPosition = intersectionPoint;// intersection point

          // Set the length of the laser beam to the distance to the collision
          float laserLength = Position.DistanceTo(intersectionPoint);
          _laserBeam.Scale = new Vector2(laserLength, 1);
        }
      }
    }
    else
    {

      _laserBeam.Scale = new Vector2(Range, 1);
    }
  }
  public override void VisualEffect()
  {
  }
}
