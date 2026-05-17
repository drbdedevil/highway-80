using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class SegmentPoint
{
	public Vector3 World = new Vector3();
	public Vector3 Screen = new Vector3();
	public float Scale = 1f;
}

public class RoadSegment
{
	public int Index = 0;
	public SegmentPoint Point = new SegmentPoint();
	public Godot.Color Color = Godot.Colors.White;
}

public partial class MainScene : Node2D
{
	[Export]
	public bool DrawDebug = false;

	public Player player = null;

	public List<RoadSegment> segments = new();
	public List<CarObstacle> obstacles = new();
	public List<ObstacleRenderData> obstacleRects = new();

	public float cameraX = 0;
	public float cameraY = 600;
	public float cameraZ = 0;
	public float distToPlayer = 500;
	public float distToPlane = 0;

	public float segmentLength = 200f;
	public int totalSegments = 0;
	public int visibleSegments = 200;
	public float visibleDistance = 0;
	public float roadWidth = 700;
	public float roadLength = 0;
	public int roadLanes = 5;
	public float perspective = 500f;

	public override void _Ready()
	{
		// camera
		distToPlane = 1 / (cameraY / distToPlayer);

		// level
		createRoad();
		totalSegments = segments.Count;
		roadLength = totalSegments * segmentLength;
		
		// player
		player = GetNode("Player") as Player;
		player.init(this);
		player.restart();

		// obstacles
		visibleDistance = visibleSegments * segmentLength;
		spawnCars();
	}

