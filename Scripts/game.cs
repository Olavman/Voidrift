using Godot;
using System;

public partial class Game : Node
{
  public Line2D ArenaBorder = null;
  public PlayerCam Camera = null;
  public Hud Hud = null;
  [Export] public PackedScene EnemyScene = null;
  [Export] public PackedScene PlayerScene = null;
  [Export] public PackedScene PlanetScene = null;
  [Export] public PackedScene GameOverScene = null;
  [Export] public int NumberOfAIShips = 10;
  [Export] public int NumberOfPlanets = 7;

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
      SpawnShip(i);
    }

    // Spawn planets
    for (int i = 0; i < NumberOfPlanets; i++)
    {
      SpawnPlanet(i);
    }
	}

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("quit"))
    {
      QuitGame();
    }
    else if (Input.IsActionJustPressed("reset"))
    {
      GetTree().ReloadCurrentScene();
    }
  }

  public void QuitGame()
  {
    GetTree().Quit();
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

  // Spawn planet
  public void SpawnPlanet(int planetNumber)
  {
    if (PlanetScene == null) return;

    // Instantiate planet
    Planet planet = PlanetScene.Instantiate() as Planet;

    // Get the center of the map (position of the Black Hole)
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;
    planet.Position = center;

    // Calculate distance for the planet based on planet number, with some variation
    float distance = ((GameSettings.LevelSize.Length()/2) / (NumberOfPlanets + 1)) * (planetNumber + 1) + GD.RandRange(-300, 300);

    // Calculate orbital speed based on distance, using Kepler's third law
    float orbitSpeed = Mathf.Sqrt((float)(200000000 / Math.Pow(distance, 3)));

    // Initialize the planet with calculated orbit radius, speed, and a random initial angle
    planet.Init(distance, orbitSpeed, (float)GD.RandRange(0, Mathf.Tau));

    // Randomize the planet scale
    float size = 0.5f+ (planetNumber + 1) * (float)GD.RandRange(0.01, 0.06);
    planet.Scale = new Vector2(size, size);

    AddChild(planet);
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
    //float direction = (float)GD.RandRange(0, Mathf.Pi);
    float direction = (Mathf.Pi * 2);
    float distance = (float)levelSize[0]*0.9f;
    player.Position = center + new Vector2(Mathf.Cos(direction), Mathf.Sin(direction))*distance/2;

    AddChild(player);

    Camera.SetFollow(player);
    Hud.SetOwner(player);
  }

  // Spawn ships
  public void SpawnShip (int shipNumber)
  {
    if (EnemyScene == null) return;

    // Increase shipNumber by 1 since it starts at 0
    shipNumber++;

    // Instantiate enemy ship
    Enemy ship = EnemyScene.Instantiate() as Enemy;

    // Get the center of the map
    Vector2 levelSize = GameSettings.LevelSize;
    Vector2 center = levelSize / 2;

    // Set the position in a random direction and at a random range from the center of the map
    //float direction = (float)GD.RandRange(0, Mathf.Pi);
    float direction = ((float)shipNumber / (float)(NumberOfAIShips+1)) * (Mathf.Pi*2);
    //float distance = (float)GD.RandRange(1080, levelSize[0]);
    float distance = (float)levelSize[0]*0.9f;
    ship.Position = center + new Vector2(Mathf.Cos(direction), Mathf.Sin(direction))*distance/2;
    AddChild(ship);
  }

  public void GameOverScreen()
  {
    GD.Print("Creating game over screen");
    var gameOverScreen = GameOverScene.Instantiate();
    var canvasLayer = new CanvasLayer();
    canvasLayer.AddChild(gameOverScreen);
    AddChild(canvasLayer);
    GD.Print("Game over screen created");
  }
}
