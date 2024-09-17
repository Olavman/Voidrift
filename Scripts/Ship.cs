using Godot;
using System;

public partial class Ship : CharacterBody2D
{
  // Weapon variables
  [Export] public PackedScene BulletScene; // Assign the bullet scene in the editor
  protected float _cooldownTimer = 0; // Timer for bullet cooldown

  [Export] public double MaxHealth = 100.0;
  [Export] public double MaxShield = 100.0;
  [Export] public float RotateSpeed = 4.0f;
  [Export] public float ThrustPower = 10.0f;
  [Export] public float MaxSpeed = 1000.0f;
  public double Health;

  [Signal] public delegate void HealthChangedEventHandler(float newHealth);
  [Signal] public delegate void ShieldChangedEventHandler(float newShield);
  [Signal] public delegate void MaxHealthChangedEventHandler(float newMaxHealth);
  [Signal] public delegate void MaxShieldChangedEventHandler(float newMaxShield);
  [Signal] public delegate void SpeedChangedEventHandler(float newSpeed);

  // Audio variables
  protected AudioStreamPlayer2D _engineAudio; // Audio player for the enigne sound

  // Shield variables
  public int ShieldRechargeSeconds = 6;
  protected double _shield;
  protected float _shieldRechargeTimer;
  protected bool _isRecharging;

  // Movement variables
  protected Vector2 _lastPosition = Vector2.Zero;
  protected Vector2 _velocity = Vector2.Zero;
  protected bool _onMapEdge;
  protected bool _isThrusting;

  public Ship LastHitBy = null;
  protected GpuParticles2D _thrustParticles;

  public override void _Ready()
  {
    // Get the AudioStreamPlayer node for the engine sound
    _engineAudio = GetNode<AudioStreamPlayer2D>("EngineAudio");
    if (_engineAudio == null)
    {
      GD.Print("No engine audio");
    }

    // Pause the sound at start
    _engineAudio.StreamPaused = true;

    _lastPosition = Position;
    _thrustParticles = GetNode < GpuParticles2D>("GPUParticles2D");
    _thrustParticles.Emitting = false; // Start with no emission

    Health = MaxHealth;
    _isThrusting = false;

    EmitSignal(nameof(MaxHealthChanged), MaxHealth);
    EmitSignal(nameof(MaxShieldChanged), MaxShield);
    EmitSignal(nameof(HealthChanged), MaxHealth);
    StartRechargeShield();
  }

  public override void _PhysicsProcess(double delta)
  {
    #region // Movement
    // Shared movement and shield recharging logic
    _isThrusting = false;
    ApplyMovement(delta);
    RechargeShield((float)delta);

    // Handling rotation

    #endregion

    #region // Shooting
    if (_cooldownTimer > 0)
    {
      _cooldownTimer -= (float)delta;
    }
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
    _isThrusting = true;

    // Play engine sound
    if (!_engineAudio.Playing)
    {
      _engineAudio.StreamPaused = false;
    }

    Vector2 thrustingVelocity;
    if (isThrustingForward)
    {
      thrustingVelocity = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * ThrustPower;
    }
    else
    {
      thrustingVelocity = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * -ThrustPower/2;
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
    float rotateSpeed = _isThrusting ? RotateSpeed/2 : RotateSpeed;
    if (isRotatingRight)
    {
      Rotation += rotateSpeed * delta;
    }
    else
    {
      Rotation -= rotateSpeed * delta;
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

    if (!isThrusting )
    {
      _engineAudio.StreamPaused = true;
    }
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

  public void Shoot()
  {
    if (BulletScene == null)
    {
      GD.Print("No bullet");
      return;
    }

    // Check if cooldown is ok
    if (_cooldownTimer <= 0)
    {
      // Create an instance of the bullet
      Bullet bullet = BulletScene.Instantiate() as Bullet;

      // Add the players velocity to the bullet
      bullet._velocity = _velocity;

      // Set the bullet's starting position and direction
      Marker2D spawnPosition = GetNode<Marker2D>("Marker2D");
      bullet.Position = spawnPosition.GlobalPosition;
      bullet.Init(new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation)), this);

      // Add the bullet to the scene
      GetParent().AddChild(bullet);

      _cooldownTimer += bullet.Cooldown;
    }
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

  public virtual void TakeDamage(double damage, Ship damageOwner = null)
  {
    StopShieldRecharging();
    if (damageOwner != null)
    {
      LastHitBy = damageOwner;
    }

    double subtractDamage = 0;
    subtractDamage = Math.Min(damage, _shield); // Reduce shield first
    _shield -= subtractDamage;
    damage -= subtractDamage;
    EmitSignal(nameof(ShieldChanged), _shield);

    if (damage > 0)
    {
      Health -= damage; // Apply remaining damage to health
      EmitSignal(nameof(HealthChanged), Health);
      
      if (Health <= 0) 
      {
        DestroyShip(); // destroy the ship when health reaches zero
      }
    }
  }

  protected virtual void DestroyShip()
  {
    GD.Print("Ship destroyed");
    QueueFree();
  }

  protected void StopShieldRecharging()
  {
    _isRecharging = false;
    _shieldRechargeTimer = 0;
  }

  protected virtual void StartRechargeShield()
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

      if (_shieldRechargeTimer >= ShieldRechargeSeconds)
      {
        StartRechargeShield();
      }
    }
  }
}