using Godot;
using System;

public partial class Bullet : Node2D
{
	[Export]
	public float Speed = 5000.0f;

	internal Vector2 _velocity = Vector2.Zero;

	public override void _Ready()
  {
    // Play the shooting sound
    GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").Play();
  }

	public void Init(Vector2 direction)
	{
		_velocity += direction.Normalized() * Speed;
		Rotation = direction.Angle();

  }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 previousPosition = Position;
		Position += _velocity * (float)delta;
		
    #region // Elongate the sprite based on movement
    float distanceTraveled = (Position  - previousPosition).Length();
		Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Scale = new Vector2(distanceTraveled / 4.0f, 1); // Adjust for your 8x8 sprite
    #endregion
		
    if (IsOutsideScreen())
		{
			QueueFree();
		}
	}

  private bool IsOutsideScreen()
  {

    Vector2 position = Position;
    Vector2 levelSize = GameSettings.LevelSize;

		if (position.X < 0 || position.X > levelSize.X || position.Y < 0 || position.Y > levelSize.Y) return true;
		return false;
  }
}
