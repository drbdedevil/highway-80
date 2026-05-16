using Godot;
using System;
using System.Collections.Generic;

// public class RoadSegment
// {
// 	public float Curve { get; set; }
// 	public float Length { get; set; }
// 	public float Y { get; set; }
// 	public int SpriteType { get; set; }
// }

public partial class MainScene : Node2D
{
	public Circuit circuit = null;
	private float speed = 2000f;          // текущая скорость (единиц/сек)
	private const float maxSpeed = 800f;
	private const float acceleration = 400f;
	private const float deceleration = 300f; 

	public override void _Ready()
	{
		circuit = new Circuit(this);
		circuit.create();

		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawRect(GetViewportRect(), Colors.Black);
		
		circuit.render3D();
	}


	public override void _Process(double delta)
	{
		circuit.cameraZ += speed * (float)delta;

		// if (circuit.cameraZ >= circuit.GetTotalLength())
		// 	circuit.cameraZ -= circuit.GetTotalLength();

		QueueRedraw();
	}
	
}
