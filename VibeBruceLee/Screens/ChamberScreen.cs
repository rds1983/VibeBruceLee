using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;

namespace VibeBruceLee.Screens;

public sealed class ChamberScreen : IGameScreen
{
	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public ChamberScreen(ScreenStack screens, ScreenContext context)
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
		if (input.WasKeyPressed(Keys.P) || input.WasButtonPressed(Buttons.Start))
		{
			_screens.Push(new PauseScreen(_screens, _context));
		}
		else if (input.WasKeyPressed(Keys.G) || input.WasButtonPressed(Buttons.Y))
		{
			_screens.Push(new GameOverScreen(_screens, _context));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_context.Pixel, new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight), Palette.Background);
		spriteBatch.Draw(_context.TestCard, new Rectangle(24, 56, 128, 96), Color.White);
		spriteBatch.Draw(_context.Lantern, new Rectangle(184, 64, 48, 48), Color.White);
		spriteBatch.Draw(_context.Pixel, new Rectangle(0, 160, GameInfo.VirtualWidth, 8), Palette.Magenta);

		spriteBatch.DrawString(_font16, "CHAMBER 1", new Vector2(16, 12), Palette.Yellow);
		spriteBatch.DrawString(_font8, "placeholder scene", new Vector2(16, 36), Palette.Cyan);
		spriteBatch.DrawString(_font8, "P: pause   G: game over", new Vector2(16, 176), Palette.Muted);
	}
}
