using Godot;
using MapMatrix2d.Generator;
using System;
using System.Collections.Generic;
using System.Text;

public partial class TerrainGenerator : Node3D
{
	// Настройки шума Перлина, доступные из редактора Godot  
	[Export] public int Width = 100;
	[Export] public int Height = 100;
	[Export] public float Frequency = 0.05f;
	[Export] public float Amplitude = 1.0f;
	[Export] public float Persistence = 0.5f;
	[Export] public int Octaves = 4;
	[Export] public int Seed = 12345;
	[Export] public float Power = 0.9f;
	[Export] public float HeightScale = 10.0f; // Масштаб высоты для 3D-карты  

	// Цвет текстуры (DarkGreen)  
	[Export] public Texture2D Texture;

	public override void _Ready()
	{
		Generate3DNoiseMap();
	}

	private void Generate3DNoiseMap()
	{
		// Генерация матрицы шума Перлина  
		float[,] noiseMatrix = PerlinNoise.GetNoiseMatrix(Width, Height, Frequency, Amplitude, Persistence, Octaves, Seed, Power);

		// Создание поверхности для меша  
		SurfaceTool surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		// Генерация вершин и цветов  
		for (int x = 0; x < Width; x++)
		{
			for (int z = 0; z < Height; z++)
			{
				// Высота вершины на основе шума  
				float y = noiseMatrix[x, z] * HeightScale;

				surfaceTool.AddVertex(new Vector3(x, y, z));
			}
		}

		// Генерация индексов для треугольников  
		for (int x = 0; x < Width - 1; x++)
		{
			for (int z = 0; z < Height - 1; z++)
			{
				int topLeft = x * Height + z;
				int topRight = (x + 1) * Height + z;
				int bottomLeft = x * Height + (z + 1);
				int bottomRight = (x + 1) * Height + (z + 1);

				// Первый треугольник (верхний левый, верхний правый, нижний левый)  
				surfaceTool.AddIndex(topLeft);
				surfaceTool.AddIndex(topRight);
				surfaceTool.AddIndex(bottomLeft);

				// Второй треугольник (верхний правый, нижний правый, нижний левый)  
				surfaceTool.AddIndex(topRight);
				surfaceTool.AddIndex(bottomRight);
				surfaceTool.AddIndex(bottomLeft);
			}
		}

		// Создание меша  
		var mesh = surfaceTool.Commit();
		surfaceTool.GenerateNormals();

		// Создание экземпляра меша  
		var meshInstance = new MeshInstance3D();
		var material = new StandardMaterial3D();
		material.AlbedoTexture = Texture;
		meshInstance.MaterialOverride = material;
		meshInstance.Mesh = mesh;

		// Добавление коллизии  
		var collisionShape = new CollisionShape3D();
		var concaveShape = new ConcavePolygonShape3D();
		concaveShape.SetFaces(mesh.GetFaces()); // Получаем массив вершин для коллизии  
		collisionShape.Shape = concaveShape;

		// Добавляем меш и коллизию в сцену  
		AddChild(meshInstance);
		AddChild(collisionShape);
	}

