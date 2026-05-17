// CityBackground.cs
using Godot;
using System;

// CityBackground.cs
using Godot;
using System;

public partial class CityBackground : Node2D
{
	[Export] public Texture2D CityTexture;
	[Export] public float ScrollSpeed = 100f;
	
	public float offsetX = 0f;
	private float textureWidth = 0f;
	private float scrollOffset = 0f;
	
	public override void _Ready()
	{
		if (CityTexture != null)
			textureWidth = CityTexture.GetWidth();
	}
	
	public override void _Draw()
	{
		if (CityTexture == null) return;
		
		Vector2 screen = GetViewport().GetVisibleRect().Size;

		float totalOffset = scrollOffset + offsetX;
		
		for (float x = -textureWidth; x <= screen.X + textureWidth; x += textureWidth)
		{
			DrawTextureRect(CityTexture, new Rect2(x + totalOffset, 0, textureWidth, screen.Y), false);
		}
	}
	
	public void UpdateScroll(float delta, float speed)
	{
		scrollOffset -= speed * ScrollSpeed * delta;
		
		while (scrollOffset <= -textureWidth)
			scrollOffset += textureWidth;
		while (scrollOffset > 0)
			scrollOffset -= textureWidth;
			
		QueueRedraw();
	}
	
	// Добавь метод для обновления сдвига при повороте
	public void UpdateOffset(float newOffset)
	{
		offsetX = newOffset;
		QueueRedraw(); // ВАЖНО: перерисовываем фон
	}
}
