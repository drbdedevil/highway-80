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

		HSlider sfxSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/SFXSlider");
		HSlider musicSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/MusicSlider");

		Label label = GetNode<Label>("MarginContainer/VBoxContainer/Label");
		Label label2 = GetNode<Label>("MarginContainer/VBoxContainer/Label2");


		_clickSound = GetNode<AudioStreamPlayer>("ClickSound");

		AnimateEntrance(startButton, exitButton, sfxSlider, musicSlider, label, label2);
		
		SetupVolumeSliders();
	}

	public override void _Process(double delta)
	{
	}

	private void AnimateEntrance(Button startBtn, Button exitBtn, HSlider sfxSld, HSlider musicSld, Label label, Label label2)
	{
		var tween = CreateTween();
		tween.SetParallel(true);

		float screenWidth = GetViewportRect().Size.X;
		
		Vector2 startPosStart = startBtn.Position;
		Vector2 startPosExit = exitBtn.Position;
		Vector2 startPosSfx = sfxSld.Position;
		Vector2 startPosMusic = musicSld.Position;
		Vector2 starPostlabel = label.Position;
		Vector2 starPostlabel2 = label.Position;

		startBtn.Position = new Vector2(screenWidth, startPosStart.Y);
		tween.TweenProperty(startBtn, "position:x", startPosStart.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		
		tween.TweenInterval(0.1f);

		exitBtn.Position = new Vector2(-screenWidth, startPosExit.Y); 
		tween.TweenProperty(exitBtn, "position:x", startPosExit.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

		tween.TweenInterval(0.1f);

		sfxSld.Position = new Vector2(screenWidth, startPosSfx.Y);
		tween.TweenProperty(sfxSld, "position:x", startPosSfx.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

		tween.TweenInterval(0.1f);

		label.Position = new Vector2(screenWidth, starPostlabel.Y);
		tween.TweenProperty(label, "position:x", starPostlabel.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

		tween.TweenInterval(0.1f);

		musicSld.Position = new Vector2(-screenWidth, startPosMusic.Y);
		tween.TweenProperty(musicSld, "position:x", startPosMusic.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
			 
		tween.TweenInterval(0.1f);

		label2.Position = new Vector2(-screenWidth, starPostlabel2.Y);
		tween.TweenProperty(label2, "position:x", starPostlabel2.X, 1f)
			 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

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
