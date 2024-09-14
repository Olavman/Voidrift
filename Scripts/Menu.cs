using Godot;
using System;


public partial class Menu : Control
{
  [Export] Button PlayBtn = null;
  [Export] Button OptionsBtn = null;
  [Export] Button QuitBtn = null;
  [Export] PackedScene GameScene = null;

  private AudioPlayer _audioPlayer = null;
  public override void _Ready()
  {
    PlayBtn.Pressed += OnPlayButtonPressed;
    OptionsBtn.Pressed += OnOptionsButtonPressed;
    QuitBtn.Pressed += OnQuitButtonPressed;

    // Get the global audioplayer
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;

    // Start menu track
    _audioPlayer.PlayMusic(_audioPlayer.MenuTrack);
  }

  private void OnPlayButtonPressed()
  {
    // Start game track
    _audioPlayer.PlayMusic(_audioPlayer.GameTrack);

    // Enter game scene
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
