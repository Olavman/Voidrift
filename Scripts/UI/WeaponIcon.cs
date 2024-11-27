using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponIcon : HBoxContainer
{
  [Export] public Godot.Collections.Array<Texture2D> IconTextures = new Godot.Collections.Array<Texture2D>();    // Icon textures

  public override void _Ready()
  {
    foreach (var texture in IconTextures)
    {
      if (texture != null)
      {
        AddIcon(texture);
      }
    }
  }

  private void AddIcon(Texture2D icon)
  {
    TextureRect textureRect = new TextureRect();
    textureRect.Texture = icon;
    textureRect.Modulate = new Color(1, 1, 1, 0.5f);

    AddChild(textureRect);
  }

  public void SelectWeapon(int selection)
  {
    int i = 0;
    foreach (var child in GetChildren(false)) 
    {
      if (child is TextureRect)
      {
        var textureRect = (TextureRect)child;
        if (i == selection)
        {
          textureRect.Modulate = new Color(1, 1, 1, 1);
          GD.Print("weapon " + i + " is set to opaque");
        }
        else
        {
          textureRect.Modulate = new Color(1, 1, 1, 0.5f);
          GD.Print("weapon " + i + " is set to semi transparent");
        }
      }
      i++;
    }
  }
}
