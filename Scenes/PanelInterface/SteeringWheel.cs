using Godot;
using System;

public partial class SteeringWheel : TextureRect
{
	[Export] public float MaxAngle = 45.0f; 
	[Export] public float ReturnSpeed = 5.0f; 
	[Export] public float TurnSpeed = 10.0f; 

	private float _currentAngle = 0.0f;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		float input = 0.0f;
		
		if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
		{
			input = -1.0f; 
		}
		else if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
		{
			input = 1.0f;
		}

		if (input != 0.0f)
		{
			_currentAngle += input * TurnSpeed * (float)delta * 60.0f;
		}
		else
		{

			_currentAngle = Mathf.Lerp(_currentAngle, 0.0f, ReturnSpeed * (float)delta);
		}

		_currentAngle = Mathf.Clamp(_currentAngle, -MaxAngle, MaxAngle);

		RotationDegrees = _currentAngle;
	}
}
