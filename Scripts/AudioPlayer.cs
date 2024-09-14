using Godot;
using System;

public partial class AudioPlayer : Node
{
  private AudioStreamPlayer _audioPlayer;
  [Export] public float FadeDuration = 2.0f; // Time in seconds for smotth fade-ins/outs
  [Export] public AudioStream GameTrack;
  [Export] public AudioStream MenuTrack;

  public override void _Ready()
  {
    // Get the AudioStreamPlayer node
    _audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    if (_audioPlayer == null )
    {
      GD.Print("No audioplayer");
    }

    // Start track
    //PlayMusic(GameTrack);
  }

  // Function to play a specific track
  public void PlayMusic(AudioStream musictrack)
  {
    if (_audioPlayer.Stream != null)
    {
      FadeOutCurrentTrack(() =>
      {
        // Callback after fading out the current track
        _audioPlayer.Stream = musictrack;
        _audioPlayer.Play();
        FadeInCurrentTrack();
      });
    }
    else
    {
      _audioPlayer.Stream = musictrack;
      _audioPlayer.Play();
      FadeInCurrentTrack();
    }
  }

  // Fade in the current track
  private void FadeInCurrentTrack()
  {
    _audioPlayer.VolumeDb = -40; // Start with a low volume
    var tween = CreateTween();
    tween.TweenProperty(_audioPlayer, "volume_db", 0, FadeDuration); // Fade to normal volume
  }

  // Fade out the current track before stopping or switching
  private void FadeOutCurrentTrack(Action callback)
  {
    var tween = CreateTween();
    tween.TweenProperty(_audioPlayer, "volume_db", -40, FadeDuration).Connect("finished", Callable.From(() => OnFadeOutComplete(callback)));
  }

  // Callback for when fade out is complete
  private void OnFadeOutComplete (Action callback)
  {
    _audioPlayer.Stop();
    callback?.Invoke();
  }

  // Stop the music and fade it out
  public void StopMusic()
  {
    if (_audioPlayer.Playing)
    {
      FadeOutCurrentTrack(() => _audioPlayer.Stop());
    }
  }
}
