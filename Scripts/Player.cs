using Godot;
using System;

public partial class Player : Ship
{
  private double _hitSuccession = 0;
  public bool _isDestroyed = false;

  public AudioPlayer Audio;
  public Game GameManager;

  public override void _Ready()
  {
    // Get the global audioplayer
    Audio = GetNode("/root/AudioPlayer") as AudioPlayer;
    base._Ready();

    // Get the global game manager
    GameManager = GetNode("/root/Game") as Game;

    // Play start game sound
    Audio.PlaySound(Audio.StartGame);
  }
  public override void _PhysicsProcess(double delta)
  {
    if (_isDestroyed) return;
    base._PhysicsProcess(delta);

    // Handling rotation
    if (Input.IsActionPressed("move_right"))
    {
      RotateShip(true, (float)delta);
    }
    if (Input.IsActionPressed("move_left"))
    {
      RotateShip(false, (float)delta);
    }

    // Handle shooting
    if (Input.IsActionPressed("shoot_basic"))
    {
      Shoot();
    }

    // Reset the hitsSuccession
    if (_hitSuccession > 0 && _hitSuccession < 20)
    {
      _hitSuccession -= 0.01;
    }
  }

  protected override void AddToVelocity(double delta)
  {
    base.AddToVelocity(delta);

    bool isThrusting = false;

    // Thrust forward or backward
    if (Input.IsActionPressed("move_up"))
    {
      isThrusting = Thrusting(true);
    }
    if (Input.IsActionPressed("move_down"))
    {
      isThrusting = Thrusting(false);
    }

    // Control particle emission
    _thrustParticles.Emitting = isThrusting;
  }

  public override void TakeDamage(double damage, Ship damageOwner = null)
  {
    double currentShield = _shield;
    double currentHealth = Health;
    double currentHits = _hitSuccession;
    base.TakeDamage(damage);

    var camera = GameManager.Camera as PlayerCam;
    // Apply larger screen shake
    camera.AddScreenShake((float)damage/2, 0.5f);

    _hitSuccession += 1;
    if (_hitSuccession == 20) // Multiple hits in short succession
    {
      if (Audio.SoundPlayer.Stream != Audio.MultipleImpacts)
      {
        Audio.PlaySound(Audio.MultipleImpacts);
      }
    }

    if (_shield <= 0 && currentShield > 0) // Shield got depleted
    {
      // Play depleted shield sound
      Audio.PlaySound(Audio.ShieldDepleted);

      // Apply minor screen shake
      camera.AddScreenShake(5, 0.5f);
    }

    if (Health <= MaxHealth/4 && currentHealth > MaxHealth / 4) // Health got low
    {
      // Play depleted shield sound
      Audio.PlaySound(Audio.HullBreach);

      // Apply larger screen shake
      camera.AddScreenShake(10, 0.5f);
    }
  }

  protected override void StartRechargeShield()
  {
    base.StartRechargeShield();

    // Play sound
    Audio.PlaySound(Audio.ShieldStabilizing);
  }

  protected override void DestroyShip()
  {
    base.DestroyShip();
    QueueFree(); // Remove the player from the scene
  }
}
