using Godot;
using System;

public partial class HealthBar : Panel
{
	private TextureProgressBar _healthBar;
	private TextureProgressBar _shieldBar;

	private int _gotHitTimer = 0;
	private bool _gotHit = false;

  private AudioPlayer _audioPlayer = null;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
	{
		_healthBar = GetNode<TextureProgressBar>("HealthTextureProgress");
		_shieldBar = GetNode<TextureProgressBar>("ShieldTextureProgress");

    // Get the global audioplayer
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;
  }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		PlayBloodEffect();
		PlayShieldEffect();
		if (_gotHit)
		{
			PlayDamageTakenEffect();
		}
  }

	public void SetHealth(double health)
	{
		if (_healthBar.Value > health) // Damage taken
		{
			_gotHit = true;
		}
		_healthBar.Value = health;
	}

	public void SetShield(double shield)
	{
    if (_shieldBar.Value > shield) // Damage taken
    {
      _gotHit = true;
    }
    _shieldBar.Value = shield;
	}

	public void SetMaxHealth(double maxHealth)
	{
		_healthBar.MaxValue = maxHealth;
	}

	public void SetMaxShield(double maxShield)
	{
    _shieldBar.MaxValue = maxShield;
	}

  private void PlayBloodEffect()
	{
    NoiseTexture2D bloodTexture = _healthBar.TextureProgress as NoiseTexture2D;
		FastNoiseLite noise = bloodTexture.Noise as FastNoiseLite;
		Vector3 position = noise.Offset;
		position.Z += 0.1f;

		noise.Offset = position;
	}

	private void PlayShieldEffect()
  {
    NoiseTexture2D shieldTexture = _shieldBar.TextureProgress as NoiseTexture2D;
    FastNoiseLite noise = shieldTexture.Noise as FastNoiseLite;
    Vector3 position = noise.Offset;
    position.Z += 0.1f;

    noise.Offset = position;
  }

	public void PlayDamageTakenEffect()
	{

	}
}
