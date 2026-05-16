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
		float steerSpeed = 0.5f; // скорость поворота
		worldPosition.X += steer * steerSpeed * (float)delta;

		// Ограничение по ширине дороги (например, половина roadWidth)
		float halfRoad = mainScene.roadWidth / 2f;
		worldPosition.X = Mathf.Clamp(worldPosition.X, -halfRoad, halfRoad);

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
		worldPosition.Z += speed * (float)delta * playerSpeedModifier;

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

		// Желаемая ширина — 10% от ширины экрана
		float targetWidth = screen.X * 0.1f;
		float targetHeight = targetWidth * originalHeight / originalWidth;

		// Устанавливаем размер через Scale (проще и надёжнее)
		float scaleX = targetWidth / originalWidth;
		float scaleY = targetHeight / originalHeight;
		texturePlayer.Scale = new Godot.Vector2(scaleX, scaleY);

		// Вычисляем позицию: центр по X, низ по Y
		float posX = (screen.X - targetWidth) / 2;
		float posY = screen.Y - targetHeight - playerHeightOffset;

		// Сбрасываем привязки, чтобы позиция была абсолютной
		texturePlayer.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		texturePlayer.Position = new Godot.Vector2(posX, posY);
	}
}
