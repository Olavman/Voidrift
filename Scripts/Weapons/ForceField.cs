using Godot;
using System;

public partial class ForceField : Node2D
{
  [Export] Ship ship = null;
  [Export] float _hitIntensityTime = 0.3f;

  private float _hitIntensity = 0.0f;
  private float _hitIntensityMax = 5.0f;
  private float _hitIntensityTimer = 0.0f;
  private ShaderMaterial _forceFieldMaterial;

  public override void _Ready()
  {
    // Destrou the forcefield if it's not attached to a ship
    if (ship == null)
    {
      QueueFree();
    }
    else
    {
      GD.Print(ship);
    }

    // Get the shader material
    var originalMaterial = GetNode<TextureRect>("ForceFieldTexture").Material as ShaderMaterial;
    if (originalMaterial != null)
    {
      _forceFieldMaterial = (ShaderMaterial)originalMaterial.Duplicate();
      GetNode<TextureRect>("ForceFieldTexture").Material = _forceFieldMaterial;
    }

  }
  public override void _Process(double delta)
  {
    // Reduce the intensity of the hit over time
    if (_hitIntensityTimer > 0.0f)
    {
      _hitIntensity = _hitIntensityMax * (_hitIntensityTimer / _hitIntensityTime);
      _hitIntensityTimer -= (float)delta;
      UpdateForcefieldShader();
    }

    if (ship.IsHit)
    {
      OnHit(ship.HitFromDirection);
    }

    GlobalRotation = 0;
  }
  private void OnHit(Vector2 hitFrom)
  {
    // Start the timer
    _hitIntensityTimer = _hitIntensityTime;

    // Set the alpha_focus_point to the hitFrom
    Vector2 hitDirection = (hitFrom - GlobalPosition).Normalized();
    Vector2 focusPoint = (hitDirection * 0.5f) + new Vector2(0.5f, 0.5f);
    _forceFieldMaterial.SetShaderParameter("alpha_focus_point", focusPoint);

    // Set the hit intensity
    //_hitIntensity = _hitIntensityMax;
    //_forceFieldMaterial.SetShaderParameter("hit_intensity", _hitIntensity);

    /*
    int currentHitCount = (int)_forceFieldMaterial.GetShaderParameter("active_hit_count");
    if (currentHitCount < 5)
    {
      // Calculate hit direction and focus point
      Vector2 hitDirection = (hitFrom - GlobalPosition).Normalized();
      Vector2 focusPoint = (hitDirection * 0.5f) + new Vector2(0.5f, 0.5f);

      // Set new hit point and intensity in the shader
      _forceFieldMaterial.SetShaderParameter($"alpha_focus_points[{currentHitCount}]", focusPoint);
      _forceFieldMaterial.SetShaderParameter($"hit_intensities[{currentHitCount}]", _hitIntensityMax);

      // Update active hit count
      _forceFieldMaterial.SetShaderParameter("active_hit_count", currentHitCount + 1);
    }*/
  }
  private void UpdateForcefieldShader()
  {
    _forceFieldMaterial.SetShaderParameter("hit_intensity", _hitIntensity);
  }
}
