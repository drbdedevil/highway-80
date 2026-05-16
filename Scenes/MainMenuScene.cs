using Godot;
using System;

public partial class MainMenuScene : Node
{
	public override void _Ready()
	{
		Button exitButton = GetNode<Button>("MarginContainer/VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitButtonPressed;

		Button startButton = GetNode<Button>("MarginContainer/VBoxContainer/StartButton");
		startButton.Pressed += OnStartButtonPressed;
		
		Button settingsButton = GetNode<Button>("MarginContainer/VBoxContainer/SettinsButton");
		settingsButton.Pressed += OnSettingsButtonPressed;
	}

	public override void _Process(double delta)
	{
	}

	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn");
	}

	private void OnSettingsButtonPressed()
	{
	}
}
