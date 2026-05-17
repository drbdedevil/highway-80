using Godot;
using System;

public partial class TestScene : Control
{

	[Export] public Result Result; 
	public override void _Ready()
	{
		Button exitButton = GetNode<Button>("MarginContainer/VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitButtonPressed;

		Button winBtn = GetNode<Button>("MarginContainer/VBoxContainer/WinButton");
		Button loseBtn = GetNode<Button>("MarginContainer/VBoxContainer/LoseButton");

		if (winBtn != null)
			winBtn.Pressed += OnWinPressed;
		
		if (loseBtn != null)
			loseBtn.Pressed += OnLosePressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnExitButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn");
	}

	private void OnWinPressed()
	{
		GD.Print("Нажата кнопка ПОБЕДА");
		if (Result != null)
		{
			Result.ShowResult(true); // Показываем окно победы
		}
		else
		{
			GD.PrintErr("Ошибка: ResultOverlay не назначен в инспекторе!");
		}
	}

	private void OnLosePressed()
	{
		GD.Print("Нажата кнопка ПРОИГРЫШ");
		if (Result != null)
		{
			Result.ShowResult(false); // Показываем окно проигрыша
		}
		else
		{
			GD.PrintErr("Ошибка: ResultOverlay не назначен в инспекторе!");
		}
	}
}
