using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public abstract partial class WeaponBase : Node2D
{
  [Export] public float Speed { get; set; } // Bullet speed
  [Export] public double Damage { get; set; }     // Base damage
  [Export] public float Cooldown { get; set; }    // Cooldown in seconds
  [Export] public float Accuracy { get; set; }    // Accuracy of the weapon (1.0 = perfect accuracy)
  [Export] public double LifeTime { get; set; }   // Lifetime of weapon in seconds
  [Export] public double Range { get; set; }      // Range of weapon
  [Export] public int Piercing { get; set; }      // Times the weapon will pierce
  [Export] public Sprite2D _sprite { get; set; }  // Weapon sprite

  public Ship WeaponOwner;
  AudioStreamPlayer2D _audioPlayer;

  internal Vector2 _velocity = Vector2.Zero;
  private Area2D _collisionArea;
  private bool _gettingDestroyed;
  private List <Ship> _targetsHit;

  public override void _Ready()
  {
    // Get the audioplayer
    _audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

    // Play the shooting sound
    _audioPlayer.Play();

    // Change pitch
    _audioPlayer.PitchScale = (float)GD.RandRange(0.9, 1.1);

    // Get the Area2D node for collision detection
    _collisionArea = GetNode<Area2D>("Area2D");

    // Connect the "body_entered" signal to detect when the weapon hits a ship
    _collisionArea.BodyEntered += OnBodyEntered;

    // Initialize _targetsHit to not get a null value
    _targetsHit = new List <Ship>();
  }

  public void Init(Vector2 direction, Ship weaponOwner)
  {
    // Introduce randomization in the direction based on accuracy
    float deviation = (1.0f - Accuracy) * 0.4f;
    float randomAngle = (float)GD.RandRange(-deviation, deviation); // Random angle deviation

    // Adjust the direction by the random angle
    direction = direction.Rotated(randomAngle);

    // Set the direction of the weapon
    _velocity += direction.Normalized() * Speed;
    Rotation = direction.Angle();

    // Set the owner of the weapon
    WeaponOwner = weaponOwner;
  }
  public override void _PhysicsProcess(double delta)
  {
    LifeTime -= delta;
    if (LifeTime < 0)
    {
      _gettingDestroyed = true;
    }

    if (_gettingDestroyed)
    {
      QueueFree();
    }

    Vector2 previousPosition = Position;
    Vector2 newPosition = Position + _velocity * (float)delta;
    
    CheckCollision(ref previousPosition, newPosition);

    // Move the weapon to the new position
    Position = newPosition;

    #region // Elongate the sprite based on movement
    float distanceTraveled = (Position - previousPosition).Length();
    Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
    sprite.Scale = new Vector2(distanceTraveled / 4.0f, 1); // Adjust for your 8x8 sprite
    #endregion

    if (IsOutsideScreen())
    {
      QueueFree();
    }
  }

  public virtual bool AllowedToPierce()
  {
    Piercing--;

    if (Piercing < 0)
    {
      return false;
    }
    return true;
  }
  private void OnBodyEntered(Node2D body)
  {
    if (body is Ship ship)
    {
      if (ship != WeaponOwner &&!_targetsHit.Contains(ship))
      { 
        // Apply damage to the ship
        Collided(ship);
      }
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

  public virtual void Fire()
  {

  }

  public virtual void Movement()
  {

  }

  private void CheckCollision(ref Vector2 previousPosition, Vector2 newPosition)
  {
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

      if (hitObject is Ship ship && ship != WeaponOwner)
      {
        // Apply damage to the ship
        Collided(ship);

        // Set the new position to the intersection point
        var intersectionPoint = (Vector2)result["position"];

        // Set to destroy the weapon after hitting the ship if not allowed to pierce
        if (!AllowedToPierce())
        {
          _gettingDestroyed = true;
          newPosition = intersectionPoint;// intersection point
        }
      }
    }
  }

  private void Collided(Ship ship)
  {
    // Apply damage to the ship
    ship.TakeDamage(Damage);

    // Add ship to the list to prevent multiple hits
    _targetsHit.Add(ship);
  }
}