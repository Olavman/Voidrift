using Godot;
using System;

public partial class Player : Ship
{
  private double _hitSuccession = 0;

  // Get the global audioplayer
  public AudioPlayer _audioPlayer;

  public override void _Ready()
  {
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;
    base._Ready();

    // Play start game sound
    _audioPlayer.PlaySound(_audioPlayer.StartGame);
  }
  public override void _PhysicsProcess(double delta)
  {
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
    if (Input.IsActionPressed("shoot"))
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

    _hitSuccession += 1;
    if (_hitSuccession == 20) // Multiple hits in short succession
    {
      if (_audioPlayer.SoundPlayer.Stream != _audioPlayer.MultipleImpacts)
      {
        _audioPlayer.PlaySound(_audioPlayer.MultipleImpacts);
      }
    }

    if (_shield <= 0 && currentShield > 0) // Shield got depleted
    {
      // Play depleted shield sound
      _audioPlayer.PlaySound(_audioPlayer.ShieldDepleted);
    }

    if (Health <= MaxHealth/4 && currentHealth > MaxHealth / 4) // Health got low
    {
      // Play depleted shield sound
      _audioPlayer.PlaySound(_audioPlayer.HullBreach);
    }
  }

  protected override void StartRechargeShield()
  {
    base.StartRechargeShield();

    // Play sound
    _audioPlayer.PlaySound(_audioPlayer.ShieldStabilizing);
  }

  protected override void DestroyShip()
  {
    //base.DestroyShip();
    //QueueFree(); // Remove the enemy from the scene
  }
}
