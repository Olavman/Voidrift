using Godot;
using System;
using System.Collections.Generic;

public partial class Minimap : SubViewportContainer
{
	SubViewport _minimapViewport;
	[Export] Camera2D _cam;
	[Export] Node2D _blackHole;
  [Export] Camera2D minimapCamera;
  [Export] private PackedScene enemyRadarIcon;

  private Sprite2D _blackHoleIcon;
  private List<Sprite2D> _enemyIcons = new List<Sprite2D>();

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _blackHoleIcon = GetNode<Sprite2D>("MinimapViewport/BlackHoleIcon");

    if (_blackHoleIcon == null)
    {
      GD.Print("No icon yo");
    }

    SpawnEnemyIcons();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (minimapCamera != null && _cam != null)
    {
      minimapCamera.Position = _cam.Position;
      _blackHoleIcon.Position = _blackHole.Position;

      var enemies = GetTree().GetNodesInGroup("enemy");

      // Ensure that the number of icons matches the number of enemies
      while (_enemyIcons.Count > enemies.Count) 
      {
        // Remove excess icons if there are fewer enemies
        _enemyIcons[_enemyIcons.Count - 1].QueueFree();
        _enemyIcons.RemoveAt(_enemyIcons.Count - 1);
      }

      // Update each enemy icon's position on the minimap
      for (int i = 0; i < enemies.Count; i++)
      {
        if (enemies[i] is Ship enemy)
        {
          // If there is no corresponding icon, instantiate one
          if(i >= _enemyIcons.Count)
          {
            var enemyIconInstance = enemyRadarIcon.Instantiate<Sprite2D>();
            GetNode("MinimapViewport").AddChild(enemyIconInstance);
            _enemyIcons.Add(enemyIconInstance);
          }

          // Update icon's position to match the enemy position
          _enemyIcons[i].Position = enemy.Position;
        }
      }
    }
  }

  private void SpawnEnemyIcons()
  {
    foreach (var enemy in GetTree().GetNodesInGroup("enemy"))
    {
      if (enemy is Ship ship)
      {
        // Instansiate a new icon for each enemy
        var enemyIconInstance = enemyRadarIcon.Instantiate<Sprite2D>();
        //enemyIconInstance.Modulate = new Color(1, 0, 0); // Color it red
        enemyIconInstance.Position = ship.Position;

        // Add the icon to the MinimapViewport
        GetNode("MinimapViewport").AddChild(enemyIconInstance);

        // Add to the list of enemy icons
        _enemyIcons.Add(enemyIconInstance);
      }
    }
  }
}
