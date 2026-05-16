using Godot;
using System;

public partial class TestScene : Control
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Button exitButton = GetNode<Button>("MarginContainer/VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnExitButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn");
	}
}