	/*
	// Параметры, доступные из редактора Godot  
	[Export] public int MapSize { get; set; } = 1000;
	[Export] public float MaxHeight { get; set; } = 10.0f;
	[Export] public float WaterLevel { get; set; } = 3.0f;
	[Export] public float ValleyLevel { get; set; } = 5.0f;
	[Export] public float MountainsLevel { get; set; } = 8.0f;

	// Текстуры для разных типов поверхности  
	[Export] public Texture2D WaterTexture { get; set; }
	[Export] public Texture2D GrassTexture { get; set; }
	[Export] public Texture2D MountainTexture { get; set; }

	private float[,] heightMap;

	public override void _Ready()
	{
		ValidateParameters();
		GenerateWorld();
	}

	private void ValidateParameters()
	{
		if (MapSize <= 0 || MaxHeight <= 0 || WaterLevel < 0 || ValleyLevel < 0 || MountainsLevel < 0)
		{
			GD.Print("Invalid parameters. Please check the values.");
			return;
		}
	}

	private void GenerateWorld()
	{
		heightMap = new float[MapSize, MapSize];
		GenerateHeights();
		ApplyIslandShape();
		AddLakes(5); // 5 озер  
		SmoothTerrain();
		FixHoles();
		MultiSmoothTerrain();
		ApplyLandscape();
	}
	[Export] public float NoiseScale { get; set; } = 10.0f;
	[Export] public int Octaves { get; set; } = 4;
	[Export] public float Persistence { get; set; } = 0.5f;
	[Export] public int SmoothIterations { get; set; } = 5;

	private void GenerateHeights()
	{
		var noise = new Perlin2D();

		for (int x = 0; x < MapSize; x++)
		{
			for (int y = 0; y < MapSize; y++)
			{
				float nx = (float)x / MapSize - 0.5f;
				float ny = (float)y / MapSize - 0.5f;
				float elevation = noise.Noise(nx * NoiseScale, ny * NoiseScale, Octaves, Persistence) * MaxHeight;
				heightMap[x, y] = elevation;
			}
		}
	}

	private float GetNearestHeight(int x, int y)
	{
		for (int radius = 1; radius < MapSize; radius++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dy = -radius; dy <= radius; dy++)
				{
					int nx = x + dx;
					int ny = y + dy;
					if (nx >= 0 && nx < MapSize && ny >= 0 && ny < MapSize && heightMap[nx, ny] != 0.0f)
					{
						return heightMap[nx, ny];
					}
				}
			}
		}
		return 0.0f;
	}

	private void MultiSmoothTerrain()
	{
		for (int i = 0; i < SmoothIterations; i++)
		{
			SmoothTerrain();
		}
	}

	private void ApplyIslandShape()
	{
		float center = MapSize / 2.0f;
		float maxDistance = center;

		for (int x = 0; x < MapSize; x++)
		{
			for (int y = 0; y < MapSize; y++)
			{
				float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
				float factor = 1.0f - Mathf.Pow(distance / maxDistance, 2);
				heightMap[x, y] *= factor;
			}
		}
	}

	private void AddLakes(int numberOfLakes)
	{
		var rand = new Random();

		for (int i = 0; i < numberOfLakes; i++)
		{
			int lakeCenterX = rand.Next(MapSize);
			int lakeCenterY = rand.Next(MapSize);

			float lakeRadius = rand.Next(5, 15);
			float lakeDepth = WaterLevel - rand.Next(5, 15)/10.0F;

			for (int x = (int)(lakeCenterX - lakeRadius); x <= lakeCenterX + lakeRadius; x++)
			{
				for (int y = (int)(lakeCenterY - lakeRadius); y <= lakeCenterY + lakeRadius; y++)
				{
					if (x >= 0 && x < MapSize && y >= 0 && y < MapSize)
					{
						float distance = Mathf.Sqrt((x - lakeCenterX) * (x - lakeCenterX) + (y - lakeCenterY) * (y - lakeCenterY));
						if (distance <= lakeRadius)
						{
							float factor = 1.0f - (distance / lakeRadius);
							heightMap[x, y] = Mathf.Lerp(heightMap[x, y], lakeDepth, factor);
						}
					}
				}
			}
		}
	}

	private void SmoothTerrain()
	{
		for (int x = 1; x < MapSize - 1; x++)
		{
			for (int y = 1; y < MapSize - 1; y++)
			{
				heightMap[x, y] = (
					heightMap[x - 1, y] +
					heightMap[x + 1, y] +
					heightMap[x, y - 1] +
					heightMap[x, y + 1]
				) / 4;
			}
		}
	}

	private void FixHoles()
	{
		for (int x = 0; x < MapSize; x++)
		{
			for (int y = 0; y < MapSize; y++)
			{
				if (heightMap[x, y] == 0.0f)
				{
					heightMap[x, y] = GetNearestHeight(x, y);
				}
			}
		}
	}

	private MeshInstance3D CreateTerrainMesh()
	{
		var surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		for (int x = 0; x < MapSize - 1; x++)
		{
			for (int y = 0; y < MapSize - 1; y++)
			{
				// Вершины для квадрата  
				Vector3 v1 = new Vector3(x, heightMap[x, y], y);
				Vector3 v2 = new Vector3(x + 1, heightMap[x + 1, y], y);
				Vector3 v3 = new Vector3(x, heightMap[x, y + 1], y + 1);
				Vector3 v4 = new Vector3(x + 1, heightMap[x + 1, y + 1], y + 1);

				// Первый треугольник  
				surfaceTool.AddVertex(v1);
				surfaceTool.AddVertex(v2);
				surfaceTool.AddVertex(v3);

				// Второй треугольник  
				surfaceTool.AddVertex(v2);
				surfaceTool.AddVertex(v4);
				surfaceTool.AddVertex(v3);
			}
		}

		surfaceTool.GenerateNormals();
		var mesh = surfaceTool.Commit();

		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;

		return meshInstance;
	}

	private void ApplyTextures(MeshInstance3D meshInstance)
	{
		var material = new StandardMaterial3D();

		// Назначение текстур в зависимости от высоты  
		if (heightMap[0, 0] < WaterLevel)
		{
			material.AlbedoTexture = WaterTexture;
		}
		else if (heightMap[0, 0] < ValleyLevel)
		{
			material.AlbedoTexture = GrassTexture;
		}
		else if (heightMap[0, 0] < MountainsLevel)
		{
			material.AlbedoTexture = MountainTexture;
		}

		meshInstance.MaterialOverride = material;
	}

	private void ApplyLandscape()
	{
		var terrainMesh = CreateTerrainMesh();
		ApplyTextures(terrainMesh);

		// Центрирование ландшафта  
		terrainMesh.Position = new Vector3(-MapSize / 2, 0, -MapSize / 2);

		AddChild(terrainMesh);
	}
}

class Perlin2D
{
	byte[] permutationTable;

	public Perlin2D(int seed = 0)
	{
		var rand = new System.Random(seed);
		permutationTable = new byte[1024];
		rand.NextBytes(permutationTable);
	}

	private float[] GetPseudoRandomGradientVector(int x, int y)
	{
		int v = (int)(((x * 1836311903) ^ (y * 2971215073) + 4807526976) & 1023);
		v = permutationTable[v] & 3;

		switch (v)
		{
			case 0: return new float[] { 1, 0 };
			case 1: return new float[] { -1, 0 };
			case 2: return new float[] { 0, 1 };
			default: return new float[] { 0, -1 };
		}
	}

	static float QunticCurve(float t)
	{
		return t * t * t * (t * (t * 6 - 15) + 10);
	}

	static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	static float Dot(float[] a, float[] b)
	{
		return a[0] * b[0] + a[1] * b[1];
	}

	public float Noise(float fx, float fy)
	{
		int left = (int)System.Math.Floor(fx);
		int top = (int)System.Math.Floor(fy);
		float pointInQuadX = fx - left;
		float pointInQuadY = fy - top;

		float[] topLeftGradient = GetPseudoRandomGradientVector(left, top);
		float[] topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
		float[] bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
		float[] bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

		float[] distanceToTopLeft = new float[] { pointInQuadX, pointInQuadY };
		float[] distanceToTopRight = new float[] { pointInQuadX - 1, pointInQuadY };
		float[] distanceToBottomLeft = new float[] { pointInQuadX, pointInQuadY - 1 };
		float[] distanceToBottomRight = new float[] { pointInQuadX - 1, pointInQuadY - 1 };

		float tx1 = Dot(distanceToTopLeft, topLeftGradient);
		float tx2 = Dot(distanceToTopRight, topRightGradient);
		float bx1 = Dot(distanceToBottomLeft, bottomLeftGradient);
		float bx2 = Dot(distanceToBottomRight, bottomRightGradient);

		pointInQuadX = QunticCurve(pointInQuadX);
		pointInQuadY = QunticCurve(pointInQuadY);

		float tx = Lerp(tx1, tx2, pointInQuadX);
		float bx = Lerp(bx1, bx2, pointInQuadX);
		float tb = Lerp(tx, bx, pointInQuadY);

		return tb;
	}

	public float Noise(float fx, float fy, int octaves, float persistence = 0.5f)
	{
		float amplitude = 1;
		float max = 0;
		float result = 0;

		while (octaves-- > 0)
		{
			max += amplitude;
			result += Noise(fx, fy) * amplitude;
			amplitude *= persistence;
			fx *= 2;
			fy *= 2;
		}

		return result / max;
	}
	*/
}
