using Godot;
using System;

public partial class PanelInterface : TextureRect
{
	public override void _Ready()
	{
		ScaleDashboard();
	}

	public override void _Process(double delta)
	{
	}

	private void ScaleDashboard()
	{

		Vector2 screenSize = GetViewportRect().Size;
		
		float targetWidth = screenSize.X * 1f;
		
		float originalWidth = Texture.GetWidth();
		float scale = targetWidth / originalWidth;
		
		// Применяем масштаб
		Scale = new Vector2(scale, scale);

		Position = new Vector2(
			(screenSize.X - (originalWidth * scale)) / 2,
			screenSize.Y - (Texture.GetHeight() * scale)  
		);
	}
}
