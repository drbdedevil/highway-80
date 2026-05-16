using Godot;
using System;

public partial class MainMenuVolumeSettings : Node
{
	public static MainMenuVolumeSettings Instance { get; private set; }

	private int _sfxBusIndex;
	private int _musicBusIndex;
	private const string ConfigPath = "user://settings.cfg";

	public override void _EnterTree()
	{
		if (Instance == null)
		{
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
		}
	}

	public override void _Ready()
	{
		InitBuses();
		LoadAndApplySettings();
	}

	public override void _Process(double delta)
	{
		
	}

	private void InitBuses()
	{
		_sfxBusIndex = AudioServer.GetBusIndex("SFX");
		_musicBusIndex = AudioServer.GetBusIndex("Music");

		if (_sfxBusIndex == -1)
		{
			AudioServer.AddBus();
			AudioServer.SetBusName(AudioServer.BusCount - 1, "SFX");
			_sfxBusIndex = AudioServer.GetBusIndex("SFX");
		}
		if (_musicBusIndex == -1)
		{
			AudioServer.AddBus();
			AudioServer.SetBusName(AudioServer.BusCount - 1, "Music");
			_musicBusIndex = AudioServer.GetBusIndex("Music");
		}
	}

	private void LoadAndApplySettings()
	{
		var config = new ConfigFile();
		Error err = config.Load(ConfigPath);

		float sfx = 0.5f;
		float music = 0.5f;

		if (err == Error.Ok)
		{
			sfx = (float)config.GetValue("audio", "sfx_volume", 0.5);
			music = (float)config.GetValue("audio", "music_volume", 0.5);
		}

		SetSfxVolume(sfx);
		SetMusicVolume(music);
	}

	public void SetSfxVolume(float value)
	{
		AudioServer.SetBusVolumeDb(_sfxBusIndex, Mathf.LinearToDb(value));
		SaveSetting("sfx_volume", value);
	}

	public void SetMusicVolume(float value)
	{
		AudioServer.SetBusVolumeDb(_musicBusIndex, Mathf.LinearToDb(value));
		SaveSetting("music_volume", value);
	}

	public float GetSfxVolume()
	{
		return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(_sfxBusIndex));
	}

	public float GetMusicVolume()
	{
		return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(_musicBusIndex));
	}

	private void SaveSetting(string key, float value)
	{
		var config = new ConfigFile();
		config.Load(ConfigPath);
		config.SetValue("audio", key, value);
		config.Save(ConfigPath);
	}
}
