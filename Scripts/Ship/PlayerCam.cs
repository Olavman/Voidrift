using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCam : Camera2D
{
  [Export] public Node2D Follow;
  [Export(PropertyHint.Range, "0, 1, 0.01")] public float ScreenshakeMultiplier;
  private ulong _followId;


  // Struct to represent an individual shake
  private struct ScreenShake
  {
    public float Intensity;
    public float Duration;

    public ScreenShake(float intensity, float duration)
    {
      Intensity = intensity;
      Duration = duration;
    }
  }

  List<ScreenShake> _shakes = new List<ScreenShake>();

  public override void _Ready()
  {
    if (Follow != null)
    {
      _followId = Follow.GetInstanceId();
    }
  }

  public override void _Process(double delta)
  {
    if (IsInstanceIdValid(_followId))
    {
      Position = Follow.GetPosition();
    }

    if (_shakes.Count > 0)
    {
      Vector2 shakeOffset = Vector2.Zero;
      float intensity = 0;

      // Iterate over each shake, run the strongest
      for (int i = _shakes.Count - 1; i >= 0; i--)
      {
        var shake = _shakes[i];
        intensity = MathF.Max(intensity, shake.Intensity);
        shake.Duration -= (float)delta; // decrease shake timer

        if (shake.Duration > 10)
        {
          GD.Print("Stop: " + shake.Duration);
          GetTree().Quit();
        }

        if (shake.Duration <= 0)
        {
          _shakes.RemoveAt(i); // Remove finished shakes
        }
        else
        {
          _shakes[i] = shake; // Update the shake's duration
        }

        // Lerp the shake intensity down over time
        shake.Intensity = Mathf.Lerp((float)intensity, 0, (float)delta * (float)shake.Duration);
      }

      // Calculate random shake offset based on intensity
      float shakeAmountX = GD.RandRange(-1, 1) * intensity * ScreenshakeMultiplier;
      float shakeAmountY = GD.RandRange(-1, 1) * intensity * ScreenshakeMultiplier;
      shakeOffset += new Vector2(shakeAmountX, shakeAmountY);


      // Apply the shake effect to the camera position
      Position += shakeOffset;
    }
  }

  public void AddScreenShake(float intensity, float duration)
  {
    // Start a new screen shake
    _shakes.Add(new ScreenShake(intensity, duration));
    //GD.Print("Time: " + shakeTime + " & intensity: " +  intensity);
  }
}
