using Godot;
using System;

public partial class Star : Node2D
{
  private Vector2 _velocity;
  private Vector2 _center;
  private float _radialAccel;
  private float _maxAccel;
  private Sprite2D _sprite;

  public void Init(Vector2 startPosition, Vector2 velocity, float radialAccel, Vector2 center, float maxAccel)
  {
    _sprite = GetNode<Sprite2D>("StarSprite");
    GlobalPosition = startPosition;
    _velocity = velocity;
    _radialAccel = radialAccel;
    _center = center;
    _maxAccel = maxAccel;

    // Start off as invisible
    Color spriteColor = _sprite.Modulate;
    spriteColor.A = 0;
    _sprite.Modulate = spriteColor;
  }

  public override void _Process(double delta)
  {
    // Apply radial acceleration (move away from center)
    Vector2 direction = (GlobalPosition - _center).Normalized();
    _velocity += direction * _radialAccel * _radialAccel * (float)delta;

    // Move the star
    GlobalPosition += _velocity * (float)delta;

    // Optionally: Destroy or recycle stars that go off screen to improve performance
    if (!GetViewportRect().HasPoint(GlobalPosition))
    {
      QueueFree(); // Or reset position for recycling
    }

    // Stretch the star based on velocity
    _sprite.Rotation = _velocity.Angle();
    _sprite.Scale = new Vector2(_velocity.Length()*0.05f, 1);

    // Change the alpha based on velocity
    Color spriteColor = _sprite.Modulate;
    spriteColor.A = _velocity.Length()*0.1f;
    // Blueshift based on acceleration
    //spriteColor.B = Math.Clamp(_radialAccel / _maxAccel, 0, 1);
    spriteColor.R = Math.Clamp(_radialAccel / _maxAccel, 0, 1);
    spriteColor.G = Math.Clamp(_radialAccel / _maxAccel, 0, 1);
    //GD.Print(_maxAccel);
    _sprite.Modulate = spriteColor;

  }


  /*[Export] public float AccelerationScale = 1.0f;

  private Vector2 _velocity;
  private Vector2 _center;
  private float _acceleration;
  private Viewport _viewport;

  public override void _Ready()
  {
    GD.Print("Spawned");
    // Get the viewport
    _viewport = GetViewport();

    // Set _center to the center of the viewport
    _center = _viewport.GetVisibleRect().Size /2;

    // Set direction of the star to point away from the center
    _velocity = (Position - _center).Normalized();

    // Set the acceleration based on the distance from the center
    CalculateAcceleration();
  }

  public override void _Process(double delta)
  {
    // Increase velocity
    _velocity += new Vector2(_acceleration, _acceleration);

    // Movement
    Position += _velocity;
  }

  private void CalculateAcceleration()
  {
    GD.Print("Calculating acceleration");
    float maxDistance = _center.DistanceTo(_viewport.GetVisibleRect().Size);
    float distanceToCenter = Position.DistanceTo(_center);
    float normalizedDistanceToCenter = 1 - Math.Clamp(distanceToCenter / maxDistance, 0, 1);

    // Set the acceleration based on the distance from the center (closer means higher acceleration)
    _acceleration = normalizedDistanceToCenter * AccelerationScale;
  }*/
}
