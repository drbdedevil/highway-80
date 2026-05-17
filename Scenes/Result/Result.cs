using Godot;
using System;

public partial class Result : Control
{
	[Export] public TextureRect WinBackground;
	[Export] public TextureRect LoseBackground;
	[Export] public Label TitleLabel;
	[Export] public Button RestartButton;
	[Export] public Button MenuButton;

	private AudioStreamPlayer _winSound;
	private AudioStreamPlayer _loseSound;

	private AudioStreamPlayer _clickSound;

	public override void _Ready()
	{
		Visible = false;

		if (WinBackground != null) WinBackground.Visible = false;
		if (LoseBackground != null) LoseBackground.Visible = false;

		_clickSound = GetNode<AudioStreamPlayer>("ClickSound");

		_winSound = GetNode<AudioStreamPlayer>("WinSound");
		_loseSound = GetNode<AudioStreamPlayer>("LoseSound");

		RestartButton.Pressed += OnRestartPressed;
		MenuButton.Pressed += OnMenuPressed;
	}

	public void ShowResult(bool isWin)
	{
		Visible = true;

		if (isWin)
		{
			TitleLabel.Text = "YOU WIN!";
			
			if (WinBackground != null) WinBackground.Visible = true;
			if (LoseBackground != null) LoseBackground.Visible = false;

			if (!_winSound.Playing)
				_winSound.Play();
		}
		else
		{
			TitleLabel.Text = "YOU LOSE!";
			
			if (WinBackground != null) WinBackground.Visible = false;
			if (LoseBackground != null) LoseBackground.Visible = true;

			if (!_loseSound.Playing)
				_loseSound.Play();
		}

	}

	private void PlayClickSound(Action action)
	{
		if (_clickSound != null)
		{
			_clickSound.Play();
			var timer = GetTree().CreateTimer(0.3f);
			timer.Timeout += () =>
			{
				action();
			};
		}
	}

	private void OnRestartPressed()
	{
		if (_winSound != null && _winSound.Playing) _winSound.Stop();
		if (_loseSound != null && _loseSound.Playing) _loseSound.Stop();

		PlayClickSound(() =>GetTree().ChangeSceneToFile("res://Scenes/MainScene.tscn"));
	}

	private void OnMenuPressed()
	{
		if (_winSound != null && _winSound.Playing) _winSound.Stop();
		if (_loseSound != null && _loseSound.Playing) _loseSound.Stop();

		PlayClickSound(() =>GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn"));
	}
}
