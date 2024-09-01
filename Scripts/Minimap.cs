using Godot;
using System;

public partial class Minimap : SubViewportContainer
{
	SubViewport _minimapViewport;
	[Export]
	CharacterBody2D _player;
	[Export]
	Node2D _blackHole;
  [Export]
  Camera2D minimapCamera;

  Sprite2D _playerIcon;
  Sprite2D _blackHoleIcon;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _playerIcon = GetNode<Sprite2D>("MinimapViewport/PlayerIcon");
    _blackHoleIcon = GetNode<Sprite2D>("MinimapViewport/BlackHoleIcon");

    if (_playerIcon == null)
    {
      GD.Print("No icon yo");
    }
  }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
  {
    if (minimapCamera != null && _player != null)
    {
      minimapCamera.Position = _player.Position;
      _playerIcon.Position = _player.Position;
      _blackHoleIcon.Position = _blackHole.Position;
    }
  }
}
