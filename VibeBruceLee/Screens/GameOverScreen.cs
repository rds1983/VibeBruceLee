using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;

namespace VibeBruceLee.Screens;

public sealed class GameOverScreen : IGameScreen
{
	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public GameOverScreen(ScreenStack screens, ScreenContext context)
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
		spriteBatch.Draw(_context.Pixel, new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight), Palette.Background);

		DrawCentered(spriteBatch, _font16, "GAME OVER", 72, Palette.Yellow);
		DrawCentered(spriteBatch, _font8, "Enter: title", 112, Palette.White);
		DrawCentered(spriteBatch, _font8, "Esc: quit", 128, Palette.Muted);
	}

	private static void DrawCentered(SpriteBatch spriteBatch, SpriteFontBase font, string text, float y, Color color)
	{
		var size = font.MeasureString(text);
		spriteBatch.DrawString(font, text, new Vector2((GameInfo.VirtualWidth - size.X) / 2, y), color);
	}
}
