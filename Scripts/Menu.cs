using Godot;
using System;


public partial class Menu : Control
{
  [Export] Button PlayBtn = null;
  [Export] Button OptionsBtn = null;
  [Export] Button QuitBtn = null;
  [Export] PackedScene GameScene = null;

  private AudioPlayer _audioPlayer = null;
  private int _fontBaseSize;
  private int _fontFocusSize;
  public override void _Ready()
  {
    GD.Print("Main menu");
    _fontBaseSize = PlayBtn.GetThemeDefaultFontSize();
    _fontFocusSize = _fontBaseSize * 2;

    // Set up button pressed connections
    PlayBtn.Pressed += OnPlayButtonPressed;
    OptionsBtn.Pressed += OnOptionsButtonPressed;
    QuitBtn.Pressed += OnQuitButtonPressed;

    // Set up button hovered connections
    PlayBtn.MouseEntered += OnPlayButtonHovered;
    OptionsBtn.MouseEntered += OnOptionsButtonHovered;
    QuitBtn.MouseEntered += OnQuitButtonHovered;

    // Get the global audioplayer
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;

    // Start menu track
    _audioPlayer.PlayMusic(_audioPlayer.MenuTrack);

    // Play start sound
    _audioPlayer.PlaySound(_audioPlayer.SystemsOnline);
  }

  private void OnPlayButtonPressed()
  {
    // Start game track
    _audioPlayer.PlayMusic(_audioPlayer.GameTrack);

    // Play button click sound
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonClicked);

    // Enter game scene
    GetTree().ChangeSceneToPacked(GameScene);
  }

  private void OnOptionsButtonPressed()
  {
    // Play button click sound
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonClicked);
    GD.Print("Enter options");
  }

  private void OnQuitButtonPressed()
  {
    // Play button click sound
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonClicked);
    GetTree().Quit();
  }

  private void OnPlayButtonHovered()
  {
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonHovered);
  }

  private void OnOptionsButtonHovered()
  {
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonHovered);
  }

  private void OnQuitButtonHovered()
  {
    _audioPlayer.PlayMenuSound(_audioPlayer.ButtonHovered);
  }
}
