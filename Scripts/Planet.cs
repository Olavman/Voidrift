using Godot;
using System;

public partial class Planet : CharacterBody2D
{
  [Export] public Node2D BlackHole;
  [Export] public float OrbitRadius = 2000f;  // Distance from the black hole
  [Export] public float OrbitSpeed = 0.5f;   // Speed of orbit (how fast it rotates)

  [Export] private Sprite2D ShadowSprite;
  [Export] private Sprite2D PlanetSprite;
  [Export] private Sprite2D CloudSprite;

  private Node2D _blackHole;
  private float _angle = 0f;  // Angle for calculating position
  private ShaderMaterial _shadowShader;
  //private GradientTexture2D _shadowTextureInstance;

  public override void _Ready()
  {
    // Get the black hole node
    BlackHole = GetNode<Node2D>("/root/Game/BlackHole");
    if (BlackHole == null)
    {
      GD.Print("_blackHole is null");
    }

    // Duplicate the shadow texture so each planet has its own
    //var _shadowTextureInstance = (GradientTexture2D)ShadowSprite.Texture.Duplicate();
    //ShadowSprite.Texture = _shadowTextureInstance;


    // Get the shader material
    var originalMaterial = PlanetSprite.Material as ShaderMaterial;
    if (originalMaterial != null)
    {
      _shadowShader = (ShaderMaterial)originalMaterial.Duplicate();
      PlanetSprite.Material = _shadowShader;
    }

  }

  public void Init(float orbitRadius, float orbitSpeed, float angle)
  {
    OrbitRadius = orbitRadius;
    _angle = angle;
    OrbitSpeed = orbitSpeed;
    GD.Print(OrbitSpeed + " orbitSpeed");
  }

  public override void _Process(double delta)
  {
    if (BlackHole == null) return;

    // Update the angle based on time (OrbitSpeed * delta)
    _angle += OrbitSpeed * (float)delta;

    // Ensure the angle is within 0 to 2 * PI range (for cleaner values)
    _angle = Mathf.Wrap(_angle, 0, Mathf.Tau);

    // Calculate the new position based on the angle and orbit radius
    float x = BlackHole.Position.X + OrbitRadius * Mathf.Cos(_angle);
    float y = BlackHole.Position.Y + OrbitRadius * Mathf.Sin(_angle);

    // Set the planet's new position
    Position = new Vector2(x, y);

    UpdateShadow();
    UpdateClouds();
  }

  private void UpdateClouds()
  {
    // Get the cloud texture
    NoiseTexture2D cloudTexture = CloudSprite.Texture as NoiseTexture2D;
    if (cloudTexture == null)
    {
      GD.Print("cloudTexture is null");
      return;
    }

    // Get the noise texture
    FastNoiseLite noise = cloudTexture.Noise as FastNoiseLite;
    if (noise == null)
    {
      GD.Print("noise is null");
      return;
    }

    // Move the clouds in the x and z direction
    Vector3 offset = noise.Offset;
    offset.Z += 0.1f;
    offset.X += 0.1f;

    // Set the offset to the noise
    noise.Offset = offset;
  }

  private void UpdateShadow()
  {
    if (BlackHole == null)
    {
      GD.Print("BlackHole is null");
      return;
    }
    if (ShadowSprite == null)
    {
      GD.Print("ShadowSprite is null");
      return;
    }

    // Calculate the direction from the black hole to the planet
    Vector2 direction = (GlobalPosition - BlackHole.GlobalPosition).Normalized();

    // Calculate the shadow fill "from" and "to" points based on the direction
    Vector2 fillFrom = new Vector2(0.5f, 0.5f) - (direction * 0.3f); // This adjusts the shadow based on the direction
    Vector2 fillTo = new Vector2(0.5f, 0.5f) + (direction * 0.3f);

    // Get the shadow texture
    //GradientTexture2D shadowTexture = ShadowSprite.Texture as GradientTexture2D;

    _shadowShader.SetShaderParameter("light_position", fillFrom);
    //PlanetSprite.Material = _shadowShader;

    // Set the shadow fill from and to
    //shadowTexture.FillFrom = fillFrom;
    //shadowTexture.FillTo = fillTo;

  }
}
