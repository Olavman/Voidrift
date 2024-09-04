using Godot;
using System;

public partial class Ship : CharacterBody2D
{
  [Export] public PackedScene BulletScene; // Assign the bullet scene in the editor

  [Export] public double MaxHealth = 100.0;
  [Export] public double MaxShield = 100.0;
  [Export] public float RotateSpeed = 2.0f;
  [Export] public float ThrustPower = 10.0f;
  [Export] public float MaxSpeed = 1000.0f;
  public double Health;

  [Signal] public delegate void HealthChangedEventHandler(float newHealth);
  [Signal] public delegate void ShieldChangedEventHandler(float newShield);
  [Signal] public delegate void MaxHealthChangedEventHandler(float newMaxHealth);
  [Signal] public delegate void MaxShieldChangedEventHandler(float newMaxShield);
  [Signal] public delegate void SpeedChangedEventHandler(float newSpeed);

  public int ShieldRechargeSeconds = 3;

  protected Vector2 _lastPosition = Vector2.Zero;
  protected Vector2 _velocity = Vector2.Zero;
  protected GpuParticles2D _thrustParticles;

  protected double _shield;
  protected float _shieldRechargeTimer;

  protected bool _onMapEdge;
  protected bool _isRecharging;

  public override void _Ready()
  {
    _lastPosition = Position;
    _thrustParticles = GetNode < GpuParticles2D>("GPUParticles2D");
    _thrustParticles.Emitting = false; // Start with no emission

    EmitSignal(nameof(MaxHealthChanged), MaxHealth);
    EmitSignal(nameof(MaxShieldChanged), MaxShield);
    Health = MaxHealth;
    EmitSignal(nameof(HealthChanged), MaxHealth);
    StartRechargeShield();
  }

  public override void _PhysicsProcess(double delta)
  {
    #region // Movement
    // Shared movement and shield recharging logic
    ApplyMovement(delta);
    RechargeShield((float)delta);

    // Handling rotation

    #endregion

    #region // Shooting
    // Handle shooting
    #endregion

    #region // Damage & Health
    if (Input.IsActionJustPressed("shoot"))
    {
      //TakeDamage(10);
    }
    #endregion
  }

  protected virtual bool Thrusting(bool isThrustingForward)
  {
    Vector2 thrustingVelocity;
    if (isThrustingForward)
    {
      thrustingVelocity = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * ThrustPower;
    }
    else
    {
      thrustingVelocity = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * -ThrustPower;
    }

    _velocity += thrustingVelocity;

    // If speed is more than max speed, we reduce the forward motion by the same amount we added. This makes it so we can turn but do not add speed when thrusting
    if (_velocity.Length() > MaxSpeed)
    {
      float adjustedVelocity = (_velocity.Length() - thrustingVelocity.Length());
      _velocity = _velocity.Normalized() * adjustedVelocity;
    }
    


    ApplyDrag();

    return true;
  }

  protected virtual void RotateShip(bool isRotatingRight, float delta)
  {
    if (isRotatingRight)
    {
      Rotation += RotateSpeed * delta;
    }
    else
    {
      Rotation -= RotateSpeed * delta;
    }
  }

  protected void GetCurrentVelocity(float delta)
  {
    // Calculate velocity based on position change
    _velocity = (Position - _lastPosition) / delta;
    _lastPosition = Position;
  }

  protected virtual void AddToVelocity(double delta)
  {
    bool isThrusting = false;

    // Apply gravity
    foreach (CelestialObject obj in GetTree().GetNodesInGroup("celestial_objects"))
    {
      _velocity += (obj.ApplyGravity(this, delta));
    }

    // Apply thrusting


    // Control particle emission
    _thrustParticles.Emitting = isThrusting;
  }

  protected virtual void ApplyMovement (double delta)
  {
    GetCurrentVelocity((float)delta);
    AddToVelocity(delta);

    // Apply movement
    Velocity = _velocity;
    EmitSignal(nameof(SpeedChanged), Velocity.Length());


    MoveAndSlide();
    _onMapEdge = false;

    // Check for wrapping around the level using the global LevelSize
    CheckWrapAround();
  }

  protected void Shoot()
  {
    if (BulletScene == null)
    {
      GD.Print("No bullet");
      return;
    }

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

  protected void ApplyDrag()
  {
    if (_onMapEdge)
    {
      _velocity *= 0.98f;
    }
    if (_velocity.Length() < Velocity.Length()) // Ship is trying to turn. Lets help the ship
    {
      _velocity *= 0.98f;
    }
  }

  protected void CheckWrapAround()
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
    subtractDamage = Math.Min(damage, _shield);
    _shield -= subtractDamage;
    damage -= subtractDamage;
    EmitSignal(nameof(ShieldChanged), _shield);

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

  protected virtual void DestroyShip()
  {
    GD.Print("Ship destroyed");
  }

  protected void StopShieldRecharging()
  {
    _isRecharging = false;
    _shieldRechargeTimer = 0;
  }

  protected void StartRechargeShield()
  {
    _isRecharging = true;
  }

  protected void RechargeShield(float delta)
  {
    if (_isRecharging)
    {
      if (_shield < MaxShield)
      {
        _shield += 0.5;
        EmitSignal(nameof(ShieldChanged), _shield);
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