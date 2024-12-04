using Godot;
using System;
using System.Xml;

public partial class Hud : Control
{
	private HealthBar _healthBar;
	private TextureProgressBar _speedBar;
	private WeaponIcon _weaponIcons;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_healthBar = GetNode<HealthBar>("HUDCanvas/HealthBar");
		_speedBar = GetNode<TextureProgressBar>("HUDCanvas/SpeedBar");
    _weaponIcons = GetNode<WeaponIcon>("HUDCanvas/WeaponIcons");

    if (_healthBar != null )
    { 
      GD.Print("healthbar initialized");
    }
    if (_speedBar != null )
    { 
      GD.Print("speedbar initialized");
    }
    if (_weaponIcons != null )
    { 
      GD.Print("weapon icons initialized");
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

    // Listen for signals from the player
    player.Connect(Player.SignalName.HealthChanged, new Callable(this, nameof(OnHealthChanged)));
    player.Connect(Player.SignalName.ShieldChanged, new Callable(this, nameof(OnShieldChanged)));
    player.Connect(Player.SignalName.MaxHealthChanged, new Callable(this, nameof(OnMaxHealthChanged)));
    player.Connect(Player.SignalName.MaxShieldChanged, new Callable(this, nameof(OnMaxShieldChanged)));
    player.Connect(Player.SignalName.SpeedChanged, new Callable(this, nameof(OnSpeedChanged)));
    player.Connect(Player.SignalName.WeaponSelected, new Callable(this, nameof(OnWeaponSelected)));

    OnHealthChanged(player.MaxHealth);
    OnShieldChanged(player.MaxShield);
    OnWeaponSelected(0);
  }

  private void OnHealthChanged(double newHealth)
  {
    _healthBar.SetHealth(newHealth);
  }
  
  private void OnWeaponSelected(int weaponSelected)
  {
    _weaponIcons.SelectWeapon(weaponSelected);
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
