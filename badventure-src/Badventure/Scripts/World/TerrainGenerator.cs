using Godot;
using MapMatrix2d.Generator;

public partial class TerrainGenerator : Node3D
{
	[Export] public int Width = 100;
	[Export] public int Height = 100;
	[Export] public float Frequency = 0.05f;
	[Export] public float Amplitude = 1.0f;
	[Export] public float Persistence = 0.5f;
	[Export] public int Octaves = 4;
	[Export] public int Seed = 12345;
	[Export] public float Power = 0.9f;
	[Export] public float HeightScale = 10.0f;
	[Export] public Texture2D Texture;

	private MeshInstance3D terrainMesh;
	private CollisionShape3D terrainCollision;

	public override void _Ready()
	{
		terrainMesh = GetNode<MeshInstance3D>("Terrain");
		terrainCollision = GetNode<CollisionShape3D>("Terrain/StaticBody3D/TerrainCollision");
		var light = new DirectionalLight3D();
		light.ShadowEnabled = true;
		light.ShadowBias = 0.05F;
		light.ShadowBlur = 1.0F;
		light.LightColor = new Color(1, 1, 1);
		AddChild(light);

		Generate3DNoiseMap();
	}

	private void Generate3DNoiseMap()
	{
		float[,] noiseMatrix = PerlinNoise.GetNoiseMatrix(Width, Height, Frequency, Amplitude, Persistence, Octaves, Seed, Power);

		SurfaceTool surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		for (int x = 0; x < Width; x++)
		{
			for (int z = 0; z < Height; z++)
			{
				float y = noiseMatrix[x, z] * HeightScale;
				surfaceTool.AddVertex(new Vector3(x, y, z));
			}
		}

		for (int x = 0; x < Width - 1; x++)
		{
			for (int z = 0; z < Height - 1; z++)
			{
				int topLeft = x * Height + z;
				int topRight = (x + 1) * Height + z;
				int bottomLeft = x * Height + (z + 1);
				int bottomRight = (x + 1) * Height + (z + 1);

				surfaceTool.AddIndex(topLeft);
				surfaceTool.AddIndex(topRight);
				surfaceTool.AddIndex(bottomLeft);

				surfaceTool.AddIndex(topRight);
				surfaceTool.AddIndex(bottomRight);
				surfaceTool.AddIndex(bottomLeft);
			}
		}

		surfaceTool.GenerateNormals();
		var mesh = surfaceTool.Commit();
		terrainMesh.Mesh = mesh;
		var material = new StandardMaterial3D();
		material.AlbedoTexture = Texture;
		terrainMesh.MaterialOverride = material;

		var collisionShape = new ConcavePolygonShape3D();
		collisionShape.SetFaces(mesh.GetFaces());
		terrainCollision.Shape = collisionShape;

		terrainMesh.Position = new Vector3(-(Width/2), 0, -(Height / 2));
		terrainCollision.Position = new Vector3(terrainCollision.Position.X, 0, terrainCollision.Position.Z);
	}
}
