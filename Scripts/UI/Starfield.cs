using Godot;
using System;

public partial class Starfield : Node2D
{
  [Export] private PackedScene _starScene; // The Star scene to instantiate
  [Export] private float AccelMultiplier = 3000.0f;
  private Vector2 _center;

  public override void _Ready()
  {
    _center = GetViewportRect().Size / 2;
    for (int i = 0; i < 2400; i++)
    {
      int timePassed = (int)GD.RandRange(1, 20);
      CreateStar(timePassed);
    }
  }

  public override void _Process(double delta)
  {
    SpawnStars(3);
  }

  private void SpawnStars(int count)
  {
    for (int i = 0; i < count; i++)
    {
      CreateStar();
    }
  }

  private void CreateStar(int secondsPassed = 0)
  {
    // Randomize spawn position and velocity
    Vector2 spawnPosition = GetRandomSpawnPosition();
    Vector2 direction = (spawnPosition - _center).Normalized();
    Vector2 velocity = direction * 0.1f;

    // Calculate radial acceleration based on distance from center
    float radialAccel = CalculateRadialAcceleration(spawnPosition);

    // Instantiate the star
    var star = _starScene.Instantiate<Star>();
    AddChild(star);
    star.Init(spawnPosition, velocity, radialAccel, _center, AccelMultiplier, secondsPassed);
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

    float acelleration = Mathf.Pow( normalizedDistance , 2)* AccelMultiplier;

    return acelleration; // Example multiplier for radial acceleration
  }
}
