using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ReloadBar : TextureRect
{
  private ShaderMaterial shaderMaterial;
  [Export] Player player;
  private float _maxValue = 1;

  public override void _Ready()
  {
    var originalShaderMaterial = Material as ShaderMaterial;

    if (originalShaderMaterial != null)
    {
      shaderMaterial = (ShaderMaterial)originalShaderMaterial.Duplicate();
      Material = shaderMaterial;
    }

    // Listen for signals from the player
    player.Connect(Player.SignalName.CooldownTimerChanged, new Callable(this, nameof(OnCooldownTimerChanged)));
  }

  public override void _Process(double delta)
  {
    if (shaderMaterial == null)
    {
      GD.PrintErr("Shader material is missing!");
      return;
    }
    UpdateShader(player.GetCooldownTimer());

  }

  private void OnCooldownTimerChanged(float value)
  {
    _maxValue = value;
  }

  public void UpdateShader(float value)
  {
    value = Mathf.Max(0, value/_maxValue);
    shaderMaterial.SetShaderParameter("progress", value);
  }
}

