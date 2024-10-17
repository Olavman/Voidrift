using Godot;
using System;

public partial class Player : Ship
{
  private double _hitSuccession = 0;
  public bool _isDestroyed = false;

  private AudioPlayer _audioPlayer;
  public Game GameManager;

  [Export] AudioStream _startGameSound;
  [Export] AudioStream _multipleImpactsSound;
  [Export] AudioStream _shieldDepletedSound;
  [Export] AudioStream _hullBreachSound;
  [Export] AudioStream _shieldStailizingSound;
  [Export] AudioStream _targetEliminatedSound;

  public override void _Ready()
  {
    // Get the global audioplayer
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;
    base._Ready();

    // Get the global game manager
    GameManager = GetNode("/root/Game") as Game;

    // Play start game sound
    PlaySound(_audioPlayer.StartGame);
  }
  public override void _PhysicsProcess(double delta)
  {
    if (_isDestroyed) return;
    base._PhysicsProcess(delta);

    // Handle shooting
    if (Input.IsActionPressed("shoot_basic"))
    {
      Shoot();
    }

    // Handling rotation
    if (Input.IsActionPressed("move_right"))
    {
      RotateShip(true, (float)delta);
    }
    if (Input.IsActionPressed("move_left"))
    {
      RotateShip(false, (float)delta);
    }

    // Switch weapon
    if (Input.IsActionJustPressed("scroll_up"))
    {
      SwitchWeapon(SCROLL.UP);
    }
    else if (Input.IsActionJustPressed("scroll_down"))
    {
      SwitchWeapon(SCROLL.DOWN);
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

  public override void TakeDamage(double damage, Vector2 hitFromDirection, Ship damageOwner = null)
  {
    double currentShield = _shield;
    double currentHealth = Health;
    double currentHits = _hitSuccession;
    base.TakeDamage(damage, hitFromDirection);

    var camera = GameManager.Camera as PlayerCam;

    // Apply screen shake based on % health lost
    double percentLost = damage / MaxHealth;
    camera.AddScreenShake((float)percentLost, 0.5f);

    _hitSuccession += 1;
    if (_hitSuccession == 20) // Multiple hits in short succession
    {
      if (_audioPlayer.SoundPlayer.Stream != _audioPlayer.MultipleImpacts)
      {
        PlaySound(_audioPlayer.MultipleImpacts);
      }
    }

    if (_shield <= 0 && currentShield > 0) // Shield got depleted
    {
      // Play depleted shield sound
      PlaySound(_audioPlayer.ShieldDepleted);

      // Apply minor screen shake
      camera.AddScreenShake(5, 0.5f);
    }

    if (Health <= MaxHealth/4 && currentHealth > MaxHealth / 4) // Health got low
    {
      // Play depleted shield sound
      PlaySound(_audioPlayer.HullBreach);

      // Apply larger screen shake
      camera.AddScreenShake(10, 0.5f);
    }
  }

  protected override void StartRechargeShield()
  {
    base.StartRechargeShield();

    // Play sound
    PlaySound(_audioPlayer.ShieldStabilizing);
  }

  protected override void DestroyShip()
  {
    base.DestroyShip();
    QueueFree(); // Remove the player from the scene
  }

  public void KillAquired()
  {
    PlaySound(_targetEliminatedSound);
  }

  private void PlaySound(AudioStream sound)
  {
    _audioPlayer.PlaySound(sound);
    //_audioPlayer?.TriggerSoundEvent(sound);
  }
}
