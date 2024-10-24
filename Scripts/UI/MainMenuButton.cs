using Godot;
using System;

public partial class MainMenuButton : Button
{
  public override void _Ready()
  {
    Pressed += OnQuitButtonPressed;
  }

  private void OnQuitButtonPressed()
  {
    // Change scene to the main menu
    GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
  }
}
