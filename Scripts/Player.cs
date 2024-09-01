using Godot;
using System;

public partial class Player : CharacterBody2D
{
  [Export] public PackedScene BulletScene; // Assign the bullet scene in the editor

  [Export] public double Health;
  [Export] public double MaxHealth;
  [Export] public double Shield;
  [Export] public double MaxShield;

  [Signal] public delegate void HealthChangedEventHandler(float newHealth);
  [Signal] public delegate void ShieldChangedEventHandler(float newShield);
  [Signal] public delegate void MaxHealthChangedEventHandler(float newMaxHealth);
  [Signal] public delegate void MaxShieldChangedEventHandler(float newMaxShield);
  [Signal] public delegate void SpeedChangedEventHandler(float newSpeed);

  public float RotateSpeed = 2.0f;
  public float ThrustPower = 5.0f;
  public int ShieldRechargeSeconds = 3;

  private Vector2 _lastPosition = Vector2.Zero;
  private Vector2 _velocity = Vector2.Zero;
  private GpuParticles2D _thrustParticles;

  private float _shieldRechargeTimer;

  private bool _onMapEdge;
  private bool _isRecharging;

  public override void _Ready()
  {
    _lastPosition = Position;
    _thrustParticles = GetNode < GpuParticles2D>("GPUParticles2D");
    _thrustParticles.Emitting = false; // Start with no emission

    EmitSignal(nameof(MaxHealthChanged), MaxHealth);
    EmitSignal(nameof(MaxShieldChanged), MaxShield);
    EmitSignal(nameof(HealthChanged), MaxHealth);
    StartRechargeShield();
  }

  public override void _PhysicsProcess(double delta)
  {
    #region // Movement
    bool isThrusting = false;

    // Handling rotation
    if (Input.IsActionPressed("move_right"))
    {
      Rotation += RotateSpeed * (float)delta;
    }
    if (Input.IsActionPressed("move_left"))
    {
      Rotation -= RotateSpeed * (float)delta;
    }

    // Calculate velocity based on position change
    _velocity = (Position - _lastPosition) / (float)delta;
    _lastPosition = Position;

    // Thrust forward or backward
    if (Input.IsActionPressed("move_up"))
    {
      _velocity += new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * ThrustPower;
      AddDrag();
      isThrusting = true;
    }
    if (Input.IsActionPressed("move_down"))
    {
      _velocity -= new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * ThrustPower;
      AddDrag();
      isThrusting = true;
    }

    // Apply movement
    Velocity = _velocity;
    EmitSignal(nameof(SpeedChanged), Velocity.Length());

    // Get all celestial objects in the scene and apply their gravity
    foreach (CelestialObject obj in GetTree().GetNodesInGroup("celestial_objects"))
    {
      obj.ApplyGravity(this, delta);
    }

    MoveAndSlide();
    _onMapEdge = false;

    // Check for wrapping around the level using the global LevelSize
    CheckWrapAround();

    // Control particle emission
    _thrustParticles.Emitting = isThrusting;
    #endregion

    #region // Shooting
    if (Input.IsActionJustPressed("shoot"))
    {
      Shoot();
    }
    #endregion

    #region // Damage & Health
    if (Input.IsActionJustPressed("shoot"))
    {
      TakeDamage(10);
    }
    RechargeShield((float)delta);
    #endregion
  }

  private void Shoot()
  {
    if (BulletScene == null) return;

    // Create an instance of the bullet
    Bullet bullet = BulletScene.Instantiate() as Bullet;

    // Add the players velocity to the bullet
    bullet._velocity = _velocity;

    // Set the bullet's starting position and direction
    Marker2D spawnPosition = GetNode<Marker2D>("Marker2D");
    bullet.Position = spawnPosition.GlobalPosition;
    bullet.Init(new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation)));


    // Add the bullet to the scene
    GetParent().AddChild(bullet);
  }

  private void AddDrag()
  {
    if (_onMapEdge)
    {
      _velocity *= 0.99f;
    }
    if (_velocity.Length() < Velocity.Length()) // Player is trying to turn. Lets help the player
    {
      _velocity *= 0.99f;
    }
  }

  private void CheckWrapAround()
  {
    Vector2 position = Position;
    Vector2 levelSize = GameSettings.LevelSize;

    // Stop at the edge (circle)
    Vector2 centerLevel = levelSize / 2;
    float radiusLevel = levelSize[0] / 2;
    if ((Position - centerLevel).Length() > radiusLevel)
    {
      Vector2 direction = (Position - centerLevel).Normalized();
      position = centerLevel + direction * radiusLevel;
      _onMapEdge = true;
    }

    /*// Stop at the edge (rectangle)
    if (position.X < 0)
      position.X = 0;
    else if (position.X > levelSize.X)
      position.X = levelSize.X;

    if (position.Y < 0)
      position.Y = 0;
    else if (position.Y > levelSize.Y)
      position.Y = levelSize.Y;
    */

    /*// Warp at the edge
    if (position.X < 0)
      position.X += levelSize.X;
    else if (position.X > levelSize.X)
      position.X -= levelSize.X;

    if (position.Y < 0)
      position.Y += levelSize.Y;
    else if (position.Y > levelSize.Y)
      position.Y -= levelSize.Y;
    */

    Position = position;
  }

  public void TakeDamage(double damage)
  {
    StopShieldRecharging();

    double subtractDamage = 0;
    subtractDamage = Math.Min(damage, Shield);
    Shield -= subtractDamage;
    damage -= subtractDamage;
    EmitSignal(nameof(ShieldChanged), Shield);

    if (damage > 0)
    {
      Health -= damage;
      EmitSignal(nameof(HealthChanged), Health);
      
      if (Health <= 0) 
      {
        DestroyShip();
      }
    }
  }

  private void DestroyShip()
  {
    GD.Print("Ship destroyed");
  }

  private void StopShieldRecharging()
  {
    GD.Print("Stop recharing");
    _isRecharging = false;
    _shieldRechargeTimer = 0;
  }

  public void StartRechargeShield()
  {
    GD.Print("Start recharging");
    _isRecharging = true;
  }

  private void RechargeShield(float delta)
  {
    if (_isRecharging)
    {
      if (Shield < MaxShield)
      {
        Shield += 0.5;
        EmitSignal(nameof(ShieldChanged), Shield);
      }
      else
      {
        _isRecharging = false;
      }
    }
    else
    {
      _shieldRechargeTimer += 1*delta;
      //_shieldRechargeTimer++;
      if (_shieldRechargeTimer >= ShieldRechargeSeconds)
      {
        StartRechargeShield();
      }
    }
  }
}