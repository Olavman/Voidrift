using Godot;
using System;
using System.Collections.Generic;

public partial class AudioPlayer : Node
{
  public AudioStreamPlayer MusicPlayer;
  public AudioStreamPlayer SoundPlayer;
  [Export] public float FadeDuration = 1.0f; // Time in seconds for smotth fade-ins/outs

  // Music tracks
  [Export] public AudioStream GameTrack;
  [Export] public AudioStream MenuTrack;

  // Menu sounds
  [Export] public AudioStream ButtonHovered;
  [Export] public AudioStream ButtonClicked;

  // Robot voices
  [Export] public AudioStream SystemsOnline;
  [Export] public AudioStream ShieldDepleted;
  [Export] public AudioStream MultipleImpacts;
  [Export] public AudioStream ShieldStabilizing;
  [Export] public AudioStream HullBreach;
  [Export] public AudioStream StealthMode;
  [Export] public AudioStream EngagingForceField;
  [Export] public AudioStream Teleportation;
  [Export] public AudioStream TargetEliminated;
  [Export] public AudioStream EmergencyThrusters;
  [Export] public AudioStream Victory;
  [Export] public AudioStream StartGame;

  [Export] private int MaxAudioPlayers2D = 30;
  [Export] private int MaxAudioPlayers = 4;

  private List<AudioStreamPlayer2D> _audioPlayers2D = new List<AudioStreamPlayer2D>();
  private List<AudioStreamPlayer> _audioPlayers = new List<AudioStreamPlayer>();

  private float _masterVolume = 1.0f;
  private float _sfxVolume = 0.5f;
  private float _musicVolume = 0.5f;

  /*
  public delegate void PlaySoundAtEventHandler(AudioStream sound, Vector2 coordinates);
  public delegate void PlaySoundEventHandler(AudioStream sound);
  public event PlaySoundAtEventHandler PlaySoundAtEvent;
  public event PlaySoundEventHandler PlaySoundEvent;
  */

  public override void _Ready()
  {
    // Get the AudioStreamPlayer node
    MusicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
    SoundPlayer = GetNode<AudioStreamPlayer>("SoundPlayer");

    if (MusicPlayer == null )
    {
      GD.Print("No audioplayer");
    }

    // Subscribe to play sound event
    //PlaySoundEvent += OnPlaySound;
    //GD.Print("Subscribed to PlaySoundEvent");

    // 2D audio streams
    for (int i = 0; i < MaxAudioPlayers2D; i++)
    {
      var player = new AudioStreamPlayer2D();
      //player.Attenuation = 0.0f;
      player.MaxDistance = 5000;
      AddChild(player);
      _audioPlayers2D.Add(player);
    }

    // audio streams without position
    for (int i = 0; i < MaxAudioPlayers; i++)
    {
      var player = new AudioStreamPlayer();
      AddChild(player);
      _audioPlayers.Add(player);
    }
  }

  // Change master volume
  public void SetMasterVolume(float value)
  {
    _masterVolume = value;
    SetSFXVolume(_sfxVolume);
    SetMusicVolume(_musicVolume);
  }

  public void SetMusicVolume(float value)
  {
    _musicVolume = value;
    MusicPlayer.VolumeDb = LinearToDb(_musicVolume * _masterVolume);
  }

  public void SetSFXVolume(float value)
  {
    _sfxVolume = value;
    SoundPlayer.VolumeDb = LinearToDb(_sfxVolume * _masterVolume);
    foreach (var player in _audioPlayers2D)
    {
      player.VolumeDb = SoundPlayer.VolumeDb;
    }
    foreach (var player in _audioPlayers)
    {
      player.VolumeDb = SoundPlayer.VolumeDb;
    }
  }

  private float LinearToDb(float linearValue)
  {
    if (linearValue == 0)
    {
      return -80f; // A very low dV level for silence, as Godot considers -80 dB as silence
    }
    return 20f * MathF.Log10(linearValue);
  }

  // Function to play a specific track
  public void PlayMusic(AudioStream musictrack)
  {
    if (MusicPlayer.Stream != null)
    {
      FadeOutCurrentTrack(() =>
      {
        // Callback after fading out the current track
        MusicPlayer.Stream = musictrack;
        MusicPlayer.Play();
        FadeInCurrentTrack();
      });
    }
    else
    {
      MusicPlayer.Stream = musictrack;
      MusicPlayer.Play();
      FadeInCurrentTrack();
    }
  }
  
