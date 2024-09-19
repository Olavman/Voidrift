using Godot;
using System;

public partial class Bullet : Node2D
{
	[Export] public float Speed = 5000.0f; // Bullet speed
	[Export] public double Damage = 2.0f; // Bullet damage value
	[Export] public float Cooldown = 0.1f; // Cooldown in seconds
	[Export] public float Accuracy = 0.5f;   // Accuracy of the bullet (1.0 = perfect accuracy)
	[Export] public double LifeTime = 1.0f; // Lifetime of bullet in seconds
	public Ship BulletOwner = null;

	internal Vector2 _velocity = Vector2.Zero;
	private Area2D _collisionArea;

	public override void _Ready()
  {
    // Play the shooting sound
    GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").Play();

		// Get the Area2D node for collision detection
		_collisionArea = GetNode<Area2D>("Area2D");

		// Connect the "body_entered" signal to detect when the bullet hits a ship
		_collisionArea.BodyEntered += OnBodyEntered;
  }

	public void Init(Vector2 direction, Ship bulletOwner)
	{
		// Introduce randomization in the direction based on accuracy
		float deviation = (1.0f - Accuracy) *0.4f; 
		float randomAngle = (float)GD.RandRange(-deviation, deviation); // Random angle deviation

		// Adjust the direction by the random angle
		direction = direction.Rotated(randomAngle);

		// Set the direction of the bullet
    _velocity += direction.Normalized() * Speed;
		Rotation = direction.Angle();

		// Set the owner of the bullet
		BulletOwner = bulletOwner;
  }

	public override void _PhysicsProcess(double delta)
	{
		LifeTime -= delta;
		if (LifeTime < 0)
		{
			QueueFree();
		}

		Vector2 previousPosition = Position;
		Vector2 newPosition = Position + _velocity * (float) delta;

		// Create the raycast query parameters
    PhysicsRayQueryParameters2D raycastParams = new PhysicsRayQueryParameters2D
		{
			From = previousPosition,
			To = newPosition,
			Exclude = new Godot.Collections.Array<Rid> { _collisionArea.GetRid() },
			CollisionMask = 1 << 1
		};

		// Perform the raycast
		var spaceState = GetWorld2D().DirectSpaceState;
		var result = spaceState.IntersectRay(raycastParams);

		if (result.Count > 0)
		{
			// If we hit something, check if it's a ship
			var hitObject = result["collider"].As<Node2D>();
			if (hitObject is Ship ship)
			{
				// Apply damage to the ship
				ship.TakeDamage(Damage);

				// Destroy the bullet after hitting the ship
				QueueFree();
			}
		}

		// If no hit, move the bullet
		Position = newPosition;

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

	private void OnBodyEntered(Node2D body)
	{
		if (body is Ship ship && ship != BulletOwner)
		{
			// Apply damage to the ship
			ship.TakeDamage(Damage, BulletOwner);

			// Destroy the bullet after hitting a ship
			QueueFree();
		}
	}

  private bool IsOutsideScreen()
  {

    Vector2 position = Position;
    Vector2 levelSize = GameSettings.LevelSize;

		if (position.X < 0 || position.X > levelSize.X || position.Y < 0 || position.Y > levelSize.Y) 
			return true;
		return false;
  }
}
