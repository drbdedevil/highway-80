using Godot;
using System;

public partial class MainMenuScene : Control
{

	private AudioStreamPlayer _clickSound;
	public override void _Ready()
	{
		Button exitButton = GetNode<Button>("MarginContainer/VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitButtonPressed;

		Button startButton = GetNode<Button>("MarginContainer/VBoxContainer/StartButton");
		startButton.Pressed += OnStartButtonPressed;

		_clickSound = GetNode<AudioStreamPlayer>("ClickSound");
		
		SetupVolumeSliders();
	}

	public override void _Process(double delta)
	{
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

	private void SetupVolumeSliders()
	{
		HSlider sfxSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/SFXSlider");
		HSlider musicSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/MusicSlider");

		sfxSlider.Value = MainMenuVolumeSettings.Instance.GetSfxVolume();
		musicSlider.Value = MainMenuVolumeSettings.Instance.GetMusicVolume();

		sfxSlider.ValueChanged += (value) => MainMenuVolumeSettings.Instance.SetSfxVolume((float)value);
		musicSlider.ValueChanged += (value) => MainMenuVolumeSettings.Instance.SetMusicVolume((float)value);
	}

	private void OnExitButtonPressed()
	{
		PlayClickSound(() => GetTree().Quit());		
	}

	private void OnStartButtonPressed()
	{
		PlayClickSound(() => GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn"));	
	}

}
