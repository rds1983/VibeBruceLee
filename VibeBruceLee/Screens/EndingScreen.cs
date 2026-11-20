using System.Globalization;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Input;
using VibeBruceLee.Presentation;

namespace VibeBruceLee.Screens;

public sealed class EndingScreen : IGameScreen
{
	public bool IsOverlay => false;

	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public EndingScreen(ScreenStack screens, ScreenContext context)
	{
		ArgumentNullException.ThrowIfNull(screens);
		ArgumentNullException.ThrowIfNull(context);

		_screens = screens;
		_context = context;
		_font16 = context.FontSystem.GetFont(16);
		_font8 = context.FontSystem.GetFont(8);
	}

	public void Activate()
	{
	}

	public void Deactivate()
	{
	}

	public void Update(GameTime gameTime, InputSystem input)
	{
		if (input.WasPressed(InputAction.Confirm))
		{
			_screens.Replace(new TitleScreen(_screens, _context));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight),
			null,
			Palette.Background,
			0f,
			Vector2.Zero,
			SpriteEffects.None,
			SpriteLayers.Background);

		DrawCentered(spriteBatch, _font16, "THE END", 40, Palette.Yellow);
		DrawCentered(spriteBatch, _font8, "FIRE WIZARD DESTROYED", 72, Palette.Cyan);
		DrawCentered(
			spriteBatch,
			_font8,
			"SCORE " + _context.Session.Score.ToString("D6", CultureInfo.InvariantCulture),
			92,
			Palette.White);
		DrawCentered(spriteBatch, _font8, "Enter: title", 136, Palette.White);
		DrawCentered(spriteBatch, _font8, "Esc: quit", 152, Palette.Muted);
	}

	private static void DrawCentered(SpriteBatch spriteBatch, SpriteFontBase font, string text, float y, Color color)
	{
		var size = font.MeasureString(text);
		spriteBatch.DrawString(
			font,
			text,
			new Vector2((GameInfo.VirtualWidth - size.X) / 2, y),
			color,
			layerDepth: SpriteLayers.Hud);
	}
}
