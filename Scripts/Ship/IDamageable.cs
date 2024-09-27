using Godot;
using System;

public partial interface IDamageable
{
  // Property to get and set the health of the object
  public int Health { get; set; }

  // Method to apply damage to the object
  public void Damage(int amount);

  // Method to heal the object
  public void Heal(int amount);

  // Event to notify when the object's health reaches 0
  public event Action OnDeath;

  // Event to notify whenever the object takes damage
  public event Action<int> OnTakeDamage;
}
