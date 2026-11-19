using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeBruceLee.Core.Input;
using VibeBruceLee.Core.Screens;

namespace VibeBruceLee.Screens;

public sealed class TitleScreen : IGameScreen
{
	private readonly ScreenStack _screens;
	private readonly ScreenContext _context;
	private readonly SpriteFontBase _font16;
	private readonly SpriteFontBase _font8;

	public TitleScreen(ScreenStack screens, ScreenContext context)
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
			_screens.Push(new ChamberScreen(_screens, _context));
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_context.Pixel, new Rectangle(0, 0, GameInfo.VirtualWidth, GameInfo.VirtualHeight), Palette.Background);
		spriteBatch.Draw(_context.TestCard, new Rectangle(16, 56, 128, 96), Color.White);
		spriteBatch.Draw(_context.Lantern, new Rectangle(168, 72, 64, 64), Color.White);

		spriteBatch.DrawString(_font16, "VibeBruceLee", new Vector2(16, 12), Palette.Yellow);
		spriteBatch.DrawString(_font8, "screen stack online", new Vector2(16, 36), Palette.Cyan);
		spriteBatch.DrawString(_font8, "Enter: start", new Vector2(16, 160), Palette.White);
		spriteBatch.DrawString(_font8, "F11: fullscreen  Esc: quit", new Vector2(16, 176), Palette.Muted);
	}
}
