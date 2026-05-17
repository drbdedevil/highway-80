using Godot;
using System;

public partial class GearStick : TextureRect // Или Sprite2D
{
	private Vector2 Pos1 = new Vector2(25, -190);
	private Vector2 Pos2 = new Vector2(25, -50);
	private Vector2 Pos3 = new Vector2(146, -190);
	private Vector2 Pos4 = new Vector2(146, -50);

	private int _currentGear = 1;
	private float _moveSpeed = 10.0f;

	public override void _Ready()
	{
		SetGear(1);
	}

	public override void _Process(double delta)
	{
		MoveToCurrentGear((float)delta);
	}

	/// <summary>
	/// Вызывай этот метод , когда меняется передача
	/// </summary>
	public void SetGear(int gear)
	{
		_currentGear = Mathf.Clamp(gear, 1, 4);
	}

	private void MoveToCurrentGear(float delta)
	{
		Vector2 targetPosition = Vector2.Zero;

		switch (_currentGear)
		{
			case 1: targetPosition = Pos1; break;
			case 2: targetPosition = Pos2; break;
			case 3: targetPosition = Pos3; break;
			case 4: targetPosition = Pos4; break;
		}

		Position = Position.Lerp(targetPosition, _moveSpeed * delta);
	}
}
