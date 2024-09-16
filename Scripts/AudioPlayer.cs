using Godot;
using System;

public partial class AudioPlayer : Node
{
  public AudioStreamPlayer MusicPlayer;
  public AudioStreamPlayer SoundPlayer;
  public AudioStreamPlayer MenuPlayer;
  [Export] public float FadeDuration = 2.0f; // Time in seconds for smotth fade-ins/outs

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

  public override void _Ready()
  {
    // Get the AudioStreamPlayer node
    MusicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
    SoundPlayer = GetNode<AudioStreamPlayer>("SoundPlayer");
    MenuPlayer = GetNode<AudioStreamPlayer>("MenuPlayer");

    if (MusicPlayer == null )
    {
      GD.Print("No audioplayer");
    }

    // Start track
    //PlayMusic(GameTrack);
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

  public void PlaySound (AudioStream sound)
  {
    SoundPlayer.Stream = sound;
    SoundPlayer.Play();
  }

  public void PlayMenuSound (AudioStream sound)
  {
    MenuPlayer.Stream = sound;
    MenuPlayer.Play();
  }

  // Fade in the current track
  private void FadeInCurrentTrack()
  {
    MusicPlayer.VolumeDb = -40; // Start with a low volume
    var tween = CreateTween();
    tween.TweenProperty(MusicPlayer, "volume_db", 0, FadeDuration); // Fade to normal volume
  }

  // Fade out the current track before stopping or switching
  private void FadeOutCurrentTrack(Action callback)
  {
    var tween = CreateTween();
    tween.TweenProperty(MusicPlayer, "volume_db", -40, FadeDuration).Connect("finished", Callable.From(() => OnFadeOutComplete(callback)));
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
}
