using Godot;
using System;

public partial class CelestialObject : Node2D
{
	[Export]
	public float Mass = 100000.0f;

	[Export]
	public bool IsStar = false;

  [Export]
  public bool IsBlackHole = false;

	private GpuParticles2D _particles;
	private Sprite2D _sprite;
  private Vector2 _spriteSize;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;
    Position = center;
    // Get references to the Sprite2D and GPUParticles2D nodes
    if (IsBlackHole)
    {
      _particles = GetNode<GpuParticles2D>("GPUParticles2D");
      _sprite = GetNode<Sprite2D>("Sprite2D");
      GD.Print("Black hole");
    }

    if (_sprite == null)
    {
      GD.Print("no sprite");
    }
    _spriteSize = _sprite.Scale / _sprite.Texture.GetSize();


    // Update the shader and particles based on mass
    UpdateShaderAndParticles();
    UpdateScale();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (IsBlackHole)
    {
      RotationDegrees -= 0.1f;
    }

    Mass += 1.0f;
    UpdateScale();
  }

  private void UpdateShaderAndParticles()
  {
    if(!IsBlackHole) { return; }
      
    // Access the shader material
    ShaderMaterial shader = _sprite.Material as ShaderMaterial;
    if (shader != null)
    {
      if (!IsBlackHole)
      {
        shader = null;
      }
    }

    // Modify the particle properties based on Mass
    if (_particles != null)
    {

      // Access the process material of the GPUParticles2D
      if (_particles.ProcessMaterial is ParticleProcessMaterial material)
      {
        //material.EmissionShape = _particles.ProcessMaterial.EmissionShapeEnum.SphereSurface;
        material.EmissionSphereRadius = 128.0f * (_sprite.Scale / _sprite.Texture.GetSize()).Length() * Mass * 0.0005f;
      }
      else
      {
        GD.Print("No valid process material found");
      }
    }

  }

  private void UpdateScale()
  {
    // Set size based on mass
    if (_sprite != null)
    {
      //_sprite.Scale = _sprite.Scale / _sprite.Texture.GetSize() * Mass * 0.005f;
      _sprite.Scale = _spriteSize * Mass * 0.005f;
    }
  }

  public void ApplyGravity(Player ship, double delta)
  {
    Vector2 direction = GlobalPosition - ship.GlobalPosition;
    float distance = direction.Length();

    // Apply gravitational force
    if (distance > 0)
    {
      direction = direction.Normalized();
      float forceMagnitude = (Mass / (distance)); // Simplified gravity equation
      ship.Velocity += direction * forceMagnitude * (float)delta;
    }

    // Check for destruction or damage based on proximity
    if (distance < Mass && IsStar)
    {
      DamageShip(ship, distance, delta);
    }
  }

  private void DamageShip(Player ship, float distance, double delta)
  {
    // Assuming the ship has a health property
    //GD.Print("Ship damaged!");
    //ship.Health -= Mass - distance;
  }

}
