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

	public float cameraZ = 0;
	public float cameraX = 0;

	public float speed = 1000f;

	public float segmentLength = 200f;
	public float roadWidth = 1000;
	public float perspective = 300f;

	public override void _Ready()
	{
		createRoad();
	}

	public override void _Process(double delta)
	{
		cameraZ += speed * (float)delta;
		QueueRedraw();
	}

	public override void _Draw()
	{
		render3D();
	}

	public void render3D()
	{
		DrawRect(GetViewportRect(), Colors.Black);

		Vector2 screenCenter = GetViewport().GetVisibleRect().Size * 0.5f;

		var currSegment = segments[1];
		var prevSegment = segments[0];

		project2D(currSegment.Point);
		project2D(prevSegment.Point);

		var p1 = prevSegment.Point.Screen;
		var p2 = currSegment.Point.Screen;

		drawSegment(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, currSegment.Color);
	}

	public void project2D(SegmentPoint Point)
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;

		Point.Screen.X = screen.X / 2;
		Point.Screen.Y = screen.Y - Point.World.Z;
		Point.Screen.Z = roadWidth;
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

	public void createRoad()
	{
		createSection(10);
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
		for (int i = 0; i < 40; i++)
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
				Color = Godot.Colors.Green
			});
		}
	}
}
