using Godot;
using System;


public partial class Menu : Control
{
  [Export] Button PlayBtn = null;
  [Export] Button OptionsBtn = null;
  [Export] Button QuitBtn = null;
  [Export] PackedScene GameScene = null;
  public override void _Ready()
  {
    PlayBtn.Pressed += OnPlayButtonPressed;
    OptionsBtn.Pressed += OnOptionsButtonPressed;
    QuitBtn.Pressed += OnQuitButtonPressed;
  }

  private void OnPlayButtonPressed()
  {
    GetTree().ChangeSceneToPacked(GameScene);
  }

  private void OnOptionsButtonPressed()
  {
    GD.Print("Enter options");
  }

  private void OnQuitButtonPressed()
  {
    GetTree().Quit();
  }
}