	public override void _Process(double delta)
	{
		// player
		player.updatePosition(delta);

		// camera
		// cameraZ += speed * (float)delta;
		cameraX = player.worldPosition.X * roadWidth;
		cameraZ = player.worldPosition.Z - distToPlayer;
		if (cameraZ < 0)
			cameraZ += roadLength;

		// obstacles
		updateObstacles((float)delta);
		checkPlayerCollisions();

		QueueRedraw();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
			{
				distToPlayer = Mathf.Min(distToPlayer + 10, 1000); // шаг 10, например
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				distToPlayer = Mathf.Max(distToPlayer - 10, 0);
			}
		}
	}

	public override void _Draw()
	{
		render3D();
		renderObstacles();
		
		if (DrawDebug)
		{
			renderPlayerDebug();
		}
	}

	public void render3D()
	{
		DrawRect(GetViewportRect(), Colors.Black);

		Vector2 screen = GetViewport().GetVisibleRect().Size;
		var clipBottomLine = screen.Y;

		var baseSegment = getSegment(cameraZ);
		var baseIndex = baseSegment.Index;

		for (int i = 0; i < visibleSegments; ++i)
		{
			var currIndex = (baseIndex + i) % totalSegments;
			var currSegment = segments[currIndex];

			var offsetZ = (currIndex < baseIndex) ? roadLength : 0;

			project3D(currSegment.Point, offsetZ);

			var currBottomLine = currSegment.Point.Screen.Y;

			if (i > 0 && currBottomLine <= clipBottomLine)
			{
				var prevIndex = (currIndex > 0) ? currIndex - 1 : totalSegments - 1;
				var prevSegment = segments[prevIndex];

				var p1 = prevSegment.Point.Screen;
				var p2 = currSegment.Point.Screen;

				drawSegment(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, currSegment.Color);
				drawEdges(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, Godot.Colors.DarkGoldenrod);

				if (currIndex % 5 == 0)
				{
					drawDashedLineMarking(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, Godot.Colors.White);
				}

				clipBottomLine = currBottomLine;
			}
		}
	}

	public void project2D(SegmentPoint Point)
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;

		Point.Screen.X = screen.X / 2;
		Point.Screen.Y = screen.Y - Point.World.Z;
		Point.Screen.Z = roadWidth;
	}
	public void project3D(SegmentPoint Point, float offsetZ)
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;

		var transX = Point.World.X - cameraX;
		var transY = Point.World.Y - cameraY;
		var transZ = Point.World.Z - cameraZ - offsetZ;

		Point.Scale = distToPlane / transZ;

		var projectedX = Point.Scale * transX;
		var projectedY = Point.Scale * transY;
		var projectedW = Point.Scale * roadWidth;

		Point.Screen.X = (float)Math.Round((1 + projectedX) * screen.X / 2);
		Point.Screen.Y = (float)Math.Round((1 - projectedY) * screen.Y / 2);
		Point.Screen.Z = (float)Math.Round(projectedW * screen.X / 2);
	}

	private void drawSegment(float x1, float y1, float w1, float x2, float y2, float w2, Godot.Color Color)
	{
		if (w1 <= 0 || w2 <= 0) return;

		DrawColoredPolygon(new Vector2[] {
			new Vector2(x1 - w1, y1),
			new Vector2(x1 + w1, y1),
			new Vector2(x2 + w2, y2),
			new Vector2(x2 - w2, y2)
		}, Color);
	}
	private void drawEdges(float x1, float y1, float w1, float x2, float y2, float w2, Godot.Color Color)
	{
		DrawLine(new Vector2(x1 - w1, y1), new Vector2(x2 - w2, y2), Color, 3f);
		DrawLine(new Vector2(x1 + w1, y1), new Vector2(x2 + w2, y2), Color, 3f);
	}
	private void drawDashedLineMarking(float x1, float y1, float w1, float x2, float y2, float w2, Godot.Color Color)
	{
		if (w1 <= 0 || w2 <= 0) return;
		if (w1 < 2f && w2 < 2f) return;

		var line_w1 = (w1 / 40) / 2;
		var line_w2 = (w2 / 40) / 2;

		var lane_w1 = (w1 * 2) / roadLanes;
		var lane_w2 = (w2 * 2) / roadLanes;

		var lane_x1 = x1 - w1;
		var lane_x2 = x2 - w2;

		for (int i = 1; i < roadLanes; ++i)
		{
			lane_x1 += lane_w1;
			lane_x2 += lane_w2;

			DrawColoredPolygon(new Vector2[] {
				new Vector2(lane_x1 - line_w1, y1),
				new Vector2(lane_x1 + line_w1, y1),
				new Vector2(lane_x2 + line_w2, y2),
				new Vector2(lane_x2 - line_w2, y2)
			}, Color);
		}
	}

	public void createRoad()
	{
		createSection(1000);
	}

	public void createSection(int Segments)
	{
		for (int i = 0; i < Segments; ++i)
		{
			createSegment();
		}
	}

	public void createSegment()
	{
		segments.Add(new RoadSegment
			{
				Index = segments.Count,
				Point = new SegmentPoint
				{
					World = new Vector3(0, 0, segments.Count * segmentLength),
					Screen = new Vector3(0, 0, 0),
					Scale = -1
				},
				Color = new Color(0.05f, 0.05f, 0.05f)
			});
	}

	public RoadSegment getSegment(float positionZ)
	{
		if (positionZ < 0)
			positionZ += this.roadLength;

		int index = (int)(positionZ / this.segmentLength) % totalSegments;

		return segments[index];
	}

	public void spawnCars()
	{
		var tex = GD.Load<Texture2D>("res://icon.svg");

		float spacing = 2500f;

		int carCount = (int)(roadLength / spacing);

		for (int i = 0; i < carCount; i++)
		{
			int carsInRow = GD.RandRange(1, 3);

			List<int> usedLanes = new();

			for (int j = 0; j < carsInRow; j++)
			{
				int lane;

				do
				{
					lane = GD.RandRange(0, roadLanes - 1);
				}
				while (usedLanes.Contains(lane));

				usedLanes.Add(lane);

				float laneX = getLaneCenter(lane);

				obstacles.Add(new CarObstacle
				{
					Texture = tex,

					World = new Vector3(
						laneX,
						0,
						i * spacing
					),

					Speed = 700 + GD.Randf() * 600,
					CurrentLane = lane,
					TargetLane = lane
				});
			}
		}
	}

	public void updateObstacles(float delta)
	{
		foreach (var car in obstacles)
		{
			float dz = car.World.Z - cameraZ;

			if (dz < 0)
				dz += roadLength;

			if (dz > visibleDistance)
				continue;

			tryChangeLane(car);

			updateLaneChanging(car, delta);

			car.World.Z += car.Speed * delta;

			if (car.World.Z >= roadLength)
				car.World.Z -= roadLength;
		}
	}

	public void renderObstacles()
	{
		obstacleRects.Clear();

		Vector2 screen = GetViewport().GetVisibleRect().Size;
		
		// Сначала вычисляем расстояние до камеры для всех машин
		var carsWithDistance = new List<(CarObstacle car, float dz)>();
		
		foreach (var car in obstacles)
		{
			float dz = car.World.Z - cameraZ;
			if (dz < 0)
				dz += roadLength;
			
			if (dz <= 1)
				continue;
				
			carsWithDistance.Add((car, dz));
		}
		
		// Сортируем от дальних к ближним (по убыванию dz)
		carsWithDistance.Sort((a, b) => b.dz.CompareTo(a.dz));
		
		// Рисуем в отсортированном порядке
		foreach (var (car, dz) in carsWithDistance)
		{
			if (dz > visibleDistance)
				continue;

			float scale = distToPlane / dz;
			
			float screenX = (1 + scale * (car.World.X - cameraX)) * screen.X / 2;
			float screenY = (1 - scale * (-cameraY)) * screen.Y / 2;
			
			float spriteWidth = car.Texture.GetWidth() * scale * screen.X / 2;
			float spriteHeight = car.Texture.GetHeight() * scale * screen.X / 2;
			
			Rect2 rect = new Rect2(
				screenX - spriteWidth / 2,
				screenY - spriteHeight,
				spriteWidth,
				spriteHeight
			);
			
			Rect2 collisionRect = new Rect2(
				rect.Position.X + rect.Size.X * 0.4f,
				rect.Position.Y + rect.Size.Y * 0.4f,
				rect.Size.X * 0.2f,
				rect.Size.Y * 0.2f
			);
			obstacleRects.Add(new ObstacleRenderData
			{
				Obstacle = car,
				Rect = collisionRect
			});

			DrawTextureRect(car.Texture, rect, false);

			if (DrawDebug)
			{
				DrawRect(collisionRect, Colors.Red, false, 2f);
			}
		}
	}

	public float getLaneCenter(int lane)
	{
		float laneWidth = (roadWidth * 2f) / roadLanes;

		float leftEdge = -roadWidth;

		return leftEdge + laneWidth * lane + laneWidth / 2f;
	}

	public void updateLaneChanging(CarObstacle car, float delta)
	{
		if (!car.IsChangingLane)
			return;

		car.LaneChangeProgress += delta * 2f;

		float fromX = getLaneCenter(car.CurrentLane);
		float toX = getLaneCenter(car.TargetLane);

		car.World.X = Mathf.Lerp(
			fromX,
			toX,
			car.LaneChangeProgress
		);

		if (car.LaneChangeProgress >= 1f)
		{
			car.CurrentLane = car.TargetLane;

			car.World.X = getLaneCenter(car.CurrentLane);

			car.IsChangingLane = false;
		}
	}
	public void tryChangeLane(CarObstacle car)
	{
		if (car.IsChangingLane)
			return;

		foreach (var other in obstacles)
		{
			if (other == car)
				continue;

			bool sameLane =
				other.CurrentLane == car.CurrentLane;

			float dz = other.World.Z - car.World.Z;

			if (dz < 0)
				dz += roadLength;

			bool closeAhead = dz < 800;

			bool slower =
				other.Speed < car.Speed;

			if (sameLane && closeAhead && slower)
			{
				int leftLane = car.CurrentLane - 1;
				int rightLane = car.CurrentLane + 1;

				if (isLaneFree(car, leftLane))
				{
					startLaneChange(car, leftLane);
					return;
				}

				if (isLaneFree(car, rightLane))
				{
					startLaneChange(car, rightLane);
					return;
				}
			}
		}
	}
	public void startLaneChange(CarObstacle car, int newLane)
	{
		car.TargetLane = newLane;

		car.IsChangingLane = true;

		car.LaneChangeProgress = 0f;
	}
	public bool isLaneFree(CarObstacle car, int lane)
	{
		if (lane < 0 || lane >= roadLanes)
			return false;

		foreach (var other in obstacles)
		{
			if (other == car)
				continue;

			if (other.CurrentLane != lane)
				continue;

			float dz = Mathf.Abs(
				other.World.Z - car.World.Z
			);

			if (dz > roadLength / 2)
				dz = roadLength - dz;

			if (dz < 1200)
				return false;
		}

		return true;
	}

	public void checkPlayerCollisions()
	{
		if (player.collisionTimer > 0f)
			return;

		float playerWidth =
			player.texturePlayer.Texture.GetWidth() *
			player.texturePlayer.Scale.X;

		float playerHeight =
			player.texturePlayer.Texture.GetHeight() *
			player.texturePlayer.Scale.Y;

		Rect2 playerRect = new Rect2(
			player.texturePlayer.Position.X + playerWidth * 0.125f,
			player.texturePlayer.Position.Y + playerHeight * 0.125f,
			playerWidth * 0.75f,
			playerHeight * 0.75f
		);

		bool hit = false;
		float bestObstacleSpeed = player.speed;

		foreach (var obstacleData in obstacleRects)
		{
			if (!playerRect.Intersects(obstacleData.Rect))
				continue;

			var obstacle = obstacleData.Obstacle;

			hit = true;

			// берём самое медленное препятствие, с которым столкнулись
			bestObstacleSpeed = Math.Min(bestObstacleSpeed, obstacle.Speed);
		}

		if (!hit)
			return;

		// мягко “прилипли” к скорости препятствия
		player.speed = bestObstacleSpeed;

		// небольшой lock, чтобы не дрожало
		player.collisionTimer = 0.25f;
		player.noAccelerationTimer = 0.6f;
	}

	public void renderPlayerDebug()
	{
		float playerWidth =
			player.texturePlayer.Texture.GetWidth() *
			player.texturePlayer.Scale.X;

		float playerHeight =
			player.texturePlayer.Texture.GetHeight() *
			player.texturePlayer.Scale.Y;

		Rect2 playerRect = new Rect2(
			player.texturePlayer.Position.X + playerWidth * 0.125f,
			player.texturePlayer.Position.Y + playerHeight * 0.125f,
			playerWidth * 0.75f,
			playerHeight * 0.75f
		);

		DrawRect(playerRect, Colors.Red, false, 2f);
	}
}
