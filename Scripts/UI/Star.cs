using Godot;
using System;

public partial class Star : Node2D
{
  private Vector2 _velocity;
  private Vector2 _center;
  private float _radialAccel;
  private float _maxAccel;
  private Sprite2D _sprite;
  private float _startAcceleration;

  public override void _Ready()
  {
    var menu = GetTree().GetFirstNodeInGroup("menus");
    menu.Connect(Menu.SignalName.Transition, new Callable(this, nameof(OnTransitioning)));
  }

  public void OnTransitioning(float transitionProgress)
  {
    _radialAccel = (1 + transitionProgress*20) * _startAcceleration;
    //GD.Print(1 + transitionProgress * 50);
  }

  public void Init(Vector2 startPosition, Vector2 velocity, float radialAccel, Vector2 center, float maxAccel, int secondsPassed)
  {
    _sprite = GetNode<Sprite2D>("StarSprite");
    GlobalPosition = startPosition;
    _velocity = velocity;
    _startAcceleration = radialAccel;
    _radialAccel = radialAccel;
    _center = center;
    _maxAccel = maxAccel;

    // Act as if the star has existed for some time
    for (int i = 0; i < secondsPassed; i++)
    {
      Move(1);
    }

    // Start off as invisible, but will fade in as it moves
    Color spriteColor = _sprite.Modulate;
    spriteColor.A = 0;
    _sprite.Modulate = spriteColor;
  }

  public override void _Process(double delta)
  {
    // Apply radial acceleration (move away from center)
    //Vector2 direction = (GlobalPosition - _center).Normalized();

    // Movement
    Move((float)delta);

    // Optionally: Destroy or recycle stars that go off screen to improve performance
    if (!GetViewportRect().HasPoint(GlobalPosition))
    {
      QueueFree(); // Or reset position for recycling
    }

    // Stretch the star based on velocity
    _sprite.Rotation = _velocity.Angle();
    _sprite.Scale = new Vector2(Math.Max(_velocity.Length() * 0.1f, 1), 1);

    // Change the alpha based on velocity
    Color spriteColor = _sprite.Modulate;
    spriteColor.A = Math.Min(_velocity.Length()*0.1f, 1);

    // Blueshift based on acceleration
    spriteColor.R = Math.Clamp(_radialAccel / _maxAccel, 0, 1);
    spriteColor.G = Math.Clamp(_radialAccel / _maxAccel, 0, 1);
    _sprite.Modulate = spriteColor;

  }

  private void Move(float delta)
  {
    // Gradually increase velocity due to radial acceleration
    _velocity *= (1 + _radialAccel * delta);

    // Move the star
    GlobalPosition += _velocity * delta;
  }

}
