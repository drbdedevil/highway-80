using Godot;
using System;

public partial class MainMenuScene : Control
{
	public override void _Ready()
	{
		Button exitButton = GetNode<Button>("MarginContainer/VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitButtonPressed;

		Button startButton = GetNode<Button>("MarginContainer/VBoxContainer/StartButton");
		startButton.Pressed += OnStartButtonPressed;
		
		SetupVolumeSliders();
	}

	public override void _Process(double delta)
	{
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
		GetTree().Quit();
	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
	}

}
