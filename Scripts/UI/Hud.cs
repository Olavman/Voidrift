using Godot;
using System;
using System.Xml;

public partial class Hud : Control
{
	private HealthBar _healthBar;
	private TextureProgressBar _speedBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var player = GetNode<Player>("../Player");
    SetOwner(player);

		_healthBar = GetNode<HealthBar>("HUDCanvas/HealthBar");
		_speedBar = GetNode<TextureProgressBar>("HUDCanvas/SpeedBar");

    if (_healthBar != null )
    { 
      GD.Print("healthbar");
    }
    if (_speedBar != null )
    { 
      GD.Print("speedbar");
    }
	}

  public void SetOwner(Player player)
  {
    if (player == null)
    {
      GD.Print("No player");
      return;
    }
    else
    {
      GD.Print($"Player set to {player.Name}");
    }

    player.Connect(Player.SignalName.HealthChanged, new Callable(this, nameof(OnHealthChanged)));
    player.Connect(Player.SignalName.ShieldChanged, new Callable(this, nameof(OnShieldChanged)));
    player.Connect(Player.SignalName.MaxHealthChanged, new Callable(this, nameof(OnMaxHealthChanged)));
    player.Connect(Player.SignalName.MaxShieldChanged, new Callable(this, nameof(OnMaxShieldChanged)));
    player.Connect(Player.SignalName.SpeedChanged, new Callable(this, nameof(OnSpeedChanged)));

    OnHealthChanged(player.MaxHealth);
    OnShieldChanged(player.MaxShield);
  }

  private void OnHealthChanged(double newHealth)
  {
    _healthBar.SetHealth(newHealth);
  }

  private void OnShieldChanged(double newShield)
  {
    _healthBar.SetShield(newShield);
  }
  private void OnMaxHealthChanged(double newMaxHealth)
  {
    _healthBar.SetMaxHealth(newMaxHealth);
  }

  private void OnMaxShieldChanged(double newMaxShield)
  {
    _healthBar.SetMaxShield(newMaxShield);
  }

  private void OnSpeedChanged(double newSpeed)
  {
    _speedBar.Value = newSpeed;
  }
  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
	{
	}
}
