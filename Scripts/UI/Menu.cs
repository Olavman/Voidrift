using Godot;
using System;


public partial class Menu : Control
{
  [Signal] public delegate void TransitionEventHandler(float transitionProgress);

  [ExportCategory("Buttons")]
  [Export] Button PlayBtn = null;
  [Export] Button OptionsBtn = null;
  [Export] Button QuitBtn = null;
  [Export] Button BackBtn = null;

  [ExportCategory("Sounds")]
  [Export] AudioStream SystemsOnline = null;
  [Export] AudioStream ButtonClicked = null;
  [Export] AudioStream ButtonHovered = null;

  [ExportCategory("Sliders")]
  [Export] Slider MasterVolume = null;
  [Export] Slider SFXVolume = null;
  [Export] Slider MusicVolume = null;

  [ExportCategory("General")]
  [Export] PackedScene GameScene = null;
  private AudioPlayer _audioPlayer = null;

  private int _fontBaseSize;
  private int _fontFocusSize;
  private float _transition = 0;
  [Export]private float _transitionTime = 3; // Time to transition to game screen, in seconds
  private bool _isTransitioning = false;
  public override void _Ready()
  {
    GD.Print("Main menu");
    _fontBaseSize = PlayBtn.GetThemeDefaultFontSize();
    _fontFocusSize = _fontBaseSize * 2;

    // Set up button pressed connections
    PlayBtn.Pressed += OnPlayButtonPressed;
    OptionsBtn.Pressed += OnOptionsButtonPressed;
    QuitBtn.Pressed += OnQuitButtonPressed;
    BackBtn.Pressed += OnBackButtonPressed;

    // Set up button hovered connections
    PlayBtn.MouseEntered += OnPlayButtonHovered;
    OptionsBtn.MouseEntered += OnOptionsButtonHovered;
    QuitBtn.MouseEntered += OnQuitButtonHovered;
    BackBtn.MouseEntered += OnBackButtonHovered;

    // Set up slider connections
    MasterVolume.ValueChanged += OnMasterVolumeChanged;
    SFXVolume.ValueChanged += OnSFXVolumeChanged;
    MusicVolume.ValueChanged += OnMusicVolumeChanged;

    // Get the global audioplayer
    _audioPlayer = GetNode("/root/AudioPlayer") as AudioPlayer;

    // Start menu track
    _audioPlayer.PlayMusic(_audioPlayer.MenuTrack);

    // Play start sound
    //PlaySound(SystemsOnline);
  }

  public override void _Process(double delta)
  {
    if (_isTransitioning)
    {
      TransitioToGame();
    }
  }

  private void OnMusicVolumeChanged(double value)
  {
    // Change music volume
    _audioPlayer.SetMusicVolume((float)value);

    // Play button click sound
    PlaySound(ButtonClicked);
  }

  private void OnSFXVolumeChanged(double value)
  {
    // Change SFX volume
    _audioPlayer.SetSFXVolume((float)value);

    // Play button click sound
    PlaySound(ButtonClicked);
  }

  private void OnMasterVolumeChanged(double value)
  {
    // Change master volume
    _audioPlayer.SetMasterVolume((float)value);

    // Play button click sound
    PlaySound(ButtonClicked);
  }

  private void OnPlayButtonPressed()
  {
    // Start game track
    _audioPlayer.PlayMusic(_audioPlayer.GameTrack);

    // Play button click sound
    PlaySound(ButtonClicked);

    // Enter game scene
    //GetTree().ChangeSceneToPacked(GameScene);

    // Start transitioning
    _isTransitioning = true;

    // Start engine sound
    var _engineSound = GetNode<AudioStreamPlayer>("EngineSound");
    _engineSound.Play();
  }

  private void OnOptionsButtonPressed()
  {
    // Play button click sound
    PlaySound(ButtonClicked);
    GD.Print("Enter options");
    var options = GetNode<HBoxContainer>("OptionsBox");
    options.Visible = true;
    var mainMenu = GetNode<MarginContainer>("MainMenuBox");
    mainMenu.Visible = false;
    BackBtn.Visible = true;
  }

  private void OnQuitButtonPressed()
  {
    // Play button click sound
    PlaySound(ButtonClicked);
    GetTree().Quit();
  }
  private void OnBackButtonPressed()
  {
    // Play button click sound
    PlaySound(ButtonClicked);

    var options = GetNode<HBoxContainer>("OptionsBox");
    options.Visible = false;

    var mainMenu = GetNode<MarginContainer>("MainMenuBox");
    mainMenu.Visible = true;

    BackBtn.Visible = false;
  }

  private void OnPlayButtonHovered()
  {
    PlaySound(ButtonHovered);
  }

  private void OnOptionsButtonHovered()
  {
    PlaySound(ButtonHovered);
  }

  private void OnQuitButtonHovered()
  {
    PlaySound(ButtonHovered);
  }
  private void OnBackButtonHovered()
  {
    PlaySound(ButtonHovered);
  }

  private void PlaySound(AudioStream sound)
  {
    _audioPlayer.PlaySound(sound);
    //_audioPlayer?.TriggerSoundEvent(sound);
  }

  private void TransitioToGame()
  {
    _transition += (float)GetProcessDeltaTime();

    EmitSignal(nameof(Transition), _transition/ _transitionTime);
    //GD.Print(_transition / _transitionTime);
    if (_transition >= _transitionTime)
    {
      _isTransitioning = false;
      // Enter game scene
      GetTree().ChangeSceneToPacked(GameScene);
    }
  }
}
