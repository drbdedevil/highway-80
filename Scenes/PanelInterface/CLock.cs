using Godot;
using System;

public partial class CLock : TextureRect
{
	[Export] public float StartAngle = 0.0f; 

	public float _currentTime = 0.0f;

	public override void _Ready()
	{
		RotationDegrees = StartAngle;
		_currentTime = 0.0f;
	}

	public override void _Process(double delta)
	{
		_currentTime += (float)delta;
		float angleOffset = _currentTime % 60.0f * 6.0f;
		RotationDegrees = StartAngle + angleOffset;
	}
}
