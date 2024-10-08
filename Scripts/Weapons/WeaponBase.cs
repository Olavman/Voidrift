using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

public abstract partial class WeaponBase : Node2D
{
  protected AudioPlayer _audioPlayer;

  [Export] public float Speed { get; set; }       // Projectile speed
  [Export] public double Damage { get; set; }     // Base damage
  [Export] public float Cooldown { get; set; }    // Cooldown in seconds
  [Export] public float Accuracy { get; set; }    // Accuracy of the weapon (1.0 = perfect accuracy)
  [Export] public double LifeTime { get; set; }   // Lifetime of weapon in seconds
  [Export] public float Range { get; set; }       // Range of weapon
  [Export] public int Piercing { get; set; }      // Times the weapon will pierce
  [Export] public float AOE { get; set; }         // Area of effect
  [Export] public Sprite2D _sprite { get; set; }  // Weapon sprite
  [Export] protected AudioStream _sound;          // Weapon firing sound


  public Ship WeaponOwner;

  internal Vector2 _velocity = Vector2.Zero;
  protected Area2D _collisionArea;
  protected bool _gettingDestroyed;
  protected List <Ship> _targetsHit;

  public override void _Ready()
  {
    // Get the global AudioPlayer singleton
    _audioPlayer = (AudioPlayer)GetNode("/root/AudioPlayer");
    if (_audioPlayer == null)
    {
      GD.Print("Failed to access AudioPlayer singleton");
    }

    // Play weapon sound
    if (_sound != null)
    {
      _audioPlayer.PlaySound(_sound, WeaponOwner.Position);
      //_audioPlayer?.TriggerSoundEvent(_sound, WeaponOwner.Position);
    }
    else
    {
      GD.Print("Sound is null");
    }

    // Change pitch
    //_audioPlayer.PitchScale = (float)GD.RandRange(0.9, 1.1);

    // Get the Area2D node for collision detection
    _collisionArea = GetNode<Area2D>("Area2D");

    // Connect the "body_entered" signal to detect when the weapon hits a ship
    _collisionArea.BodyEntered += OnBodyEntered;

    // Initialize _targetsHit to not get a null value
    _targetsHit = new List <Ship>();


  }
  public virtual void Init(Vector2 direction, Ship weaponOwner)
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
    // Check if the weapon is about to be destroyed, and destroy it
    if (_gettingDestroyed)
    {
      DestroyWeapon();
    }

    // Decrease lifetime of weapon
    if (LifeTime >= 0)
    {
      DecreaseLifetime(delta);
    }

    // Get the previous position and 
    Vector2 previousPosition = Position;

    // Calculate its new position based on the weapons movement behaviour
    Vector2 newPosition = Movement(delta);

    // Use raycast between previous and new position to check for collisions
    CheckCollision(ref previousPosition, newPosition);

    // Move the weapon to its new position
    Position = newPosition;

    // Destroy the weapon if outside the screen
    if (IsOutsideScreen())
    {
      QueueFree();
    }

    // Handle any visual effects
    VisualEffect();
  }
  public virtual void VisualEffect()
  {

  }
  public virtual void DecreaseLifetime(double delta)
  {
    // Decrease lifetime of weapon
    LifeTime -= delta;
    if (LifeTime < 0)
    {
      _gettingDestroyed = true;
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

        // Destroy the weapon if out of piercings
        if (!AllowedToPierce())
        {
          _gettingDestroyed = true;
        }
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
  public virtual Vector2 Movement(double delta)
  {
    Vector2 newPosition = Position + _velocity * (float)delta;

    return newPosition;
  }
  public virtual void CheckCollision(ref Vector2 previousPosition, Vector2 newPosition)
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
  protected virtual void Collided(Ship ship)
  {
    // Apply damage to the ship
    ship.TakeDamage(Damage, WeaponOwner);

    // Add ship to the list to prevent multiple hits
    _targetsHit.Add(ship);
  }
  public virtual void DestroyWeapon()
  {
    // Trigger AoE
    TriggerAOE();

    // Remove the instance and all its children
    QueueFree();
  }
  public virtual void TriggerAOE()
  {
    // Expand the collision shape
    var collisionShape = _collisionArea.GetNode<CollisionShape2D>("CollisionShape2D");
    if (collisionShape.Shape is CircleShape2D circleShape)
    {
      // Duplicate the shape so that only this instance is affected
      var newCircleShape = circleShape.Duplicate() as CircleShape2D;

      // Set the AoE radius
      newCircleShape.Radius += AOE;
    }

    // Detect all bodies in the AoE and apply effects
    var bodiesInAOE = _collisionArea.GetOverlappingBodies();
    
    foreach (var body in bodiesInAOE)
    {
      OnBodyEntered(body);
    }
  }
}
