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
	List<RoadSegment> segments = new();

	public float cameraX = 0;
	public float cameraY = 1000;
	public float cameraZ = 0;
	public float distToPlayer = 100;
	public float distToPlane = 0;

	public float speed = 1000f;

	public float segmentLength = 200f;
	public int totalSegments = 0;
	public int visibleSegments = 200;
	public float roadWidth = 1000;
	public float roadLength = 0;
	public int roadLanes = 3;
	public float perspective = 300f;

	public override void _Ready()
	{
		// camera
		distToPlane = 1 / (cameraY / distToPlayer);

		// level
		createRoad();
		totalSegments = segments.Count;
		roadLength = totalSegments * segmentLength;
	}

	public override void _Process(double delta)
	{
		// camera
		// cameraZ += speed * (float)delta;
		cameraZ = -distToPlayer;

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
	}

	public void render3D()
	{
		DrawRect(GetViewportRect(), Colors.Black);

		var baseSegment = getSegment(cameraZ);
		var baseIndex = baseSegment.Index;

		for (int i = 0; i < visibleSegments; ++i)
		{
			var currIndex = (baseIndex + i) % totalSegments;
			var currSegment = segments[currIndex];

			project3D(currSegment.Point);

			if (i > 0)
			{
				var prevIndex = (currIndex > 0) ? currIndex - 1 : totalSegments - 1;
				var prevSegment = segments[prevIndex];

				var p1 = prevSegment.Point.Screen;
				var p2 = currSegment.Point.Screen;

				drawSegment(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, currSegment.Color);
				drawEdges(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, Godot.Colors.DarkGoldenrod);

				if (currIndex % 2 == 0)
				{
					drawDashedLineMarking(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, Godot.Colors.White);
				}
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
	public void project3D(SegmentPoint Point)
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;

		var transX = Point.World.X - cameraX;
		var transY = Point.World.Y - cameraY;
		var transZ = Point.World.Z - cameraZ;

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
		var line_w1 = (w1 / 20) / 2;
		var line_w2 = (w2 / 20) / 2;

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
}
