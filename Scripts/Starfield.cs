using Godot;
using System;

public partial class Starfield : Node2D
{
  [Export] private PackedScene _starScene; // The Star scene to instantiate
  [Export] private float AccelMultiplier = 300.0f;
  private Vector2 _center;

  public override void _Ready()
  {
    _center = GetViewportRect().Size / 2;
  }

  public override void _Process(double delta)
  {
    SpawnStars(1);
  }

  private void SpawnStars(int count)
  {
    for (int i = 0; i < count; i++)
    {
      CreateStar();
    }
  }

  private void CreateStar()
  {
    // Randomize spawn position and velocity
    Vector2 spawnPosition = GetRandomSpawnPosition();
    Vector2 direction = (spawnPosition - _center).Normalized();
    float velocityMagnitude = (float)GD.RandRange(0.0, 1.0); // Random velocity between 0-1
    Vector2 velocity = direction * velocityMagnitude;

    // Calculate radial acceleration based on distance from center
    float radialAccel = CalculateRadialAcceleration(spawnPosition);

    // Instantiate the star
    var star = _starScene.Instantiate<Star>();
    AddChild(star);
    star.Init(spawnPosition, velocity, radialAccel, _center, AccelMultiplier);
  }

  private Vector2 GetRandomSpawnPosition()
  {
    Rect2 rect = GetViewportRect();
    return new Vector2(
        (float)GD.RandRange(rect.Position.X, rect.Position.X + rect.Size.X),
        (float)GD.RandRange(rect.Position.Y, rect.Position.Y + rect.Size.Y)
    );
  }

  private float CalculateRadialAcceleration(Vector2 position)
  {
    float maxDistance = _center.DistanceTo(GetViewportRect().Size);
    float distanceToCenter = position.DistanceTo(_center);
    float normalizedDistance = 1 - Mathf.Clamp(distanceToCenter / maxDistance, 0, 1);

    return normalizedDistance * AccelMultiplier; // Example multiplier for radial acceleration
  }
}
