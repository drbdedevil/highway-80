using Godot;
using System;

public partial class Speedometer : TextureRect
{

	[Export] public float MinSpeed = 0.0f;
	[Export] public float MaxSpeed = 12000.0f;
	[Export] public float MinAngle = -90.0f;
	[Export] public float MaxAngle = 90.0f;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void SetSpeed(float speed)
	{
		float normalized = Mathf.InverseLerp(MinSpeed, MaxSpeed, speed);
		
		float angle = Mathf.Lerp(MinAngle, MaxAngle, normalized);
		
		RotationDegrees = angle;
	}
}
