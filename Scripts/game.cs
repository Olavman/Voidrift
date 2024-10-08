using Godot;
using System;

public partial class Game : Node
{
  public Line2D ArenaBorder = null;
  public PlayerCam Camera = null;
  public Hud Hud = null;
  [Export] public PackedScene EnemyScene = null;
  [Export] public PackedScene PlayerScene = null;
  [Export] public int NumberOfAIShips = 10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    // Set the arena border
    ArenaBorder = GetNode<Line2D>("ArenaBorder");
    SetBorderLines();

    // Get the camera
    Camera = GetNode<PlayerCam>("PlayerCam") as PlayerCam;
    if (Camera == null)
    {
      GD.Print("No Camera");
    }

    // Get the HUD
    Hud = GetNode<Hud>("HUD");
    if (Hud == null)
    {
      GD.Print("No HUD");
    }

    // Spawn player
    SpawnPlayer();

    // Spawn enemies
    for (int i = 0; i < NumberOfAIShips; i++)
    {
      SpawnShip();
    }
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

  // Spawn player
  public void SpawnPlayer ()
  {
    if (PlayerScene == null) return;

    // Instantiate enemy ship
    Player player = PlayerScene.Instantiate() as Player;

    // Get the center of the map
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;

    // Set the position in a random direction and at a random range from the center of the map
    float direction = (float)GD.RandRange(0, Mathf.Pi);
    float distance = (float)levelSize[0]*0.9f;
    player.Position = center + new Vector2(Mathf.Cos(direction), Mathf.Sin(direction))*distance;

    AddChild(player);

    Camera.SetFollow(player);
    Hud.SetOwner(player);
  }
  // Spawn ships
  public void SpawnShip ()
  {
    if (EnemyScene == null) return;

    // Instantiate enemy ship
    Enemy ship = EnemyScene.Instantiate() as Enemy;

    // Get the center of the map
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;

    // Set the position in a random direction and at a random range from the center of the map
    float direction = (float)GD.RandRange(0, Mathf.Pi);
    //float distance = (float)GD.RandRange(1080, levelSize[0]);
    float distance = (float)levelSize[0]*0.9f;
    ship.Position = center + new Vector2(Mathf.Cos(direction), Mathf.Sin(direction))*distance;
    AddChild(ship);
  }
}
