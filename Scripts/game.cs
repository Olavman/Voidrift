using Godot;
using System;

public partial class Game : Node
{
  public Line2D ArenaBorder = null;
  public Camera2D Camera = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    // Set the arena border
    ArenaBorder = GetNode<Line2D>("ArenaBorder");
    SetBorderLines();

    // Get the camera
    Camera = GetNode<Camera2D>("PlayerCam") as PlayerCam;

	}

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("quit"))
    {
      GetTree().Quit();
    }
    else if (Input.IsActionJustPressed("reset"))
    {
      GetTree().ReloadCurrentScene();
    }
  }
  private void SetBorderLines()
  {
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;
    float radius = levelSize[0] / 2;
    int numPoints = 360;

    ArenaBorder.ClearPoints();

    for (int i = 0; i < numPoints+1; i++)
    {
      float angle = Mathf.DegToRad(i); // Convert degrees to radians
      Vector2 point = new Vector2(
          center.X + Mathf.Cos(angle) * radius,
          center.Y + Mathf.Sin(angle) * radius
        );
      ArenaBorder.AddPoint(point);
    }
  }
}
