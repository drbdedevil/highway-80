using Godot;
using System;

public partial class IntroScene : Control
{
	private VideoStreamPlayer _videoPlayer;

	public override void _Ready()
	{
		var player = GetNode<VideoStreamPlayer>("VideoStreamPlayer");
		player.Play();

		var timer = GetTree().CreateTimer(3.0f);
		timer.Timeout += () =>
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn");
		};
	}

	public override void _Process(double delta)
	{
	}

}