  public void PlaySound (AudioStream sound, Vector2 coordinates)
  {
    // Check if the sound's coordinates are within the camera's viewport
    if (sound != null)
    {
      foreach (var player in _audioPlayers2D)
      {
        if (!player.Playing)
        {
          player.Position = coordinates;
          player.Stream = sound;
          player.Play();
          return;
        }
      }

      // If all players are busy, reuse the oldest one (FIFO strategy)
      var oldestPlayer = _audioPlayers2D[0];
      oldestPlayer.Stop(); // Stop the current sound
      oldestPlayer.Position = coordinates;
      oldestPlayer.Stream = sound;
      oldestPlayer.Play();

      // Move the reused player to the end of the list to maintain the FIFO order
      _audioPlayers2D.RemoveAt(0);
      _audioPlayers2D.Add(oldestPlayer);
    }
  }
  public void PlaySound (AudioStream sound)
  {
    foreach (var player in _audioPlayers)
    {
      if (!player.Playing)
      {
        player.Stream = sound;
        player.Play();
        return;
      }
    }

    // If all players are busy, add a new one (optional, based on MaxAudioPlayers limit)
    var newPlayer = new AudioStreamPlayer();
    AddChild(newPlayer);
    _audioPlayers.Add(newPlayer);
    newPlayer.Stream = sound;
    newPlayer.Play();
  }

  // Fade in the current track
  private void FadeInCurrentTrack()
  {
    MusicPlayer.VolumeDb = -80; // Start with a low volume
    var tween = CreateTween();
    tween.TweenProperty(MusicPlayer, "volume_db", LinearToDb(_musicVolume * _masterVolume), FadeDuration); // Fade to normal volume
  }

  // Fade out the current track before stopping or switching
  private void FadeOutCurrentTrack(Action callback)
  {
    var tween = CreateTween();
    tween.TweenProperty(MusicPlayer, "volume_db", -80, FadeDuration).Connect("finished", Callable.From(() => OnFadeOutComplete(callback)));
  }

  // Callback for when fade out is complete
  private void OnFadeOutComplete (Action callback)
  {
    MusicPlayer.Stop();
    callback?.Invoke();
  }

  // Stop the music and fade it out
  public void StopMusic()
  {
    if (MusicPlayer.Playing)
    {
      FadeOutCurrentTrack(() => MusicPlayer.Stop());
    }
  }

  /*
  // Method to trigger the event externally
  public void TriggerSoundEvent(AudioStream sound, Vector2 coordinates)
  {
    if (sound == null)
    {
      GD.Print("Sound is null, event not triggered");
    }
    else
    {
      GD.Print("Play sound at: " + coordinates);
    }
    PlaySoundEvent?.Invoke(sound, coordinates);
  }
  // Method to trigger the event externally
  public void TriggerSoundEvent(AudioStream sound)
  {
    if (sound == null)
    {
      GD.Print("Sound is null, event not triggered");
    }
    else
    {
      GD.Print("Play sound");
    }
    PlaySoundEvent?.Invoke(sound);
  }

  private void OnPlaySound(AudioStream sound, Vector2 coordinates)
  {
    // Check if the sound's coordinates are within the camera's viewport
    if (sound != null && IsWithinViewport(coordinates))
    {
      int i = 0;
      foreach (var player in _audioPlayers2D)
      {
        i++;
        if (!player.Playing)
        {
          player.Position = coordinates;
          player.Stream = sound;
          player.Bus = i.ToString();
          player.Play();
          GD.Print("player " + i + ": sound played at: " + player.Position);
          GD.Print(_audioPlayers.Count);
          return;
        }
      }

      // If all players are busy, reuse the oldest one (FIFO strategy)
      var oldestPlayer = _audioPlayers2D[0];
      oldestPlayer.Stop(); // Stop the current sound
      oldestPlayer.Position = coordinates;
      oldestPlayer.Stream = sound;
      oldestPlayer.Play();

      // Move the reused player to the end of the list to maintain the FIFO order
      _audioPlayers2D.RemoveAt(0);
      _audioPlayers2D.Add(oldestPlayer);

      GD.Print("Reused player to play sound at: " + coordinates);
    }
    else
    {
      GD.Print("position: " + coordinates + ". Sound is outside of viewport, not playing");
    }
  }

  private void OnPlaySound(AudioStream sound)
  {
    foreach (var player in _audioPlayers)
    {
      if (!player.Playing)
      {
        player.Stream = sound;
        player.Play();
        GD.Print("Sound played");
        return;
      }
    }

    // If all players are busy, add a new one (optional, based on MaxAudioPlayers limit)
    var newPlayer = new AudioStreamPlayer();
    AddChild(newPlayer);
    _audioPlayers.Add(newPlayer);
    newPlayer.Stream = sound;
    newPlayer.Play();
  }
  */
}
