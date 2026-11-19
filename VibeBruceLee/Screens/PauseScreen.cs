using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;

namespace VibeBruceLee.Screens;

public sealed class PauseScreen : IGameScreen
{
	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public PauseScreen(ScreenStack screens, ScreenContext context)
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

	public void Update(GameTime gameTime, FrameInput input)
	{
		if (input.WasKeyPressed(Keys.P) ||
			input.WasKeyPressed(Keys.Enter) ||
			input.WasButtonPressed(Buttons.Start) ||
			input.WasButtonPressed(Buttons.A))
		{
			_screens.Pop();
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(
			_context.Pixel,
			new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight),
			new Color(0, 0, 0, 190));

		DrawCentered(spriteBatch, _font16, "PAUSED", 78, Palette.Yellow);
		DrawCentered(spriteBatch, _font8, "P: resume", 112, Palette.White);
	}

	private static void DrawCentered(SpriteBatch spriteBatch, SpriteFontBase font, string text, float y, Color color)
	{
		var size = font.MeasureString(text);
		spriteBatch.DrawString(font, text, new Vector2((GameInfo.VirtualWidth - size.X) / 2, y), color);
	}
}
