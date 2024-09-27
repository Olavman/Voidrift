using Godot;
using System;

public partial interface WeaponBase
{
  [Export] public double Damage { get; set; }     // Base damage
  [Export] public float Cooldown { get; set; }    // Cooldown in seconds
  [Export] public float Accuracy { get; set; }    // Accuracy of the weapon (1.0 = perfect accuracy)
  [Export] public double LifeTime { get; set; }   // Lifetime of weapon in seconds
  [Export] public double Range { get; set; }      // Range of weapon
  [Export] public Sprite2D _sprite { get; set; }  // Weapon sprite


  public virtual void Fire()
  {

  }

  public virtual void Movement()
  {

  }

  private Node CheckCollision()
  {
    return null;
  }
}
