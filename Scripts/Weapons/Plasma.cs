using Godot;
using System;

public partial class Plasma : WeaponBase
{
  public override void VisualEffect()
  {
    #region // Elongate the sprite based on movement
    float distanceTraveled = _velocity.Length() * (float)GetPhysicsProcessDeltaTime();
    Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
    sprite.Scale = new Vector2(distanceTraveled / 4.0f, 1); // Adjust for your 8x8 sprite
    #endregion
  }
}
