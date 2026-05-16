using Godot;
using System;

public partial class MainMenuVolumeSettings : Node
{
	private HSlider _sfxSlider;
	private HSlider _musicSlider;
	private int _sfxBusIndex;
	private int _musicBusIndex;

	public override void _Ready()
	{
		_sfxSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/SFXSlider");
		_musicSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/MusicSlider");

		_sfxBusIndex = AudioServer.GetBusIndex("SFX");
		_musicBusIndex = AudioServer.GetBusIndex("Music");

		var config = new ConfigFile();
		Error err = config.Load("user://settings.cfg");

		float sfxValue = 0.5f;
		float musicValue = 0.5f;

		if (err == Error.Ok)
		{
			sfxValue = (float)config.GetValue("audio", "sfx_volume", 0.5);
			musicValue = (float)config.GetValue("audio", "music_volume", 0.5);
		}

		AudioServer.SetBusVolumeDb(_sfxBusIndex, Mathf.LinearToDb(sfxValue));
		AudioServer.SetBusVolumeDb(_musicBusIndex, Mathf.LinearToDb(musicValue));

		_sfxSlider.Value = sfxValue;
		_musicSlider.Value = musicValue;

		_sfxSlider.ValueChanged += OnSfxVolumeChanged;
		_musicSlider.ValueChanged += OnMusicVolumeChanged;
	}

	public override void _Process(double delta)
	{
		
	}

	private void OnSfxVolumeChanged(double value)
	{
		float linearValue = (float)value;
		AudioServer.SetBusVolumeDb(_sfxBusIndex, Mathf.LinearToDb(linearValue));
		SaveSettings(linearValue, (float)_musicSlider.Value);
	}

	private void OnMusicVolumeChanged(double value)
	{
		float linearValue = (float)value;
		AudioServer.SetBusVolumeDb(_musicBusIndex, Mathf.LinearToDb(linearValue));
		SaveSettings((float)_sfxSlider.Value, linearValue);
	}

	private void SaveSettings(float sfxVol, float musicVol)
	{
		var config = new ConfigFile();
		config.SetValue("audio", "sfx_volume", sfxVol);
		config.SetValue("audio", "music_volume", musicVol);
		config.Save("user://settings.cfg");
	}
}
