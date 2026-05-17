using Godot;
using System.Numerics;

public partial class Player : Node2D
{
	public MainScene mainScene = null;
	public TextureRect texturePlayer = null;

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

	public override void _Ready()
	{
		base._Ready();

		texturePlayer = GetNode("Texture") as TextureRect;
	}

	public override void _Process(double delta)
	{
		float steer = Input.GetActionStrength("steer_right") - Input.GetActionStrength("steer_left");
		float steerSpeed = 1.7f; // скорость поворота
		worldPosition.X += steer * steerSpeed * (float)delta;

		worldPosition.X = Mathf.Clamp(worldPosition.X, -5f, 5f);

		texturePlayer.Rotation = -steer * 0.3f; 
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
		playerHeightOffset = screen.Y / 4;  // 1/4 от высоты экрана
		
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
}
