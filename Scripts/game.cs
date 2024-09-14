using Godot;
using System;

public partial class game : Node
{
  public Line2D _arenaBorder = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    _arenaBorder = GetNode<Line2D>("ArenaBorder");
    SetBorderLines();

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

    _arenaBorder.ClearPoints();

    for (int i = 0; i < numPoints+1; i++)
    {
      float angle = Mathf.DegToRad(i); // Convert degrees to radians
      Vector2 point = new Vector2(
          center.X + Mathf.Cos(angle) * radius,
          center.Y + Mathf.Sin(angle) * radius
        );
      _arenaBorder.AddPoint(point);
    }
  }
}
