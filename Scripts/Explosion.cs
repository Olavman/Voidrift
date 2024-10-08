using Godot;
using System;

public partial class Explosion : Node2D
{
  private GpuParticles2D _shockwave2;
  private GpuParticles2D _flames;
  private GpuParticles2D _smoke;
  private GpuParticles2D _sparks;
  private Sprite2D _shockwave;
  private ShaderMaterial _shockwaveShader;
  private CanvasGroup _canvasLayer;
  private double _lifetime = 0; // Lifetime in seconds
  private double _lifetimeCounter = 0;
  [Export] public double _shockwaveLifetime = 0.5; // Lifetime of shockwave in seconds
  private double _timer = 0;
  private float _shockwaveSize = 1.0f;
  [Export] private AudioStream _explosionSound = null;
  private AudioPlayer _audioPlayer;

  public override void _Ready()
  {
    _shockwave2 = GetNode<GpuParticles2D>("CanvasLayer/Shockwave2");
    _flames = GetNode<GpuParticles2D>("CanvasLayer/Flames");
    _smoke = GetNode<GpuParticles2D>("Smoke");
    _sparks = GetNode<GpuParticles2D>("CanvasLayer/Sparks");
    _shockwave = GetNode<Sprite2D>("CanvasLayer/Shockwave");

    // Get the CanvasLayer
    _canvasLayer = GetNode<CanvasGroup>("CanvasLayer");

    // Get the ShaderMaterial from the Shockwave sprite
    _shockwaveShader = (ShaderMaterial)_shockwave.Material;

    _lifetime = (_flames.Lifetime + _smoke.Lifetime);

    _shockwave.Scale = new Vector2(0, 0);

    //_shockwave.Emitting = true;
    _flames.Emitting = true;
    _smoke.Emitting = true;
    _sparks.Emitting = true;

    // Get the global AudioPlayer singleton
    _audioPlayer = (AudioPlayer)GetNode("/root/AudioPlayer");
    if (_audioPlayer != null)
    {
      GD.Print("AudioPlayer singleton accessed");
    }
    else
    {
      GD.Print("Failed to access AudioPlayer singleton");
    }

    // Play explosion sound
    //_audioPlayer?.TriggerSoundEvent(_explosionSound, Position);
    _audioPlayer.PlaySound(_explosionSound, Position);
  }

  public void Init(float explosionSize)
  {
    _shockwaveSize = explosionSize;
  }

  public override void _Process(double delta)
  {

    float shockwaveLerp = MathF.Min((float)(_lifetimeCounter / _shockwaveLifetime), 1.0f) ;
    float size = _shockwaveSize * shockwaveLerp;

    // Interpolate scale of shockwave
    if (shockwaveLerp != 1.0f)
    {
      _shockwave.Scale = new Vector2(size, size);
    }
    else
    {
      _shockwave.Scale = new Vector2(0, 0);
    }

    // Make the shockwave less intense over its lifetime
    _shockwaveShader.SetShaderParameter("Strength", Math.Max(1.0 - shockwaveLerp, 0));

    // Make canvaslayer more transparent over time
    var modulate = _canvasLayer.Modulate;
    modulate.A = 1.0f-(float)(_lifetimeCounter / _lifetime); 
    _canvasLayer.Modulate = modulate;

    // Destroy the object
    _lifetimeCounter += delta;
    if (_lifetimeCounter >= _lifetime)
    {
      QueueFree();
    }
  }
}
