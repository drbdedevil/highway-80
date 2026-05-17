using Godot;
using System.Numerics;

public enum SteeringVisualState
{
	Straight,
	TurnLightLeft,
	TurnHardLeft,
	TurnLightRight,
	TurnHardRight
}

public partial class Player : Node2D
{
	public MainScene mainScene = null;
	public TextureRect texturePlayer = null;

	[Export] public Texture2D Straight1;
	[Export] public Texture2D Straight2;
	[Export] public Texture2D Straight3;

	[Export] public Texture2D Turn1;
	[Export] public Texture2D Turn2;

	[Export]
	public float playerHeightOffset = 0f;
	[Export]
	public float playerSpeedModifier = 1f;
	[Export]
	public float playerScaleK = 0.1f;

	[Export]
	public float acceleration = 800f;
	public float noAccelerationTimer = 0f;
	[Export]
	public float collisionSlowdown = 0.4f;
	[Export]
	public float offroadSlowdown = 0.6f;
	[Export] 
	public float offroadAccelerationPenalty = 0.3f;
	[Export]
	public float collisionTimer = 0f;

	public float relativePosition = 0f;
	public Godot.Vector3 worldPosition = new Godot.Vector3();

	public float playerWidth = 0;
	public Godot.Vector4 screenCoord = new Godot.Vector4();

	public float maxSpeed;
	public float speed;

	public float leftHoldTime = 0f;
	public float rightHoldTime = 0f;

	[Export]
	public float hardTurnThreshold = 0.25f;
	private Tween spriteTween;
	private Tween rotationTween;
	private SteeringVisualState lastSteeringState = SteeringVisualState.Straight;

	public override void _Ready()
	{
		base._Ready();

		texturePlayer = GetNode("Texture") as TextureRect;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		bool left = Input.IsActionPressed("steer_left");
		bool right = Input.IsActionPressed("steer_right");

		if (left)
		{
			leftHoldTime += dt;
			rightHoldTime = Mathf.MoveToward(rightHoldTime, 0f, dt * 4f);
			
			if (mainScene?.cityBackground != null)
			{
				float newOffset = mainScene.cityBackground.offsetX + 200f * dt;
				mainScene.cityBackground.UpdateOffset(newOffset);
			}
		}
		else if (right)
		{
			rightHoldTime += dt;
			leftHoldTime = Mathf.MoveToward(leftHoldTime, 0f, dt * 4f);
			
			if (mainScene?.cityBackground != null)
			{
				float newOffset = mainScene.cityBackground.offsetX - 200f * dt;
				mainScene.cityBackground.UpdateOffset(newOffset);
			}
		}
		else
		{
			leftHoldTime = Mathf.MoveToward(leftHoldTime, 0f, dt * 4f);
			rightHoldTime = Mathf.MoveToward(rightHoldTime, 0f, dt * 4f);
			
			// if (mainScene?.cityBackground != null)
			// {
			// 	float newOffset = Mathf.Lerp(mainScene.cityBackground.offsetX, 0f, dt * 3f);
			// 	mainScene.cityBackground.UpdateOffset(newOffset);
			// }
		}

		float steer = Input.GetActionStrength("steer_right") - Input.GetActionStrength("steer_left");
		float steerSpeed = 1.7f;
		worldPosition.X += steer * steerSpeed * dt;
		worldPosition.X = Mathf.Clamp(worldPosition.X, -5f, 5f);

		updateSpriteSmooth(getVisualState());
	}

	public void init(MainScene inMainScene)
	{
		mainScene = inMainScene;
		maxSpeed = mainScene.segmentLength / (1f / 60f);

		updateTexture();
	}

	public void restart()
	{
		worldPosition = new Godot.Vector3();
		speed = maxSpeed;
	}

	public void updatePosition(double delta)
	{
		float dt = (float)delta;

		if (collisionTimer > 0f)
			collisionTimer -= dt;

		if (noAccelerationTimer > 0f)
			noAccelerationTimer -= dt;

		float targetSpeed = maxSpeed;

		// collision / slowdown lock
		if (collisionTimer > 0f)
			targetSpeed = Mathf.Min(targetSpeed, maxSpeed * collisionSlowdown);

		// offroad penalty
		float roadCenter = mainScene.getRoadCenterX(worldPosition.Z);
		float leftBorder = roadCenter - mainScene.roadWidth;
		float rightBorder = roadCenter + mainScene.roadWidth;

		bool offroad = mainScene.cameraX < leftBorder || mainScene.cameraX > rightBorder;

		// GD.Print($"cameraX={mainScene.cameraX}, roadCenter={roadCenter}, leftBorder={roadCenter - mainScene.roadWidth}, rightBorder={roadCenter + mainScene.roadWidth}");

		if (offroad)
		{
			// GD.Print("OFFROAD");
			targetSpeed = Mathf.Min(targetSpeed, maxSpeed * offroadSlowdown);
		}

		// если есть lock на ускорение
		if (noAccelerationTimer > 0f)
		{
			// не даём резко ускоряться, но и не ломаем физику
			speed = Mathf.MoveToward(speed, targetSpeed, acceleration * 0.3f * dt);
		}
		else
		{
			speed = Mathf.MoveToward(speed, targetSpeed, acceleration * dt);
		}

		worldPosition.Z += speed * dt * playerSpeedModifier;

		if (worldPosition.Z >= mainScene.roadLength)
			worldPosition.Z -= mainScene.roadLength;

		updateTexture();
	}

