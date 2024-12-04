using Godot;
using System;
using System.Collections.Generic;

public partial class Ship : CharacterBody2D
{
  protected enum SCROLL
  {
    UP = -1,
    DOWN = 1
  }

  // Weapon variables
  [Export] public Godot.Collections.Array<PackedScene> WeaponScenes = new Godot.Collections.Array<PackedScene>();    // Weapon scenes
  [Export] public PackedScene ExplosionScene; // Assign the bullet scene in the editor
  protected float _cooldownTimer = 0;         // Timer for bullet cooldown
  protected int _currentWeaponSlot = 0;       // Current weapon ready for use
  protected bool _isShooting;

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
  [Signal] public delegate void CooldownTimerChangedEventHandler(float newCooldown);

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

  public bool IsHit = false;
  public Vector2 HitFromDirection;

  public override void _Ready()
  {
    HitFromDirection = new Vector2();

    // Get the AudioStreamPlayer node for the engine sound
    _engineAudio = GetNode<AudioStreamPlayer2D>("EngineAudio");
    if (_engineAudio == null)
    {
      GD.Print("No engine audio");
    }

    // Check if weapons exist
    if (WeaponScenes.Count == 0)
    {
      GD.Print("No weapons available");
    }

    // Pause the sound at start
    _engineAudio.StreamPaused = true;

    _lastPosition = Position;
    _thrustParticles = GetNode < GpuParticles2D>("GPUParticles2D");
    _thrustParticles.Emitting = false; // Start with no emission

    Health = MaxHealth;
    _shield = MaxShield;
    _isThrusting = false;

    EmitSignal(nameof(MaxHealthChanged), MaxHealth);
    EmitSignal(nameof(MaxShieldChanged), MaxShield);
    EmitSignal(nameof(HealthChanged), MaxHealth);
    //StartRechargeShield();
  }

  public override void _PhysicsProcess(double delta)
  {
    IsHit = false;

    #region // Movement
    // Shared movement and shield recharging logic
    _isThrusting = false;
    _isShooting = false;
    ApplyMovement(delta);
    RechargeShield((float)delta);
    #endregion

    #region // Shooting
    if (_cooldownTimer > 0)
    {
      _cooldownTimer -= (float)delta;
    }
    #endregion

    // Handling rotation


    #region // Damage & Health
    #endregion
  }

  public double GetShield()
  {
    return _shield;
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
    float rotateSpeed = (_isThrusting || _isShooting) ? RotateSpeed/2 : RotateSpeed;
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

  public virtual void Shoot()
  {
    _isShooting = true;

    if (WeaponScenes.Count == 0)
    {
      GD.Print("No weapons");
      return;
    }

    // Check if cooldown is ok
    if (_cooldownTimer <= 0)
    {
      // Create an instance of the bullet
      PackedScene currentWeaponScene = WeaponScenes[_currentWeaponSlot];
      WeaponBase weapon = currentWeaponScene.Instantiate() as WeaponBase;

      // Add the players velocity to the bullet
      Vector2 velocity = new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation)) * Velocity.Length();
      //bullet._velocity = velocity;

      // Set the bullet's starting position and direction
      Marker2D spawnPosition = GetNode<Marker2D>("Marker2D");
      weapon.Position = spawnPosition.GlobalPosition;
      weapon.Init(new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation)), this);

      // Add the bullet to the scene
      GetParent().AddChild(weapon);

      _cooldownTimer += weapon.Cooldown;

      EmitSignal(nameof(CooldownTimerChanged), _cooldownTimer);
    }
  }
  protected virtual void SwitchWeapon(SCROLL scrollDirection)
  {
    _currentWeaponSlot = (_currentWeaponSlot + (int)scrollDirection) % WeaponScenes.Count;
    if (_currentWeaponSlot < 0)
    {
      _currentWeaponSlot += WeaponScenes.Count;
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

  public virtual void TakeDamage(double damage, Vector2 hitFromDirection, Ship damageOwner = null)
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
    GotHit(hitFromDirection);

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
  private void GotHit(Vector2 hitFromDirection)
  {
    IsHit = true;
    HitFromDirection = hitFromDirection;
  }

  protected virtual void DestroyShip()
  {
    GD.Print("Ship destroyed");

    // Apply huge screen shake
    Game gameManager = GetNode("/root/Game") as Game;
    var camera = gameManager.Camera as PlayerCam;
    float maxDist = 1080.0f;
    float multiplier = (maxDist-Position.DistanceTo(camera.Position)) / maxDist;
    camera.AddScreenShake(20* multiplier, 0.5f);

    // Remove from group to avoid counting when checking for win
    RemoveFromGroup("enemy_ships");

    // Check for win condition
    gameManager.CheckIfWinning();

    // Check if ExplosionScene is assigned
    if (ExplosionScene != null)
    {
      // Create an instance of the explosion
      Explosion explosion = ExplosionScene.Instantiate<Explosion>();

      // Set the explosion's position to the ship's current position
      explosion.Position = Position;

      // Set the explosion's size
      explosion.Init(500);

      // Add the explosion to the scene tree (same parent as the ship)
      GetParent().AddChild(explosion);
    }
    else
    {
      GD.Print("No explosion");
    }
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
      // Recharge shield if not full shield
      if (_shield < MaxShield)
      {
        _shield += 0.5;
        EmitSignal(nameof(ShieldChanged), _shield);
      }
      // Stop recharging
      else
      {
        _isRecharging = false;
      }
    }
    else // If not recharging
    {
      _shieldRechargeTimer += 1*delta;

      // Start recharging if timer allows it & shield is not already full
      if (_shieldRechargeTimer >= ShieldRechargeSeconds && _shield < MaxShield)
      {
        StartRechargeShield();
      }
    }
  }
}