	public void updateTexture()
	{
		if (texturePlayer?.Texture == null) return;

		Godot.Vector2 screen = GetViewport().GetVisibleRect().Size;
		float originalWidth = texturePlayer.Texture.GetWidth();
		float originalHeight = texturePlayer.Texture.GetHeight();

		// Пересчитываем офсет при каждом обновлении (адаптивно к экрану)
		playerHeightOffset = screen.Y / 3;  // 1/3 от высоты экрана
		
		float targetWidth = screen.X * playerScaleK;
		float targetHeight = targetWidth * originalHeight / originalWidth;

		float scaleX = targetWidth / originalWidth;
		float scaleY = targetHeight / originalHeight;
		texturePlayer.Scale = new Godot.Vector2(scaleX, scaleY);

		float posX = (screen.X - targetWidth) / 2;
		float posY = screen.Y - targetHeight - playerHeightOffset;

		texturePlayer.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		texturePlayer.Position = new Godot.Vector2(posX, posY);
	}

	public void updateSprite(SteeringVisualState state)
	{
		texturePlayer.FlipH = false;

		switch (state)
		{
			case SteeringVisualState.Straight:
			{
				int frame = (int)(Time.GetTicksMsec() / 120 % 3);

				switch (frame)
				{
					case 0:
						texturePlayer.Texture = Straight1;
						break;

					case 1:
						texturePlayer.Texture = Straight2;
						break;

					case 2:
						texturePlayer.Texture = Straight3;
						break;
				}

				break;
			}

			case SteeringVisualState.TurnLightLeft:
				texturePlayer.Texture = Turn1;
				break;

			case SteeringVisualState.TurnHardLeft:
				texturePlayer.Texture = Turn2;
				break;

			case SteeringVisualState.TurnLightRight:
				texturePlayer.Texture = Turn1;
				texturePlayer.FlipH = true;
				break;

			case SteeringVisualState.TurnHardRight:
				texturePlayer.Texture = Turn2;
				texturePlayer.FlipH = true;
				break;
		}
	}

	public SteeringVisualState getVisualState()
	{
		bool left = Input.IsActionPressed("steer_left");
		bool right = Input.IsActionPressed("steer_right");

		if (left)
		{
			if (leftHoldTime >= hardTurnThreshold)
				return SteeringVisualState.TurnHardLeft;

			return SteeringVisualState.TurnLightLeft;
		}

		if (right)
		{
			if (rightHoldTime >= hardTurnThreshold)
				return SteeringVisualState.TurnHardRight;

			return SteeringVisualState.TurnLightRight;
		}

		return SteeringVisualState.Straight;
	}

	public async void updateSpriteSmooth(SteeringVisualState newState)
	{
		if (lastSteeringState == newState)
		{
			updateSprite(newState);
			return;
		}
		
		// Плавный поворот руля
		float targetRotation = 0f;
		if (newState == SteeringVisualState.TurnLightLeft)
			targetRotation = -0.05f;
		else if (newState == SteeringVisualState.TurnHardLeft)
			targetRotation = -0.1f;
		else if (newState == SteeringVisualState.TurnLightRight)
			targetRotation = 0.05f;
		else if (newState == SteeringVisualState.TurnHardRight)
			targetRotation = 0.1f;
		
		// Анимация поворота
		float startRotation = texturePlayer.Rotation;
		for (float t = 0; t < 0.15f; t += 0.016f)
		{
			texturePlayer.Rotation = Mathf.Lerp(startRotation, targetRotation, t / 0.15f);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		texturePlayer.Rotation = targetRotation;
		
		// Если возвращаемся в прямое положение
		if (lastSteeringState != SteeringVisualState.Straight && newState == SteeringVisualState.Straight)
		{
			// Показываем промежуточный спрайт
			if (lastSteeringState == SteeringVisualState.TurnHardLeft || 
				lastSteeringState == SteeringVisualState.TurnLightLeft)
			{
				texturePlayer.Texture = Turn1;
				texturePlayer.FlipH = false;
			}
			else
			{
				texturePlayer.Texture = Turn1;
				texturePlayer.FlipH = true;
			}
			
			await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
			updateSprite(newState);
		}
		else
		{
			updateSprite(newState);
		}
		
		lastSteeringState = newState;
	}
}
